$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type @'
using System;
using System.Runtime.InteropServices;
public class InstanceNative {
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr h);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h,int command);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
}
'@
$exe=(Resolve-Path artifacts/portable/FluentTlsTunnel.exe).Path
$app=Start-Process $exe -PassThru
try {
 for($i=0;$i -lt 40;$i++){Start-Sleep -Milliseconds 250;$app.Refresh();if($app.MainWindowHandle -ne [IntPtr]::Zero){break};if($app.HasExited){throw 'First instance exited'}}
 if($app.MainWindowHandle -eq [IntPtr]::Zero){throw 'First instance has no window'}
 foreach($state in @('visible','minimized','hidden')){
  $handle=$app.MainWindowHandle
  if($state -eq 'minimized'){[InstanceNative]::ShowWindow($handle,6)|Out-Null}
  if($state -eq 'hidden'){[InstanceNative]::ShowWindow($handle,0)|Out-Null}
  $second=Start-Process $exe -PassThru
  if(!$second.WaitForExit(10000)){throw 'Second instance did not exit'}
  if($second.ExitCode -ne 0){throw 'Second launch failed'}
  $restored=$false
  for($i=0;$i -lt 40;$i++){Start-Sleep -Milliseconds 250;$app.Refresh();if($app.HasExited){throw 'Original instance exited after second launch'};if([InstanceNative]::IsWindowVisible($handle) -and ![InstanceNative]::IsIconic($handle)){$restored=$true;break}}
  if(!$restored){throw "Existing window was not restored from $state"}
  if(@(Get-Process FluentTlsTunnel -ErrorAction SilentlyContinue).Count -ne 1){throw 'Duplicate or missing app process'}
  if([InstanceNative]::GetForegroundWindow() -ne $handle){throw 'Existing window was not focused'}
  Write-Host "Second launch from $state: one process, original window visible and focused"
 }
 $bitmap=New-Object System.Drawing.Bitmap([System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width,[System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height)
 $g=[System.Drawing.Graphics]::FromImage($bitmap);$g.CopyFromScreen(0,0,0,0,$bitmap.Size)
 New-Item artifacts/screenshots -ItemType Directory -Force|Out-Null
 $bitmap.Save((Join-Path (Get-Location) 'artifacts/screenshots/single-instance-restored.png'))
 $g.Dispose();$bitmap.Dispose()
 [InstanceNative]::ShowWindow($handle,5)|Out-Null
 $app.CloseMainWindow()|Out-Null
 if(!$app.WaitForExit(15000)){throw 'Closing left a headless app process'}
 Set-Content artifacts/single-instance-passed.txt passed
} finally {Get-Process FluentTlsTunnel -ErrorAction SilentlyContinue|Stop-Process -Force}
