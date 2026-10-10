using System.Globalization;
using FluentTlsTunnel.Core;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
namespace FluentTlsTunnel;
public sealed partial class MainWindow : Window
{
    Profile profile = Profile.Load();
    Tunnel? tunnel;
    CancellationTokenSource? cancel;
    bool loaded, busy, closing, allowClose;
    Task? connectingTask;
    readonly System.Diagnostics.Stopwatch connectedClock=new();
    readonly SemaphoreSlim stopping = new(1,1);
    bool Arabic => profile.Language == "ar" || (profile.Language == "system" && CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar");
    string T(string en,string ar) => Arabic ? ar : en;
    readonly DispatcherTimer heartbeat = new() { Interval=TimeSpan.FromSeconds(1) };
    public MainWindow()
    {
        InitializeComponent();
        SystemBackdrop = new MicaBackdrop();
        var hwnd=WinRT.Interop.WindowNative.GetWindowHandle(this);
        var appWindow=AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hwnd));
        appWindow.Resize(new Windows.Graphics.SizeInt32(820,880));
        appWindow.Closing += async (_,e) =>
        {
            if(allowClose) return;
            e.Cancel=true;
            if(closing) return;
            closing=true;
            try { await StopAsync(); }
            catch(Exception ex) { Log(ex.Message); closing=false; return; }
            allowClose=true; Close();
        };
        Host.Text=profile.Host; Port.Value=profile.Port; Sni.Text=profile.Sni; Username.Text=profile.Username;
        SocksPort.Value=profile.SocksPort; VerifyTls.IsChecked=profile.VerifyTls; Dns.Text=profile.DnsServers; UseDns.IsChecked=profile.UseDns;
        foreach(var a in DnsManager.Adapters()) Adapter.Items.Add(a);
        Adapter.SelectedItem=Adapter.Items.Cast<DnsManager.AdapterInfo>().FirstOrDefault(a=>a.Index==profile.AdapterIndex);
        Language.SelectedIndex=profile.Language switch {"en"=>1,"ar"=>2,_=>0};
        Theme.SelectedIndex=profile.Theme switch {"light"=>1,"dark"=>2,_=>0};
        loaded=true; Localize(); ProfilePanel.IsExpanded=string.IsNullOrWhiteSpace(profile.Host);
        if(DnsManager.Pending) { Status.Severity=InfoBarSeverity.Warning; Status.Message=T("A DNS backup is pending. Restore DNS before a new connection.","توجد نسخة احتياطية لإعدادات DNS. استعد الإعدادات قبل اتصال جديد."); }
        heartbeat.Tick += async (_,_) => { UpdateClock(); if(!busy && tunnel!=null && !tunnel.Connected) { Log(T("Connection lost.","انقطع الاتصال.")); try { await StopAsync(); } catch(Exception e) { Log(e.Message); } } };
        heartbeat.Start();
        var preview=Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--preview="));
        if(preview!=null) { profile.Language=preview.Split('=')[1]; Language.SelectedIndex=profile.Language=="ar"?2:1; Localize(); }
    }
    void Localize()
    {
        Root.FlowDirection=Arabic?FlowDirection.RightToLeft:FlowDirection.LeftToRight;
        Root.RequestedTheme=profile.Theme switch {"light"=>ElementTheme.Light,"dark"=>ElementTheme.Dark,_=>ElementTheme.Default};
        Subtitle.Text=T("A private path. One simple connection.","مسار خاص. اتصال بسيط.");
        ProfilePanel.Header=T("Connection profile","إعدادات الاتصال"); AdvancedPanel.Header=T("Advanced · DNS & routing","خيارات متقدمة · DNS والتوجيه");
        ConnectionTitle.Text=T("Server & authentication","الخادم والمصادقة"); Host.Header=T("Server","الخادم"); Port.Header=T("Port","المنفذ"); Sni.Header="SNI";
        Username.Header=T("SSH username","اسم مستخدم SSH"); Password.Header=T("Password (not saved)","كلمة المرور (لا تُحفظ)");
        VerifyTls.Content=T("Verify TLS certificate (recommended)","التحقق من شهادة TLS (موصى به)");
        TlsHint.Text=T("Some SSH-over-TLS services require this off. SSH host keys are still checked; disabling TLS verification weakens protection.","تتطلب بعض الخدمات تعطيل هذا الخيار. يستمر التحقق من مفتاح SSH، لكن تعطيل التحقق من TLS يقلل الحماية.");
        RoutingTitle.Text=T("Routing & DNS","التوجيه وDNS"); SocksPort.Header=T("Local SOCKS5 port","منفذ SOCKS5 المحلي");
        ProxyHint.Text=T("SOCKS5 on 127.0.0.1. Configure Firefox or another SOCKS-aware app. This is not a full-device VPN; other traffic is unchanged.","يعمل SOCKS5 على 127.0.0.1. اضبط Firefox أو تطبيقًا يدعم SOCKS. ليس شبكة VPN للجهاز بالكامل؛ لا يتغير توجيه بقية التطبيقات.");
        UseDns.Content=T("Use custom DNS while connected (administrator permission)","استخدام DNS مخصص أثناء الاتصال (يتطلب صلاحية المسؤول)");
        Adapter.Header=T("Network adapter","محول الشبكة"); Dns.Header=T("DNS server IP addresses","عناوين IP لخوادم DNS");
        DnsHint.Text=T("Changes the selected Windows adapter, not remote SOCKS DNS. The old settings are backed up and restored on disconnect. After a crash, use Restore DNS. Windows asks for administrator approval each time.","يغيّر محول ويندوز المحدد، لا DNS البعيد في SOCKS. تُحفظ الإعدادات السابقة وتُستعاد عند قطع الاتصال. بعد تعطل التطبيق، اضغط استعادة DNS. يطلب ويندوز موافقة المسؤول لكل تغيير.");
        RestoreDns.Content=T("Restore DNS","استعادة DNS"); LogsPanel.Header=T("Connection log","سجل الاتصال");
        Import.Content=T("Import config","استيراد إعدادات"); Export.Content=T("Export config","تصدير إعدادات");
        Save.Content=T("Save profile","حفظ الإعدادات"); SetState();
    }
    void SettingsChanged(object sender,SelectionChangedEventArgs e)
    {
        if(!loaded) return;
        profile.Language=(Language.SelectedItem as ComboBoxItem)?.Tag?.ToString()??"system";
        profile.Theme=(Theme.SelectedItem as ComboBoxItem)?.Tag?.ToString()??"system";
        Localize();
    }
    void SetState()
    {
        bool connected=tunnel?.Connected==true;
        HeroTitle.Text=connected?T("You're connected","أنت متصل"):busy?T("Finding your private path…","جارٍ إنشاء مسارك الخاص…"):T("Ready when you are","جاهز عندما تكون مستعدًا");
        HeroDetail.Text=connected?$"{profile.Host} · SOCKS5 127.0.0.1:{profile.SocksPort}":T("Your apps. Your server. One connection.","تطبيقاتك. خادمك. اتصال واحد.");
        ConnectLabel.Text=busy?T("Cancel","إلغاء"):connected?T("Connected · Disconnect","متصل · قطع الاتصال"):T("Connect","اتصال");
        ConnectIcon.Symbol=connected?Symbol.Accept:busy?Symbol.Cancel:Symbol.Link;
        ConnectionTime.Visibility=connected?Visibility.Visible:Visibility.Collapsed; UpdateClock();
        if(connected)
        {
            ConnectButton.Background=new SolidColorBrush(Windows.UI.Color.FromArgb(255,24,112,55));
            ConnectButton.Foreground=new SolidColorBrush(Colors.White);
        }
        else
        {
            ConnectButton.ClearValue(Control.BackgroundProperty);
            ConnectButton.ClearValue(Control.ForegroundProperty);
        }
        Status.Title=tunnel?.Connected==true?T("Connected","متصل"):busy?T("Connecting…","جارٍ الاتصال…"):T("Disconnected","غير متصل");
        Status.Severity=tunnel?.Connected==true?InfoBarSeverity.Success:InfoBarSeverity.Informational;
        foreach(var control in new Control[]{Host,Port,Sni,Username,Password,SocksPort,VerifyTls,UseDns,Adapter,Dns,Save,Import,Export,Language,Theme,RestoreDns}) control.IsEnabled=!busy && tunnel==null;
    }
    Profile ReadProfile()
    {
        profile.Host=Host.Text.Trim(); profile.Port=checked((int)Port.Value); profile.Sni=Sni.Text.Trim(); profile.Username=Username.Text.Trim();
        profile.SocksPort=checked((int)SocksPort.Value); profile.VerifyTls=VerifyTls.IsChecked==true; profile.UseDns=UseDns.IsChecked==true;
        profile.DnsServers=Dns.Text.Trim(); profile.AdapterIndex=(Adapter.SelectedItem as DnsManager.AdapterInfo)?.Index??0;
        profile.Validate(); return profile;
    }
    void Log(string message) => DispatcherQueue.TryEnqueue(()=>
    {
        if(message=="tls-ready") message=T("TLS ready.","اتصال TLS جاهز.");
        if(message=="connected") message=T("SSH ready. Local SOCKS5 is listening.","اتصال SSH جاهز. يعمل SOCKS5 المحلي.");
        Logs.Text=($"{DateTime.Now:HH:mm:ss} {message}\n"+Logs.Text);
        if(Logs.Text.Length>12000) Logs.Text=Logs.Text[..12000];
    });
    Task<bool> ConfirmKey(string endpoint,string key,bool changed)
    {
        var result=new TaskCompletionSource<bool>();
        DispatcherQueue.TryEnqueue(async ()=>
        {
            try
            {
                var dialog=new ContentDialog { XamlRoot=Root.XamlRoot, Title=changed?T("SSH host key changed","تغيّر مفتاح خادم SSH"):T("Trust this SSH server?","هل تثق بخادم SSH؟"),
                    Content=endpoint+"\n\n"+key+"\n\n"+T("Compare this fingerprint with your provider before accepting. A changed key may mean an attack or a replaced server.","تحقق من هذه البصمة لدى مزود الخدمة قبل الموافقة. قد يدل تغير المفتاح على هجوم أو استبدال الخادم."),
                    PrimaryButtonText=T("Trust key","الثقة بالمفتاح"), CloseButtonText=T("Cancel","إلغاء"), DefaultButton=ContentDialogButton.Close };
                result.TrySetResult(await dialog.ShowAsync()==ContentDialogResult.Primary);
            } catch { result.TrySetResult(false); }
        });
        return result.Task;
    }
    async void ConnectClick(object sender,RoutedEventArgs e)
    {
        if(busy) { cancel?.Cancel(); return; }
        if(tunnel!=null) { try { await StopAsync(); } catch(Exception ex) { Status.Message=ex.Message; } return; }
        try
        {
            var p=ReadProfile();
            if(DnsManager.Pending) throw new InvalidOperationException(T("Restore the pending DNS backup first.","استعد إعدادات DNS السابقة أولًا."));
            if(Password.Password.Length==0) throw new ArgumentException(T("Enter your password.","أدخل كلمة المرور."));
            p.Save(); busy=true; Status.Message=""; cancel=new(); tunnel=new(); SetState();
            var password = Password.Password;
            connectingTask=Task.Run(()=>tunnel.ConnectAsync(p,password,ConfirmKey,Log,cancel.Token));
            await connectingTask;
            cancel.Token.ThrowIfCancellationRequested();
            if(p.UseDns) await DnsManager.ApplyAsync(p.AdapterIndex,Profile.ParseDns(p.DnsServers));
            connectedClock.Restart(); ProfilePanel.IsExpanded=false; AdvancedPanel.IsExpanded=false; ShowConnectedNotification();
            Password.Password="";
            Status.Message=$"SOCKS5 · 127.0.0.1:{p.SocksPort}";
        }
        catch(Exception ex) { Log(ex.Message); try { await StopAsync(); } catch(Exception restore) { Log(restore.Message); } Status.Message=ex.Message; }
        finally { busy=false; connectingTask=null; SetState(); }
    }
    async Task StopAsync()
    {
        cancel?.Cancel();
        if(connectingTask!=null) try { await connectingTask; } catch { }
        await stopping.WaitAsync();
        try
        {
            if(tunnel!=null) { await Task.Run(async ()=>await tunnel.DisposeAsync()); tunnel=null; }
            if(DnsManager.Pending) await DnsManager.RestoreAsync();
            cancel?.Dispose(); cancel=null; connectedClock.Reset(); SetState();
        }
        finally { stopping.Release(); }
    }
    void SaveClick(object sender,RoutedEventArgs e) { try { ReadProfile().Save(); Status.Message=T("Profile saved. Passwords are never saved.","حُفظت الإعدادات. لا تُحفظ كلمات المرور."); } catch(Exception ex) { Status.Message=ex.Message; } }
    async void RestoreDnsClick(object sender,RoutedEventArgs e) { try { await DnsManager.RestoreAsync(); Status.Message=T("Previous DNS settings restored.","استُعيدت إعدادات DNS السابقة."); } catch(Exception ex) { Status.Message=ex.Message; } }
    async void ExportClick(object sender,RoutedEventArgs e)
    {
        try
        {
            var p=ReadProfile();
            var picker=new Windows.Storage.Pickers.FileSavePicker { SuggestedFileName="fluent-tls-config" };
            picker.FileTypeChoices.Add("Tunnel configuration",new List<string>{".ftlsc"});
            WinRT.Interop.InitializeWithWindow.Initialize(picker,WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file=await picker.PickSaveFileAsync(); if(file==null) return;
            // Export network settings only. Never export passwords or trust records.
            var portable=new Profile { Host=p.Host,Port=p.Port,Sni=p.Sni,Username=p.Username,SocksPort=p.SocksPort,VerifyTls=p.VerifyTls,DnsServers=p.DnsServers };
            var protection=await ExportProtection(); if(!protection.Proceed) return;
            await Windows.Storage.FileIO.WriteTextAsync(file,ConfigCrypto.Encode(portable,protection.Password));
            Status.Message=T("Config exported without passwords or trusted keys.","صُدّرت الإعدادات دون كلمات مرور أو مفاتيح موثوقة.");
        } catch(Exception ex) { Status.Message=ex.Message; }
    }
    async void ImportClick(object sender,RoutedEventArgs e)
    {
        try
        {
            var picker=new Windows.Storage.Pickers.FileOpenPicker(); picker.FileTypeFilter.Add(".ftlsc");
            WinRT.Interop.InitializeWithWindow.Initialize(picker,WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file=await picker.PickSingleFileAsync(); if(file==null) return;
            if((await file.GetBasicPropertiesAsync()).Size>65536) throw new ArgumentException(T("Config file is too large.","ملف الإعدادات كبير جدًا."));
            var content=await Windows.Storage.FileIO.ReadTextAsync(file);
            string? secret=null; if(ConfigCrypto.Encrypted(content)) {var answer=await ImportPassword(); if(answer==null) return; secret=answer;}
            var p=ConfigCrypto.Decode(content,secret);
            p.UseDns=false; p.AdapterIndex=0; p.Validate(); Profile.ParseDns(p.DnsServers);
            var dialog=new ContentDialog {XamlRoot=Root.XamlRoot,Title=T("Import these settings?","هل تريد استيراد هذه الإعدادات؟"),Content=$"{p.Host}:{p.Port}\nSNI: {p.Sni}\nSSH: {p.Username}\nSOCKS: {p.SocksPort}\nDNS: {p.DnsServers}",PrimaryButtonText=T("Import","استيراد"),CloseButtonText=T("Cancel","إلغاء"),DefaultButton=ContentDialogButton.Close};
            if(await dialog.ShowAsync()!=ContentDialogResult.Primary) return;
            Host.Text=p.Host;Port.Value=p.Port;Sni.Text=p.Sni;Username.Text=p.Username;SocksPort.Value=p.SocksPort;VerifyTls.IsChecked=p.VerifyTls;Dns.Text=p.DnsServers;UseDns.IsChecked=false;
            Password.Password="";
            Status.Message=T("Config imported. Review it before connecting. DNS changes remain off.","استُوردت الإعدادات. راجعها قبل الاتصال. يبقى تغيير DNS معطّلًا.");
        } catch(Exception ex) { Status.Message=ex.Message; }
    }

    async Task<(bool Proceed,string? Password)> ExportProtection()
    {
        var check=new CheckBox {Content=T("Encrypt with a password","التشفير بكلمة مرور")};
        var secret=new PasswordBox {Header=T("Export password (at least 8 characters)","كلمة مرور التصدير (8 أحرف على الأقل)")};
        var stack=new StackPanel {Spacing=12};stack.Children.Add(check);stack.Children.Add(secret);
        var dialog=new ContentDialog {XamlRoot=Root.XamlRoot,Title=T("Protect this config","حماية ملف الإعدادات"),Content=stack,PrimaryButtonText=T("Export","تصدير"),CloseButtonText=T("Cancel","إلغاء")};
        if(await dialog.ShowAsync()!=ContentDialogResult.Primary) return (false,null);
        if(check.IsChecked==true && secret.Password.Length<8) throw new ArgumentException(T("Use at least 8 characters for the export password.","استخدم 8 أحرف على الأقل لكلمة مرور التصدير."));
        return (true,check.IsChecked==true?secret.Password:null);
    }
    async Task<string?> ImportPassword()
    {
        var secret=new PasswordBox {Header=T("Config password","كلمة مرور ملف الإعدادات")};
        var dialog=new ContentDialog {XamlRoot=Root.XamlRoot,Title=T("Encrypted config","ملف إعدادات مشفّر"),Content=secret,PrimaryButtonText=T("Unlock","فتح"),CloseButtonText=T("Cancel","إلغاء")};
        return await dialog.ShowAsync()==ContentDialogResult.Primary?secret.Password:null;
    }

    void UpdateClock()
    {
        var elapsed=connectedClock.Elapsed;
        ConnectionTime.Text=$"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }
    void ShowConnectedNotification()
    {
        try
        {
            var notification=new Microsoft.Windows.AppNotifications.Builder.AppNotificationBuilder()
                .AddText("Fluent TLS Tunnel").AddText(T("Connected","متصل")).BuildNotification();
            Microsoft.Windows.AppNotifications.AppNotificationManager.Default.Show(notification);
        }
        catch { Log(T("Windows notifications are unavailable.","إشعارات ويندوز غير متاحة.")); }
    }

}
