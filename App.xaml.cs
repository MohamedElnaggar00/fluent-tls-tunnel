using Microsoft.UI.Xaml;
namespace FluentTlsTunnel;
public partial class App : Application
{
    MainWindow? window;
    Mutex? instance;
    public App() { InitializeComponent(); }
    protected override void OnLaunched(LaunchActivatedEventArgs args) { instance=new Mutex(true,"FluentTlsTunnel.SingleInstance",out bool created); if(!created) {Environment.Exit(0); return;} window = new MainWindow(); window.Activate(); }
}
