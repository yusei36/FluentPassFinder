# FluentPassFinder
KeePass Plugin with a fluent design search window to quickly find entries and autotype or copy passwords or other fields.
Shortcut can be used to open the small search window from everywhere.

## Requirements
- KeePass2 Version 2.54 or never
- .NET Framework 4.8

## How to use

### General shortcuts:
- Open FluentPassFinder on current screen: `Control + Alt + S` or `Alt Gr + S`
- Open FluentPassFinder on main screen: `Control + Alt + F` or `Alt Gr + F`
- Navigate up in list: `Arrow Up`
- Navigate down in list `Arrow Down`

### Entry shortcuts
- Open entry context menu: `Enter`
- Copy user name: `Shift + Enter`
- Copy password: `Control + Enter`
- Copy TOTP: `Alt + Enter`
- Select action in entry context menu: `Enter`

## Screenshots
//TODO

## Configuration
WARNING: The configuration may undergo changes that are not compatible with the current naming and format until the settings window is implemented.

Plugin can currently only be configured within the `KeePass.config.xml` configuration, but a settings window will be implemented later.
After KeePass is closed for the first time after the plugin was installed the configuration file should contain the configuration for the FluentPassFinderPlugin.
```
<Custom>
    <Item>
		<Key>FluentPassFinderPlugin</Key>
		<Value>{
  "SearchOptions": {
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
  "PluginTotpPlaceholder": "{TOTP}",
  "PluginTotpFieldConfig": "totpsettings_stringname",
  "MainAction": "OpenContextMenu",
  "ShiftAction": "Copy_UserName",
  "ControlAction": "Copy_Password",
  "AltAction": "Copy_Totp",
  "ActionSorting": {
    "AutoType_UserName": 1,
    "AutoType_Password": 2,
    "AutoType_Totp": 3,
    "Copy_UserName": 101,
    "Copy_Password": 102,
    "Copy_Totp": 103
  },
  "ShowActionsForCustomFields": true,
  "ExcludeActionsForFields": [
    "_etm_template_uuid"
  ],
  "GlobalHotkeyCurrentScreen": "Ctrl+Alt+S",
  "GlobalHotkeyPrimaryScreen": "Ctrl+Alt+F",
  "Theme": "Dark"
}</Value>
	</Item>
</Custom>
```

## License
- License GPL-3.0: [LICENSE](./LICENSE)
- Third Party Licenses: [ThirdPartyNotices.md](./ThirdPartyNotices.md)