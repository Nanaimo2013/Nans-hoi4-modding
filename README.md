# Nan's HOI4 Modding Tool

A modern WPF desktop application for creating and managing Hearts of Iron IV mods with real-time collaboration.

<div align="center">

### The ultimate modding suite for Hearts of Iron IV

<pre>
███╗   ██╗ █████╗ ███╗   ██╗███████╗           ███████╗████████╗██╗   ██╗██████╗ ██╗ ██████╗ ███████╗  
████╗  ██║██╔══██╗████╗  ██║██╔════╝           ██╔════╝╚══██╔══╝██║   ██║██╔══██╗██║██╔═══██╗██╔════╝  
██╔██╗ ██║███████║██╔██╗ ██║███████╗           ███████╗   ██║   ██║   ██║██║  ██║██║██║   ██║███████╗  
██║╚██╗██║██╔══██║██║╚██╗██║╚════██║           ╚════██║   ██║   ██║   ██║██║  ██║██║██║   ██║╚════██║  
██║ ╚████║██║  ██║██║ ╚████║███████║           ███████║   ██║   ╚██████╔╝██████╔╝██║╚██████╔╝███████║  
╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝           ╚══════╝   ╚═╝    ╚═════╝ ╚═════╝ ╚═╝ ╚═════╝ ╚══════╝  
██╗  ██╗ ██████╗ ██╗██╗  ██╗███╗   ███╗ ██████╗ ██████╗         ████████╗ ██████╗  ██████╗ ██╗                  
██║  ██║██╔═══██╗██║██║  ██║████╗ ████║██╔═══██╗██╔══██╗        ╚══██╔══╝██╔═══██╗██╔═══██╗██║                  
███████║██║   ██║██║███████║██╔████╔██║██║   ██║██║  ██║           ██║   ██║   ██║██║   ██║██║                  
██╔══██║██║   ██║██║╚════██║██║╚██╔╝██║██║   ██║██║  ██║           ██║   ██║   ██║██║   ██║██║                  
██║  ██║╚██████╔╝██║     ██║██║ ╚═╝ ██║╚██████╔╝██████╔╝           ██║   ╚██████╔╝╚██████╔╝███████╗             
╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝ ╚═════╝ ╚═════╝           ╚═╝    ╚═════╝  ╚═════╝ ╚══════╝             
                                                        
</pre>

