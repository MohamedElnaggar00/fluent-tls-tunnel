using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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
    public bool Healthy => helper is {HasExited:false} && File.Exists(Path.Combine(session,"ready"));
    public async Task StartAsync(string endpoint,int socksPort,CancellationToken ct)
    {
        _=IPAddress.Parse(endpoint); port=socksPort;
        if(port is <1024 or >65535) throw new ArgumentException("Invalid SOCKS port");
        session=Path.Combine(Profile.DataDir,"routing-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(session);
        try
        {
            udp=new UdpClient(new IPEndPoint(IPAddress.Loopback,53));tcp=new TcpListener(IPAddress.Loopback,53);tcp.Start();
            _=UdpLoop();_=TcpLoop();
            var owner=Process.GetCurrentProcess();
            var script=Path.Combine(AppContext.BaseDirectory,"RoutingHelper.ps1");
            var args=$"-NoProfile -ExecutionPolicy Bypass -File \"{script}\" -Owner {owner.Id} -OwnerTicks {owner.StartTime.ToUniversalTime().Ticks} -Endpoint {endpoint} -SocksPort {port} -Session \"{session}\"";
            helper=Process.Start(new ProcessStartInfo("powershell.exe",args){UseShellExecute=true,Verb="runas",WindowStyle=ProcessWindowStyle.Hidden}) ?? throw new InvalidOperationException("Routing helper did not start");
            var deadline=DateTime.UtcNow.AddSeconds(30);
            while(!File.Exists(Path.Combine(session,"ready")))
            {
                ct.ThrowIfCancellationRequested();
                if(File.Exists(Path.Combine(session,"error.txt"))) throw new InvalidOperationException(File.ReadAllText(Path.Combine(session,"error.txt")));
                if(helper.HasExited || DateTime.UtcNow>deadline) throw new InvalidOperationException("Windows routing did not become ready");
                await Task.Delay(100,ct);
            }
        }
        catch {await DisposeAsync();throw;}
    }
    async Task<byte[]> Resolve(byte[] query,CancellationToken ct)
    {
        using var c=new TcpClient();await c.ConnectAsync(IPAddress.Loopback,port,ct);var s=c.GetStream();
        await s.WriteAsync(new byte[]{5,1,0},ct);var hello=new byte[2];await s.ReadExactlyAsync(hello,ct);
        if(hello[0]!=5 || hello[1]!=0) throw new IOException("SOCKS negotiation failed");
        // DNS goes over TCP through the SSH tunnel, never the physical adapter.
        await s.WriteAsync(new byte[]{5,1,0,1,1,1,1,1,0,53},ct);var head=new byte[4];await s.ReadExactlyAsync(head,ct);
        if(head[0]!=5 || head[1]!=0) throw new IOException("Remote DNS unavailable");
        int length=head[3] switch {1=>4,4=>16,3=>await ReadByte(s,ct),_=>throw new IOException("Invalid SOCKS reply")};
        await s.ReadExactlyAsync(new byte[length+2],ct);
        await s.WriteAsync(new byte[]{(byte)(query.Length>>8),(byte)query.Length},ct);await s.WriteAsync(query,ct);
        var size=new byte[2];await s.ReadExactlyAsync(size,ct);int n=(size[0]<<8)|size[1];if(n<12)throw new IOException("Invalid DNS answer");
        var answer=new byte[n];await s.ReadExactlyAsync(answer,ct);return answer;
    }
    static async Task<int> ReadByte(Stream s,CancellationToken ct){var b=new byte[1];await s.ReadExactlyAsync(b,ct);return b[0];}
    async Task UdpLoop(){try{while(!stop.IsCancellationRequested){var packet=await udp!.ReceiveAsync(stop.Token);_=Respond(packet);}}catch(OperationCanceledException){}catch(ObjectDisposedException){}}
    async Task Respond(UdpReceiveResult packet){try{using var t=CancellationTokenSource.CreateLinkedTokenSource(stop.Token);t.CancelAfter(10000);var answer=await Resolve(packet.Buffer,t.Token);await udp!.SendAsync(answer,packet.RemoteEndPoint,t.Token);}catch{ /* No direct DNS fallback. */ }}
    async Task TcpLoop(){try{while(!stop.IsCancellationRequested){var c=await tcp!.AcceptTcpClientAsync(stop.Token);_=RespondTcp(c);}}catch(OperationCanceledException){}catch(ObjectDisposedException){}catch(SocketException) when(stop.IsCancellationRequested){}}
    async Task RespondTcp(TcpClient c){using(c)try{using var t=CancellationTokenSource.CreateLinkedTokenSource(stop.Token);t.CancelAfter(10000);var s=c.GetStream();var size=new byte[2];await s.ReadExactlyAsync(size,t.Token);int n=(size[0]<<8)|size[1];if(n<12)return;var q=new byte[n];await s.ReadExactlyAsync(q,t.Token);var a=await Resolve(q,t.Token);await s.WriteAsync(new byte[]{(byte)(a.Length>>8),(byte)a.Length},t.Token);await s.WriteAsync(a,t.Token);}catch{}}
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
