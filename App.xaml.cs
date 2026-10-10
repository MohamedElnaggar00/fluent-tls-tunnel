using Microsoft.UI.Xaml;
namespace FluentTlsTunnel;
public partial class App : Application
{
    MainWindow? window;
    public App() { InitializeComponent(); }
    protected override void OnLaunched(LaunchActivatedEventArgs args) { window = new MainWindow(); window.Activate(); }
}
