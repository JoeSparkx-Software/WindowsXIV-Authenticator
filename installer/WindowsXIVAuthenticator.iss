#ifndef AppVersion
  #define AppVersion "2.0.0"
#endif

#define AppName "XIV Authenticator"
#define AppPublisher "JSS Software"
#define AppExeName "WindowsXIVAuthenticator.exe"
#define AppRepoUrl "https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator"

[Setup]
AppId={{BDB68EF5-6F8F-4D1A-90FC-6FE57B66895A}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppRepoUrl}
AppSupportURL={#AppRepoUrl}
AppUpdatesURL={#AppRepoUrl}/releases
DefaultDirName={localappdata}\Programs\JSS Software\XIV Authenticator
DefaultGroupName=JSS Software\XIV Authenticator
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
OutputDir=..\dist
OutputBaseFilename=WindowsXIVAuthenticator-Setup-{#AppVersion}
SetupIconFile=assets\xivauthenticator.ico
UninstallDisplayIcon={app}\Icons\xivauthenticator.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
LicenseFile=..\LICENSE
UsePreviousAppDir=yes
UsePreviousGroup=yes
Uninstallable=yes
MinVersion=10.0.22000

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "assets\xivauthenticator.ico"; DestDir: "{app}\Icons"; Flags: ignoreversion
Source: "assets\xivlaunch.ico"; DestDir: "{app}\Icons"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\*.md"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autodesktop}\XIV Authenticator"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\Icons\xivauthenticator.ico"
Name: "{autodesktop}\Launch XIV"; Filename: "{app}\{#AppExeName}"; Parameters: "--launch"; WorkingDir: "{app}"; IconFilename: "{app}\Icons\xivlaunch.ico"

Name: "{group}\XIV Authenticator"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\Icons\xivauthenticator.ico"
Name: "{group}\Launch XIV"; Filename: "{app}\{#AppExeName}"; Parameters: "--launch"; WorkingDir: "{app}"; IconFilename: "{app}\Icons\xivlaunch.ico"
Name: "{group}\Uninstall XIV Authenticator"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch XIV Authenticator"; Flags: nowait postinstall skipifsilent

[Code]

function HasUsableTpm20(): Boolean;
var
  Locator: Variant;
  Services: Variant;
  TpmObjects: Variant;
  Tpm: Variant;
  SpecVersion: String;
begin
  Result := False;

  try
    Locator :=
      CreateOleObject(
        'WbemScripting.SWbemLocator');

    Services :=
      Locator.ConnectServer(
        '.',
        'root\CIMV2\Security\MicrosoftTpm');

    TpmObjects :=
      Services.ExecQuery(
        'SELECT * FROM Win32_Tpm');

    if TpmObjects.Count = 0 then
      Exit;

    Tpm :=
      TpmObjects.ItemIndex(0);

    if not Tpm.IsEnabled_InitialValue then
      Exit;

    SpecVersion :=
      VarToStr(
        Tpm.SpecVersion);

    if Pos(
         '2.0',
         SpecVersion) = 0 then
      Exit;

    Result := True;
  except
    Result := False;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;

  if not HasUsableTpm20() then
  begin
    MsgBox(
      'Windows XIV Authenticator 2.x requires Windows 11 with TPM 2.0 enabled and available.' +
      #13#10#13#10 +
      'This PC does not meet the supported hardware requirements for the v2 security model.' +
      #13#10#13#10 +
      'If you need compatibility with a system without TPM 2.0, install the latest Windows XIV Authenticator 1.0.x release instead.' +
      #13#10#13#10 +
      'Version 1 uses the older Windows DPAPI-based storage model and does not provide the TPM-backed v2 vault.',
      mbError,
      MB_OK);

    Result := False;
  end;
end;