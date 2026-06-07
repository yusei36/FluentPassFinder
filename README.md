# FluentPassFinder

[![GitHub](https://img.shields.io/badge/GitHub-yusei36%2FFluentPassFinder-black?logo=github&style=for-the-badge)](https://github.com/yusei36/FluentPassFinder) [![GitHub Release](https://img.shields.io/github/v/release/yusei36/FluentPassFinder?include_prereleases&style=for-the-badge)](https://github.com/yusei36/FluentPassFinder/releases/latest) [![GitHub Release Date](https://img.shields.io/github/release-date-pre/yusei36/FluentPassFinder?style=for-the-badge)](https://github.com/yusei36/FluentPassFinder/releases/latest)

**[Download](https://github.com/yusei36/FluentPassFinder/releases/latest)** | **[Installation](#installation)** | **[How to use](#how-to-use)** | **[Configuration](#configuration)**

A KeePass plugin with a Fluent Design search window to quickly find entries and autotype or copy passwords and other fields. A global shortcut opens the search window from anywhere.

![Search window](docs/images/search_field.png)

## Requirements
- [KeePass](https://keepass.info/) 2.54 or later
- Windows 10 or 11

A Linux/macOS port should be feasible but isn't implemented yet. If you'd like to help build and test it, see [docs/CrossPlatform.md](docs/CrossPlatform.md).

## Installation

1. Download `FluentPassFinder-<version>.zip` from the [releases page](https://github.com/yusei36/FluentPassFinder/releases/latest) and extract it.
2. Copy the `FluentPassFinderPlugin` folder into your KeePass `Plugins` folder (e.g. `C:\Program Files\KeePass Password Safe 2\Plugins\`).
3. (Re)start KeePass.
4. Open a database, then press the hotkey (default `Ctrl+Alt+S`) to launch the search window.

## How it works

The search window runs as a **separate process** from KeePass, built with [Avalonia](https://avaloniaui.net/) and FluentAvaloniaUI. The KeePass plugin hosts a named-pipe server and spawns the app; all KeePass operations (search, copy, autotype, settings) are routed back over the pipe:

```
         Global hotkey
              ↓
      FluentPassFinder.exe   (Avalonia search window, .NET 10)
              ↓  (Named pipe JSON)
    FluentPassFinder.dll     (KeePass plugin, .NET Framework 4.8)
              ↓  (KeePass API)
       KeePass Database
```

## How to use

### General shortcuts:
- Open FluentPassFinder on current screen: `Ctrl+Alt+S` (or `Alt Gr+S`)
- Open FluentPassFinder on main screen: `Ctrl+Alt+F` (or `Alt Gr+F`)
- Navigate up in list: `Arrow Up`
- Navigate down in list: `Arrow Down`

### Entry shortcuts
- Open entry context menu: `Enter`
- Copy user name: `Shift+Enter`
- Copy password: `Ctrl+Enter`
- Copy TOTP: `Alt+Enter`
- Select action in entry context menu: `Enter`


## Screenshots
### Search Window
![Search window](docs/images/search_field.png)

### Entry context menu
![Entry context menu](docs/images/context_menu.png)

### Settings
![Settings](docs/images/settings_menu.png)


## Configuration

Plugin settings can be changed via the built-in **settings panel** inside the FluentPassFinder window (gear icon). The settings are persisted as JSON under the `FluentPassFinder` key in `KeePass.config.xml`, which you can also edit directly if you prefer.

### TOTP placeholder

The **TOTP placeholder** setting controls how time-based one-time passwords are resolved. It defaults to KeePass's built-in `{TIMEOTP}`, but you can change it to the placeholder used by another TOTP plugin (for example KeePassOTP's `{KPOTP}`) so FluentPassFinder resolves codes through that plugin instead.

> [!WARNING]
> Do not set the TOTP placeholder to a counter-based OTP placeholder such as `{HMACOTP}`. It is not blocked and the code will resolve, but HOTP is counter-based: KeePass increments and saves the entry's counter every time the placeholder is resolved. FluentPassFinder resolves it just to decide whether to *offer* the TOTP action, so simply browsing entries would advance the counter (and mark the database modified) without you ever using a code, desyncing it from the server.

## Building

### Prerequisites

| Requirement | Notes |
|---|---|
| .NET 10 SDK | For the Avalonia app (`FluentPassFinder`) |
| .NET Framework 4.8 SDK | For the KeePass plugin (`FluentPassFinderPlugin`) |
| KeePass.exe (2.54+, compile reference) | Place at `build\KeePass\KeePass.exe`. If absent, the plugin falls back to the (unsigned) `KeePass` NuGet package. |

```powershell
dotnet restore
dotnet build --no-restore               # Debug build
dotnet build --no-restore -c Release    # Release build
```

After building the plugin, a post-build target copies the plugin and app into `build\KeePass\Plugins\FluentPassFinder\` for local testing.

### Packaging

`scripts\Publish-Package.ps1` produces the distributable zip (merges plugin DLLs with ILRepack, publishes the app as a single-file executable):

```powershell
.\scripts\Publish-Package.ps1                     # Release (default)
.\scripts\Publish-Package.ps1 -Configuration Debug
.\scripts\Publish-Package.ps1 -SkipBuild          # repackage without rebuilding
```

## Project structure

```
src/
  FluentPassFinderContracts    Shared DTOs, interfaces, and named-pipe IPC protocol (net48 + net10.0)
  FluentPassFinderPlugin       KeePass plugin entry point; hosts the pipe server (net48)
  FluentPassFinder             Standalone Avalonia search window; pipe client (net10.0)
scripts/
  Publish-Package.ps1          Build Release and produce the distributable zip
```

## License

Copyright © 2023 - 2026 Uwe Kögel

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.

See [LICENSE](LICENSE) for the full license text.
