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
MinVersion=10.0.22000

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
