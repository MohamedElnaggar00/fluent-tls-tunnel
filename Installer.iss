#define AppName "Fluent TLS Tunnel"
#define AppExe "FluentTlsTunnel.exe"
#ifndef BuildVersion
#define BuildVersion "0.1.0"
#endif
#ifndef SourceDir
#define SourceDir "artifacts\portable"
#endif
#ifndef Flavor
#define Flavor "installer"
#endif
[Setup]
AppId={{79B51A3D-2181-4CAC-83A4-68190B5DD994}
AppName={#AppName}
AppVersion={#BuildVersion}
AppPublisher=Mohamed Elnaggar
DefaultDirName={localappdata}\Programs\FluentTlsTunnel
DefaultGroupName={#AppName}
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=artifacts\packages
OutputBaseFilename=Fluent-TLS-Tunnel-{#Flavor}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=1-AppIcon.ico
DisableProgramGroupPage=yes
LicenseFile=LICENSE
CloseApplications=yes
CloseApplicationsFilter={#AppExe}
RestartApplications=yes
UninstallDisplayIcon={app}\{#AppExe}
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"
[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch Fluent TLS Tunnel"; Flags: nowait postinstall skipifsilent
[Code]
procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel2.Caption := WizardForm.WelcomeLabel2.Caption + #13#10#13#10 + 'brought to you by Instinct - https://instinct.com';
end;
function InitializeSetup: Boolean;
var Versions: TArrayOfString; I: Integer;
begin
  Result := True;
#ifdef Dependent
  Result := False;
  if RegGetValueNames(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', Versions) then
    for I := 0 to GetArrayLength(Versions)-1 do
      if Pos('8.', Versions[I]) = 1 then Result := True;
  if not Result then
    MsgBox('This build requires the .NET 8 Desktop Runtime. Install it from https://dotnet.microsoft.com/download/dotnet/8.0 or use the bundled installer.', mbError, MB_OK);
#endif
end;
