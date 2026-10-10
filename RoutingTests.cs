using System.Net;
using System.Net.Sockets;
using FluentTlsTunnel;
using FluentTlsTunnel.Core;
if(!OperatingSystem.IsWindows())return;
if(args.Contains("--owner")){await Task.Delay(Timeout.Infinite);return;}
var socks=new TcpListener(IPAddress.Loopback,19080);socks.Start();int ipv4=0,ipv6=0,dns=0;
_=Task.Run(async()=>{while(true){var c=await socks.AcceptTcpClientAsync();_=Task.Run(async()=>{using(c)try{
var s=c.GetStream();var greet=new byte[3];await s.ReadExactlyAsync(greet);await s.WriteAsync(new byte[]{5,0});
var h=new byte[4];await s.ReadExactlyAsync(h);int n=h[3]==1?4:h[3]==4?16:await Byte(s);var target=new byte[n+2];await s.ReadExactlyAsync(target);int port=(target[^2]<<8)|target[^1];
await s.WriteAsync(new byte[]{5,0,0,1,127,0,0,1,0,0});
if(port==53){Interlocked.Increment(ref dns);var size=new byte[2];await s.ReadExactlyAsync(size);var q=new byte[(size[0]<<8)|size[1]];await s.ReadExactlyAsync(q);q[2]=0x81;q[3]=0x80;q[6]=0;q[7]=1;var answer=q.Concat(new byte[]{0xc0,0x0c,0,1,0,1,0,0,0,30,0,4,203,0,113,10}).ToArray();await s.WriteAsync(new byte[]{(byte)(answer.Length>>8),(byte)answer.Length});await s.WriteAsync(answer);}
else{if(h[3]==4)Interlocked.Increment(ref ipv6);else Interlocked.Increment(ref ipv4);var b=new byte[4096];await s.ReadAsync(b);await s.WriteAsync(System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 12\r\nConnection: close\r\n\r\nTUN-TEST-OK!"));}
}catch(Exception e){Console.WriteLine("Mock: "+e.Message);}});}});
await using var route=new WindowsRouting();
await route.StartAsync("1.1.1.1",19080,CancellationToken.None);
using var client=new HttpClient(new SocketsHttpHandler{UseProxy=false}){Timeout=TimeSpan.FromSeconds(15)};
Check(await client.GetStringAsync("http://203.0.113.10/")=="TUN-TEST-OK!","IPv4 TCP from non-proxy-aware client through TUN");
Check(await client.GetStringAsync("http://[2001:db8::10]/")=="TUN-TEST-OK!","IPv6 TCP from non-proxy-aware client through TUN");
var addresses=await Dns.GetHostAddressesAsync("fluent-routing-test.invalid");
Check(addresses.Any(a=>a.ToString()=="203.0.113.10"),"Windows DNS policy uses local SSH DNS bridge");
Check(ipv4>0&&ipv6>0&&dns>0,"SOCKS TCP paths observed");
await route.DisposeAsync();
Console.WriteLine("PASS normal routing restore; route/NRPT comparison is in PowerShell");
static void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("PASS "+label);}
static async Task<int> Byte(Stream s){var b=new byte[1];await s.ReadExactlyAsync(b);return b[0];}
