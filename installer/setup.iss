; ═══════════════════════════════════════════════════════════════════════════════
;  Nan's Hoi4 Tool  —  Inno Setup Installer Script
;  Compile with Inno Setup 6.x: https://jrsoftware.org/isinfo.php
; ═══════════════════════════════════════════════════════════════════════════════

#define AppName        "Nan's Hoi4 Tool"
#define AppVersion     "0.0.2"
#define AppPublisher   "Nanaimo_2013"
#define AppURL         "https://github.com/Nanaimo2013/Nans-hoi4-modding"
#define AppExeName     "NansHoi4Tool.exe"
#define AppId          "{{A3F8C2D1-44B7-4E9A-B6F2-1D8E3C5A7B90}"
#define BuildDir       "..\src\NansHoi4Tool\bin\Release\net8.0-windows"
#define ServerBuildDir "..\src\NansHoi4Tool.Server\bin\Release\net8.0"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases
DefaultDirName={autopf}\NansHoi4Tool
DefaultGroupName={#AppName}
AllowNoIcons=no
LicenseFile=LICENSE.txt
OutputDir=dist
OutputBaseFilename=NansHoi4Tool-{#AppVersion}-Setup
SetupIconFile=..\src\NansHoi4Tool\Resources\icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardSizePercent=120
DisableProgramGroupPage=no
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} Installer
ChangesAssociations=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";    Description: "{cm:CreateDesktopIcon}";    GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunch";   Description: "Create a &Quick Launch shortcut"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "associate";     Description: "&Associate .hoi4mod files with {#AppName}"; GroupDescription: "File associations:"; Flags: unchecked

[Files]
; Main application
Source: "{#BuildDir}\{#AppExeName}";           DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\NansHoi4Tool.dll";        DestDir: "{app}"; Flags: ignoreversion
Source: "{#ServerBuildDir}\NansHoi4Tool.Server.exe"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#BuildDir}\*.dll";                   DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "{#BuildDir}\*.json";                  DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\*.pdb";                   DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; Resources
Source: "{#BuildDir}\Resources\*";             DestDir: "{app}\Resources"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: ResourcesExist
; Runtimes (if self-contained publish is used)
Source: "{#BuildDir}\Themes\*";                 DestDir: "{app}\Themes"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#BuildDir}\runtimes\*";              DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: RuntimesExist

[Icons]
Name: "{group}\{#AppName}";                    Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}";          Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";              Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunch

[Registry]
; File association .hoi4mod
Root: HKCU; Subkey: "Software\Classes\.hoi4mod";                         ValueType: string; ValueName: ""; ValueData: "NansHoi4Tool.Project"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCU; Subkey: "Software\Classes\NansHoi4Tool.Project";              ValueType: string; ValueName: ""; ValueData: "Nan's Hoi4 Tool Project"; Flags: uninsdeletekey; Tasks: associate
Root: HKCU; Subkey: "Software\Classes\NansHoi4Tool.Project\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName},0"; Tasks: associate
Root: HKCU; Subkey: "Software\Classes\NansHoi4Tool.Project\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""; Tasks: associate
; App data in registry for uninstall tracking
Root: HKCU; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill"; Parameters: "/f /im {#AppExeName}"; Flags: runhidden

[Code]
function ServerExists: Boolean;
begin
  Result := FileExists(ExpandConstant('{#ServerBuildDir}\NansHoi4Tool.Server.exe'));
end;

function ResourcesExist: Boolean;
begin
  Result := DirExists(ExpandConstant('{#BuildDir}\Resources'));
end;

function RuntimesExist: Boolean;
begin
  Result := DirExists(ExpandConstant('{#BuildDir}\runtimes'));
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  UserDataDir: String;
begin
  if CurStep = ssPostInstall then
  begin
    UserDataDir := ExpandConstant('{localappdata}\NansHoi4Tool');
    if not DirExists(UserDataDir) then
      CreateDir(UserDataDir);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  UserDataDir: String;
  Response:    Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    UserDataDir := ExpandConstant('{localappdata}\NansHoi4Tool');
    if DirExists(UserDataDir) then
    begin
      Response := MsgBox(
        'Do you want to remove your settings and project data?' + #13#10 +
        '(' + UserDataDir + ')',
        mbConfirmation, MB_YESNO);
      if Response = IDYES then
        DelTree(UserDataDir, True, True, True);
    end;
  end;
end;
