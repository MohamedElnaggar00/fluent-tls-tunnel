Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @'
using System;
using System.Runtime.InteropServices;
public class CaptureNative {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr handle);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr handle,int attribute,out RECT rect,int size);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr handle, out RECT rect);
 public struct RECT { public int Left,Top,Right,Bottom; }
}
'@
New-Item artifacts/screenshots -ItemType Directory -Force | Out-Null
foreach($lang in @('en','ar')) {
foreach($state in @('home','profile','connected','settings','updates','update-checking','update-current','update-new','update-empty','update-error','about','about-updates','connection-actions','tray','material-mica','material-mica-alt','material-acrylic')) {
  $mode=$state;$material='mica-alt';$theme='light'
 if($state.StartsWith('material-')){$mode='settings';$material=$state.Substring(9);$theme='dark'}
 $app=Start-Process artifacts/portable/FluentTlsTunnel.exe -ArgumentList "--preview=$lang --state=$mode --material=$material --theme=$theme" -PassThru
 try {
  for($i=0;$i -lt 30;$i++) {Start-Sleep -Milliseconds 500;$app.Refresh();if($app.MainWindowHandle -ne [IntPtr]::Zero){break};if($app.HasExited){throw 'Application exited before screenshot'}}
  if($app.MainWindowHandle -eq [IntPtr]::Zero){throw 'No application window'}
  $area=[System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea
  $width=[Math]::Min(1060,$area.Width-24);$height=[Math]::Min(860,$area.Height-24)
  [CaptureNative]::SetWindowPos($app.MainWindowHandle,[IntPtr]::Zero,12,12,$width,$height,0x0040) | Out-Null
  if($state -ne 'tray'){[CaptureNative]::SetForegroundWindow($app.MainWindowHandle) | Out-Null}
  Start-Sleep -Seconds 2
  if($state -eq 'home' -or $state -eq 'connected'){
   $expected=if($state -eq 'connected'){if($lang -eq 'ar'){(-join ([char[]](0x642,0x637,0x639,0x20,0x627,0x644,0x627,0x62A,0x635,0x627,0x644)))}else{'Disconnect'}}else{if($lang -eq 'ar'){(-join ([char[]](0x627,0x62A,0x635,0x627,0x644)))}else{'Connect'}}
   $root=[System.Windows.Automation.AutomationElement]::FromHandle($app.MainWindowHandle)
   $condition=New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty,$expected)
   $element=$root.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
   if(!$element){throw "Connection action did not show $expected in $lang/$state"}
   "PASS connection action $lang/$state = $expected"|Tee-Object artifacts/button-state-tests.txt -Append
  }
  $rect=New-Object CaptureNative+RECT
  [CaptureNative]::DwmGetWindowAttribute($app.MainWindowHandle,9,[ref]$rect,16) | Out-Null
  $bitmap=New-Object System.Drawing.Bitmap(($rect.Right-$rect.Left),($rect.Bottom-$rect.Top))
  $g=[System.Drawing.Graphics]::FromImage($bitmap)
  $g.CopyFromScreen($rect.Left,$rect.Top,0,0,$bitmap.Size)
  $bitmap.Save((Join-Path (Get-Location) "artifacts/screenshots/$lang-$state.png"))
  $g.Dispose();$bitmap.Dispose()
 } finally {if(!$app.HasExited){$app.Kill();$app.WaitForExit()}}
}

}
