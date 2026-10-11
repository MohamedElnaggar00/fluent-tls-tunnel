using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.Http.Headers;
using System.Text;
using FluentTlsTunnel.Core;
namespace FluentTlsTunnel;
public sealed class WindowsRouting : IAsyncDisposable
{
    Process? helper;
    string session="";
    readonly CancellationTokenSource stop=new();
    UdpClient? udp;
    TcpListener? tcp;
    int port;
    bool disposed;
    Action<string>? diagnostic;
    long dnsRequests,dnsAnswers,dnsFailures;
    DateTime nextSummary=DateTime.UtcNow;
    readonly Dictionary<string,int> linesRead=new();
    string[] resolvers=new[]{"1.1.1.1","1.0.0.1"};
    bool preferDoh;
    bool socksProbePassed;
    public void PollDiagnostics()
    {
        foreach(var name in new[]{"route-diagnostics.txt","engine-output.txt","engine-error.txt"})try{
            var path=Path.Combine(session,name);if(!File.Exists(path))continue;
            var lines=File.ReadAllLines(path);linesRead.TryGetValue(name,out var count);
            foreach(var line in lines.Skip(count).TakeLast(30))if(!string.IsNullOrWhiteSpace(line))diagnostic?.Invoke("Routing: "+name+": "+line);
            linesRead[name]=lines.Length;
        }catch(IOException){}
        if(DateTime.UtcNow>=nextSummary){nextSummary=DateTime.UtcNow.AddSeconds(15);diagnostic?.Invoke($"Routing: DNS requests={Interlocked.Read(ref dnsRequests)} answers={Interlocked.Read(ref dnsAnswers)} failures={Interlocked.Read(ref dnsFailures)} transport={(preferDoh?"HTTPS through SOCKS":"TCP through SOCKS")}");}
    }
    static byte[] TestQuery()=>new byte[]{0x46,0x54,1,0,0,1,0,0,0,0,0,0,7,101,120,97,109,112,108,101,3,99,111,109,0,0,1,0,1};
    async Task ProbeHttp(bool throughSocks,CancellationToken ct)
    {
        using var c=new TcpClient();Stream stream;
        if(throughSocks)stream=await OpenSocks(c,"1.0.0.1",80,ct);
        else{await c.ConnectAsync(IPAddress.Parse("1.0.0.1"),80,ct);stream=c.GetStream();}
        await stream.WriteAsync(Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: 1.0.0.1\r\nConnection: close\r\n\r\n"),ct);
        var data=new byte[128];int n=await stream.ReadAsync(data,ct);
        if(n<5||!Encoding.ASCII.GetString(data,0,n).StartsWith("HTTP/"))throw new IOException("No HTTP response bytes");
        diagnostic?.Invoke($"Routing: probe {(throughSocks?"SOCKS TCP":"Windows TUN TCP")} PASS ({n} response bytes)");
    }

    public bool Healthy => helper is {HasExited:false} && File.Exists(Path.Combine(session,"ready"));
    public async Task StartAsync(string endpoint,int socksPort,CancellationToken ct,string? sourceAddress=null,Action<string>? log=null,string[]? dnsServers=null)
    {
        _=IPAddress.Parse(endpoint); if(sourceAddress!=null)_=IPAddress.Parse(sourceAddress); port=socksPort;
        if(port is <1024 or >65535) throw new ArgumentException("Invalid SOCKS port");
        diagnostic=log;resolvers=dnsServers is {Length:>0}?dnsServers:new[]{"1.1.1.1","1.0.0.1"};foreach(var r in resolvers)_=IPAddress.Parse(r);
        session=Path.Combine(Profile.DataDir,"routing-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(session);
        try
        {
            using(var probe=CancellationTokenSource.CreateLinkedTokenSource(ct)){
                probe.CancelAfter(TimeSpan.FromSeconds(10));
                diagnostic?.Invoke("Routing: probe SOCKS TCP starting (1.0.0.1:80)");
                try{await ProbeHttp(true,probe.Token);socksProbePassed=true;}
                catch(Exception e) when(!ct.IsCancellationRequested){diagnostic?.Invoke("Routing: probe SOCKS TCP FAIL: "+e.GetType().Name+": "+e.Message+" (diagnostic target may be blocked)");}
            }
            using(var probe=CancellationTokenSource.CreateLinkedTokenSource(ct)){
                probe.CancelAfter(TimeSpan.FromSeconds(20));
                diagnostic?.Invoke("Routing: probe remote DNS starting");
                var answer=await Resolve(TestQuery(),probe.Token);
                if((answer[3]&15)!=0||((answer[6]<<8)|answer[7])==0)throw new IOException("Remote DNS probe returned no usable A answer");
                diagnostic?.Invoke("Routing: probe remote DNS PASS");
            }
            udp=new UdpClient(new IPEndPoint(IPAddress.Loopback,53));tcp=new TcpListener(IPAddress.Loopback,53);tcp.Start();
            _=UdpLoop();_=TcpLoop();
            var owner=Process.GetCurrentProcess();
            var script=Path.Combine(AppContext.BaseDirectory,"RoutingHelper.ps1");
            var args=$"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -Owner {owner.Id} -OwnerTicks {owner.StartTime.ToUniversalTime().Ticks} -Endpoint {endpoint} -SocksPort {port} {(sourceAddress!=null?"-SourceAddress "+sourceAddress:"")} -Session \"{session}\"";
            helper=Process.Start(new ProcessStartInfo("powershell.exe",args){UseShellExecute=true,Verb="runas",WindowStyle=ProcessWindowStyle.Hidden}) ?? throw new InvalidOperationException("Routing helper did not start");
            var deadline=DateTime.UtcNow.AddSeconds(30);
            while(!File.Exists(Path.Combine(session,"ready")))
            {
                PollDiagnostics();
                ct.ThrowIfCancellationRequested();
                if(File.Exists(Path.Combine(session,"error.txt"))) throw new InvalidOperationException(File.ReadAllText(Path.Combine(session,"error.txt")));
                if(helper.HasExited || DateTime.UtcNow>deadline) throw new InvalidOperationException("Windows routing did not become ready");
                await Task.Delay(100,ct);
            }
            PollDiagnostics();
            using(var probe=CancellationTokenSource.CreateLinkedTokenSource(ct)){
                probe.CancelAfter(TimeSpan.FromSeconds(10));
                diagnostic?.Invoke("Routing: probe Windows TUN TCP starting (1.0.0.1:80)");
                try{await ProbeHttp(false,probe.Token);}
                catch(Exception e) when(!ct.IsCancellationRequested){
                    PollDiagnostics();diagnostic?.Invoke("Routing: probe Windows TUN TCP FAIL: "+e.GetType().Name+": "+e.Message);
                    if(socksProbePassed)throw new IOException("SOCKS TCP works but Windows TUN TCP fails. Routing was restored; see engine diagnostics.",e);
                }
            }
        }
        catch {await DisposeAsync();throw;}
    }
    async Task<Stream> OpenSocks(TcpClient c,string host,int destinationPort,CancellationToken ct)
    {
        await c.ConnectAsync(IPAddress.Loopback,port,ct);var s=c.GetStream();
        await s.WriteAsync(new byte[]{5,1,0},ct);var hello=new byte[2];await s.ReadExactlyAsync(hello,ct);
        if(hello[0]!=5||hello[1]!=0)throw new IOException("SOCKS negotiation failed");
        var address=IPAddress.Parse(host);var bytes=address.GetAddressBytes();
        await s.WriteAsync(new byte[]{5,1,0,(byte)(bytes.Length==4?1:4)}.Concat(bytes).Concat(new byte[]{(byte)(destinationPort>>8),(byte)destinationPort}).ToArray(),ct);
        var head=new byte[4];await s.ReadExactlyAsync(head,ct);
        if(head[0]!=5||head[1]!=0)throw new IOException($"SOCKS CONNECT {host}:{destinationPort} rejected (reply={head[1]})");
        int length=head[3] switch {1=>4,4=>16,3=>await ReadByte(s,ct),_=>throw new IOException("Invalid SOCKS reply")};
        await s.ReadExactlyAsync(new byte[length+2],ct);return s;
    }
    async Task<byte[]> ResolveTcp(byte[] query,string resolver,CancellationToken ct)
    {
        using var c=new TcpClient();var s=await OpenSocks(c,resolver,53,ct);
        await s.WriteAsync(new byte[]{(byte)(query.Length>>8),(byte)query.Length},ct);await s.WriteAsync(query,ct);
        var size=new byte[2];await s.ReadExactlyAsync(size,ct);int n=(size[0]<<8)|size[1];if(n<12)throw new IOException("Invalid DNS answer");
        var answer=new byte[n];await s.ReadExactlyAsync(answer,ct);return answer;
    }
    async Task<byte[]> ResolveDoh(byte[] query,CancellationToken ct)
    {
        // Literal endpoint avoids bootstrap DNS. Proxy is mandatory: never fall back to the physical network.
        using var handler=new SocketsHttpHandler{Proxy=new WebProxy($"socks5://127.0.0.1:{port}"),UseProxy=true,AllowAutoRedirect=false};
        using var client=new HttpClient(handler);
        using var body=new ByteArrayContent(query);body.Headers.ContentType=new MediaTypeHeaderValue("application/dns-message");
        using var response=await client.PostAsync("https://1.1.1.1/dns-query",body,ct);response.EnsureSuccessStatusCode();
        var answer=await response.Content.ReadAsByteArrayAsync(ct);if(answer.Length<12||answer.Length>65535)throw new IOException("Invalid HTTPS DNS answer");
        // HTTP correlates the response; restore the Windows query ID.
        answer[0]=query[0];answer[1]=query[1];return answer;
    }
    async Task<byte[]> Resolve(byte[] query,CancellationToken ct)
    {
        if(query.Length<12)throw new IOException("Invalid DNS query");
        Interlocked.Increment(ref dnsRequests);
        if(preferDoh)return await ResolveDoh(query,ct);
        Exception? last=null;
        foreach(var resolver in resolvers)try{
            using var attempt=CancellationTokenSource.CreateLinkedTokenSource(ct);attempt.CancelAfter(TimeSpan.FromSeconds(4));
            var answer=await ResolveTcp(query,resolver,attempt.Token);
            if(answer[0]!=query[0]||answer[1]!=query[1]||(answer[2]&128)==0)throw new IOException("Mismatched DNS answer");
            if((answer[3]&15)==2||(answer[3]&15)==5)throw new IOException("DNS resolver returned failure or refused query");
            return answer;
        }catch(Exception e) when(!ct.IsCancellationRequested){last=e;diagnostic?.Invoke($"Routing: DNS TCP {resolver}:53 failed: {e.GetType().Name}: {e.Message}");}
        if(resolvers.All(r=>r is "1.1.1.1" or "1.0.0.1")){
            diagnostic?.Invoke("Routing: DNS trying Cloudflare HTTPS through SOCKS (port 443)");
            var answer=await ResolveDoh(query,ct);preferDoh=true;diagnostic?.Invoke("Routing: DNS HTTPS fallback PASS");return answer;
        }
        throw new IOException("All configured DNS TCP resolvers failed; no direct DNS fallback",last);
    }
    static async Task<int> ReadByte(Stream s,CancellationToken ct){var b=new byte[1];await s.ReadExactlyAsync(b,ct);return b[0];}
    async Task UdpLoop(){try{while(!stop.IsCancellationRequested){var packet=await udp!.ReceiveAsync(stop.Token);_=Respond(packet);}}catch(OperationCanceledException){}catch(ObjectDisposedException){}}
    async Task Respond(UdpReceiveResult packet){try{using var t=CancellationTokenSource.CreateLinkedTokenSource(stop.Token);t.CancelAfter(10000);var answer=await Resolve(packet.Buffer,t.Token);Interlocked.Increment(ref dnsAnswers);await udp!.SendAsync(answer,packet.RemoteEndPoint,t.Token);}catch(Exception e){Interlocked.Increment(ref dnsFailures);if(Interlocked.Read(ref dnsFailures)<=5)diagnostic?.Invoke("Routing: DNS UDP failed: "+e.Message); /* No direct DNS fallback. */ }}
    async Task TcpLoop(){try{while(!stop.IsCancellationRequested){var c=await tcp!.AcceptTcpClientAsync(stop.Token);_=RespondTcp(c);}}catch(OperationCanceledException){}catch(ObjectDisposedException){}catch(SocketException) when(stop.IsCancellationRequested){}}
    async Task RespondTcp(TcpClient c){using(c)try{using var t=CancellationTokenSource.CreateLinkedTokenSource(stop.Token);t.CancelAfter(10000);var s=c.GetStream();var size=new byte[2];await s.ReadExactlyAsync(size,t.Token);int n=(size[0]<<8)|size[1];if(n<12)return;var q=new byte[n];await s.ReadExactlyAsync(q,t.Token);var a=await Resolve(q,t.Token);Interlocked.Increment(ref dnsAnswers);await s.WriteAsync(new byte[]{(byte)(a.Length>>8),(byte)a.Length},t.Token);await s.WriteAsync(a,t.Token);}catch(Exception e){Interlocked.Increment(ref dnsFailures);if(Interlocked.Read(ref dnsFailures)<=5)diagnostic?.Invoke("Routing: DNS TCP client failed: "+e.Message);}}
    public static async Task RestoreAsync()
    {
        string dir=Path.Combine(Profile.DataDir,"restore-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        string script=Path.Combine(AppContext.BaseDirectory,"RoutingHelper.ps1");
        using var p=Process.Start(new ProcessStartInfo("powershell.exe",$"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -Owner 0 -Endpoint 127.0.0.1 -SocksPort 1080 -Session \"{dir}\""){UseShellExecute=true,Verb="runas",WindowStyle=ProcessWindowStyle.Hidden}) ?? throw new InvalidOperationException("Restore helper did not start");
        await p.WaitForExitAsync();if(!File.Exists(Path.Combine(dir,"restored")))throw new InvalidOperationException("Routing recovery failed");
    }
    public async ValueTask DisposeAsync()
    {
        if(disposed)return;
        if(helper!=null)
        {
            File.WriteAllText(Path.Combine(session,"stop"),"stop");
            using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(20));
            try{await helper.WaitForExitAsync(timeout.Token);}catch(OperationCanceledException){throw new InvalidOperationException("Routing restore is still pending. Keep the app open.");}
            if(!File.Exists(Path.Combine(session,"restored")))throw new InvalidOperationException("Routing restore failed. Keep the recovery journal.");
            helper.Dispose();helper=null;
        }
        stop.Cancel();udp?.Dispose();tcp?.Stop();disposed=true;
    }
}
