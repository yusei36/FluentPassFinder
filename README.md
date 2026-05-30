[![Build](https://github.com/yusei36/FluentPassFinder/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/yusei36/FluentPassFinder/actions/workflows/build.yml)

## Status
⚠️ **Early Stage / Personal Project**: This project was mostly developed for personal use and has never been properly released or maintained due to limited available time. A larger refactoring is currently in progress and may be released in the future.

# FluentPassFinder
KeePass Plugin with a fluent design search window to quickly find entries and autotype or copy passwords or other fields.
Shortcut can be used to open the small search window from everywhere.

## Requirements
- [KeePass](https://keepass.info/) 2.54 or later
- Windows 10 or 11

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
![Search window](https://github.com/yusei36/FluentPassFinder/assets/15942327/ff1fd9aa-6a4d-4728-a4a0-9ec456bb1e3e)

### Entry context menu
![Entry context menu](https://github.com/yusei36/FluentPassFinder/assets/15942327/5bbf27fd-ae17-466d-9800-88e161510a1a)


## Configuration

Plugin settings can be changed via the built-in **settings panel** inside the FluentPassFinder window (gear icon), or by editing `KeePass.config.xml` directly.

After KeePass is closed for the first time after the plugin was installed, the configuration file will contain an entry like this:
(Note that this example may not be always up to date)

```xml
<Custom>
    <Item>
        <Key>FluentPassFinder</Key>
        <Value>{
  "Version": 1,
  "Search": {
    "IncludeTitleField": true,
    "IncludeUserNameField": false,
    "IncludePasswordField": false,
    "IncludeUrlField": true,
    "IncludeNotesField": true,
    "IncludeTags": true,
    "IncludeCustomFields": true,
    "IncludeProtectedCustomFields": false,
    "ExcludeExpiredEntries": true,
    "ExcludeGroupsBySearchSetting": true,
    "ResolveFieldReferences": true
  },
  "Otp": {
    "TotpPlaceholder": "{TIMEOTP}"
  },
  "Actions": {
    "Main": "OpenContextMenu",
    "Shift": "Copy_UserName",
    "Control": "Copy_Password",
    "Alt": "Copy_Totp",
    "ShowForCustomFields": true,
    "ExcludeFields": [
      "_etm_template_uuid"
    ],
    "Sorting": {
      "AutoType_UserName": 1,
      "AutoType_Password": 2,
      "AutoType_Totp": 3,
      "Copy_UserName": 101,
      "Copy_Password": 102,
      "Copy_Totp": 103
    }
  },
  "Hotkeys": {
    "CurrentScreen": "Ctrl+Alt+S",
    "PrimaryScreen": "Ctrl+Alt+F"
  },
  "Window": {
    "Width": 450,
    "Height": 400,
    "Anchor": "CenterCenter",
    "OffsetX": 0,
    "OffsetY": 0
  },
  "Behavior": {
    "PreserveLastSearch": false,
    "PreserveLastSearchTimeoutMilliseconds": 30000,
    "EscAlwaysClosesWindow": false
  },
  "Theme": "Dark"
}</Value>
    </Item>
</Custom>
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