<br/>

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg?style=for-the-badge)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4.svg?style=for-the-badge)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg?style=for-the-badge)](LICENSE)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-green.svg?style=for-the-badge)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![MaterialDesign](https://img.shields.io/badge/MaterialDesign-UI-757575.svg?style=for-the-badge)](http://materialdesigninxaml.net/)
[![Version](https://img.shields.io/badge/VERSION-1.0.0-blue.svg?style=for-the-badge)](https://github.com/Nanaimo2013/Nans-hoi4-modding/releases)

<br/>

[🚀 Getting Started](#-getting-started) •
[✨ Features](#-features) •
[🏗️ Building](#-building) •
[👥 Collaboration](#-collaboration) •
[📖 Documentation](#-documentation) •
[🐛 Troubleshooting](#-troubleshooting)

</div>

<br/>

## ✨ Features

<table>
<tr>
<td>

### 🗺️ Visual Editors
- **Focus Tree Editor** — Drag-and-drop canvas with node connections
- **Event Editor** — Card-based list with option chains
- **Decision Editor** — Categories and decision management
- **Technology Editor** — Visual tech tree with prerequisites

</td>
<td>

### 🏛️ Content Management
- **National Spirits** — Ideas and modifiers editor
- **Country Editor** — Identity, flag, ideology, starting setup
- **Localisation** — Multi-language DataGrid editor
- **Units & Equipment** — Unit definitions and stats

</td>
</tr>
<tr>
<td>

### 🤝 Real-time Collaboration
- **LAN Multiplayer** — Host/join sessions via SignalR
- **Entity Locking** — Prevents concurrent edits
- **Live Chat** — Built-in collaboration chat
- **Presence Indicators** — See who's online

</td>
<td>

### ⚙️ Quality of Life
- **Auto-save** — Configurable interval backups
- **Version History** — Snapshots and restore points
- **Discord Rich Presence** — Show off your modding
- **Dark/Light Themes** — 20+ accent colors

</td>
</tr>
</table>

### 📦 Project Management
- **`.h4proj` Files** — Self-contained ZIP + JSON format
- **Import/Export** — Bring in existing HOI4 mods
- **Mod Export** — Ready-to-play HOI4 mod folders
- **Auto-updater** — GitHub Releases integration

---

## 📂 Project Structure

```
Nans-hoi4-modding/
├── src/
│   ├── NansHoi4Tool/              # WPF Client (.NET 8 Windows)
│   │   ├── Controls/              # NavButton, NotificationOverlay
│   │   ├── Helpers/               # Converters, FocusGridConverter
│   │   ├── Models/                # AppSettings, Project models
│   │   ├── Services/              # Core services (AutoUpdate, Discord, etc.)
│   │   ├── ViewModels/            # MVVM ViewModels (CommunityToolkit)
│   │   └── Views/                 # XAML Views + Editor pages
│   │
│   ├── NansHoi4Tool.Shared/       # Shared models & contracts
│   │   ├── Collaboration/           # SignalR DTOs
│   │   └── Project/               # ProjectMetadata, Serialization
│   │
│   └── NansHoi4Tool.Server/       # ASP.NET Core SignalR Server
│       ├── Hubs/
│       │   └── ModProjectHub.cs   # Real-time collaboration hub
│       └── Program.cs
│
├── installer/
│   └── setup.iss                  # Inno Setup installer script
│
└── dist/                          # Build outputs
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) (with C# extension)
- Windows 10/11
- Hearts of Iron IV (for testing mods)

### Quick Start

```powershell
# Clone the repository
git clone https://github.com/Nanaimo2013/Nans-hoi4-modding.git
cd Nans-hoi4-modding

# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the client
dotnet run --project src/NansHoi4Tool/NansHoi4Tool.csproj
```

### Installation (End Users)

Download the latest installer from [Releases](https://github.com/Nanaimo2013/Nans-hoi4-modding/releases):

1. Download `NansHoi4Tool-Setup.exe`
2. Run the installer
3. Launch from Start Menu or Desktop shortcut
4. The tool auto-checks for updates on startup

---

## 🏗️ Building

### Build Commands

```powershell
# Full solution build
dotnet build NansHoi4Tool.sln -c Release

# Build just the client
dotnet build src/NansHoi4Tool/NansHoi4Tool.csproj -c Release

# Build the collaboration server
dotnet build src/NansHoi4Tool.Server/NansHoi4Tool.Server.csproj -c Release
```

### Creating the Installer

```powershell
# Build Release first, then run Inno Setup
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\setup.iss
```

Output: `dist\NansHoi4Tool-1.0.0-Setup.exe`

---

## 👥 Collaboration

Work together on mods in real-time over your local network!

### Hosting a Session

1. Open your project
2. Go to **Collaboration** → **Host Session**
3. Share your IP address with friends
4. The server runs on port **51420** by default

### Joining a Session

1. Go to **Collaboration** → **Join Session**
2. Enter the host's IP address and port
3. Start collaborating!

### Collaboration Features

| Feature | Description |
|---------|-------------|
| 🔔 Entity Locking | Prevents two users editing the same focus/event simultaneously |
| 💬 Live Chat | Built-in chat panel for coordination |
| 👤 Presence | See who's online and what they're doing |
| 📡 Auto-Host | Server launches automatically when hosting |

---

## ⚙️ Configuration

Settings are stored at:
```
%AppData%\NansHoi4Tool\settings.json
```

| Setting | Default | Description |
|---------|---------|-------------|
| Theme | `Dark` | UI theme (Dark/Light) |
| Accent | `Indigo` | Primary accent color |
| Auto-save | `60s` | Auto-save interval |
| Collab Port | `51420` | SignalR server port |
| Discord RPC | `Enabled` | Rich Presence integration |

---

## 🐛 Troubleshooting

### Build Errors

**Error: `The tag 'XXX' does not exist in XML namespace`**
- Ensure all NuGet packages are restored: `dotnet restore`

**Error: `MaterialDesign resources not found`**
- Check that `MaterialDesignThemes.MahApps` is installed

### Runtime Issues

**Collaboration connection fails**
- Verify port 51420 is not blocked by firewall
- Check that the server is running on the host machine
- Ensure both machines are on the same network

**Discord Rich Presence not showing**
- Set your Client ID in `Services/DiscordService.cs`
- Register a Discord application at [Discord Developer Portal](https://discord.com/developers/applications)

**Export mod not appearing in HOI4**
- Ensure the export path points to your `Documents\Paradox Interactive\Hearts of Iron IV\mod` folder
- Check that both `.mod` file and mod folder are created

---

## 📸 Screenshots

> *(Screenshots coming soon — add them to a `docs/screenshots/` folder and reference here)*

| Focus Tree Editor | Collaboration Panel |
|-------------------|---------------------|
| ![Focus Tree](docs/screenshots/focus-tree.png) | ![Collab](docs/screenshots/collab.png) |

---

## 🗺️ Roadmap

- [x] Modern UI overhaul (Material Design)
- [x] Auto-updater with GitHub Releases
- [x] Real-time collaboration via SignalR
- [x] Inno Setup installer
- [ ] HOI4 Clausewitz script parser (import vanilla files)
- [ ] Map view for state/territory editing
- [ ] Steam Workshop integration
- [ ] AI-assisted content generation
- [ ] Multi-language UI support

---

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

---

## 📜 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

Hearts of Iron IV is a trademark of Paradox Interactive. This tool is an unofficial community project.

---

## 🙏 Acknowledgements

- [MahApps.Metro](https://mahapps.com/) — Modern UI toolkit for WPF
- [Material Design in XAML](http://materialdesigninxaml.net/) — Material Design components
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction) — MVVM support
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) — Real-time communication
- [Paradox Interactive](https://www.paradoxinteractive.com/) — For creating Hearts of Iron IV

---

<div align="center">

**[📖 Documentation](docs/)** •
**[🐛 Report Bug](https://github.com/Nanaimo2013/Nans-hoi4-modding/issues)** •
**[💡 Request Feature](https://github.com/Nanaimo2013/Nans-hoi4-modding/issues)**

<br/>

Made with ❤️ by [Nanaimo2013](https://github.com/Nanaimo2013)

</div>
```
