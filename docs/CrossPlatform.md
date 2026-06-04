# Cross-Platform Support (Avalonia App)

Scope and plan for making the standalone `FluentPassFinder` Avalonia app run on
Linux and macOS, not just Windows. This document covers **the app only**. The
KeePass plugin side is out of scope here: KeePass 2.x is assumed to run via Mono,
so i assume the plugin itself should already compatible. 

## Status

The app no longer targets `net10.0-windows` — it targets plain `net10.0`
(see [SizeReduction.md](SizeReduction.md) for the publish setup). Dropping the
`-windows` TFM suffix was the trivial part; it does **not** make the app
cross-platform on its own. The app is still `SelfContained` `win-x64` and depends
on Win32 P/Invoke at runtime.

All Win32 has since been isolated behind the `IPlatformServices` abstraction (see
[the abstraction section](#platform-abstraction-iplatformservices) below). The
views and view models no longer contain any native calls, so adding an OS is now a
matter of writing one new implementation. Only `WindowsPlatformServices` exists
today, so the app still runs on Windows only.

## What is already portable

These pieces use only cross-platform APIs and need no work:

- **Avalonia UI** — FluentAvalonia, `.axaml`, MVVM, `Microsoft.Extensions.DependencyInjection`
- **IPC** — `NamedPipeServerStream` / `NamedPipeClientStream` work on Linux/macOS
  (backed by Unix domain sockets); the length-prefixed JSON framing is OS-agnostic
- **Process model** — `Process.Start` plus host-PID watching
- **Settings** — plain JSON

## The Windows-bound surface

All Windows lock-in is raw Win32 P/Invoke, now concentrated in the `Platform/`
folder behind `IPlatformServices`. The functional pieces a new OS must provide:

### 1. Global hotkey registration (hardest)

Today `Platform/HotkeyRegistrar.cs` uses Win32 `RegisterHotKey` plus a message-only
window on a dedicated STA thread, wrapped by `WindowsPlatformServices`. There is no
cross-platform global-hotkey API, so this needs one implementation per OS:

- **Windows** — existing code
- **Linux / X11** — `XGrabKey` plus an X event loop
- **Linux / Wayland** — no standard mechanism; compositor-specific and often
  effectively unavailable without a desktop portal. This is a genuine **runtime
  limitation**, not just an effort question.
- **macOS** — `RegisterEventHotKey` (Carbon) or a `CGEventTap`

### 2. Foreground window + cursor

Lives in `Platform/WindowsPlatformServices.cs` (previously inline in
`SearchWindow.axaml.cs`, now moved out):

- `GetForegroundWindow` / `SetForegroundWindow` / `GetWindowThreadProcessId` /
  `AttachThreadInput` / `SetWinEventHook` — force the launcher window to the
  foreground despite focus-stealing prevention, and watch for focus leaving to
  another process. Needs X11 (`_NET_ACTIVE_WINDOW` via EWMH) and macOS
  (Accessibility API / `NSWorkspace`) equivalents.
- `GetCursorPos` — window placement near the cursor; trivial per-OS, or replace
  with Avalonia's pointer APIs.
- `DwmSetWindowAttribute` (border color) — cosmetic; no-op off Windows.

### 3. `Program.cs` — STA + platform attribute

- `[STAThread]` — a Windows COM concept; harmless elsewhere but should be
  conditionalized.
- `[assembly: SupportedOSPlatform("windows")]` — added when the TFM was switched to
  plain `net10.0` to keep the platform analyzer (CA1416) happy. Making the app
  cross-platform means removing this assembly-wide attribute and instead guarding
  each Win32 call site with `OperatingSystem.IsWindows()` (or `[SupportedOSPlatform]`
  on the Windows-only implementation class).

## Platform abstraction: `IPlatformServices`

**Done.** A platform abstraction lives in `src/FluentPassFinder/Platform/` and is
registered in DI as a singleton in `App.Init()`:

```csharp
internal interface IPlatformServices
{
    void RegisterHotkey(string name, string gesture, Action callback);
    void UnregisterHotkey(string name);
    void DisposeHotkeys();
    PixelPoint GetCursorPosition();
    void RemoveWindowBorder(IntPtr windowHandle);
    void ForceForegroundWindow(IntPtr windowHandle);
    void StartForegroundWatch(Action onForegroundChangedToOtherProcess);
}
```

- `Platform/WindowsPlatformServices.cs` — the only place Win32 now lives; holds the
  cursor / DWM / foreground P/Invoke and delegates global hotkeys to
  `Platform/HotkeyRegistrar.cs`.
- `App.Init()` constructs the implementation behind an OS-selection seam (today an
  unconditional `new WindowsPlatformServices()`) and passes it to `SearchWindow` via
  constructor injection.
- The views and view models contain no native calls; `SearchWindow` only deals with
  Avalonia-level concerns (its own window handle, visibility-state gating).

Still **to do** for an actual non-Windows build:

- Add `X11PlatformServices` / `MacPlatformServices` and branch on
  `OperatingSystem.IsWindows()` / `IsLinux()` / `IsMacOS()` in the `App.Init()` seam.
- Remove the assembly-wide `[assembly: SupportedOSPlatform("windows")]` from
  `Program.cs` and instead guard each implementation class with `[SupportedOSPlatform]`
  so the analyzer stays satisfied per-OS.

## Build / packaging changes

- App csproj: drop the hardcoded `SelfContained` `win-x64` assumption; publish
  per-RID (`win-x64`, `linux-x64`, `osx-arm64`).
- `FindAppExePath` and any spawn logic assume an `.exe` name; on Unix the
  executable has no extension.
- (Plugin-side copy steps reference `runtimes\win-x64\native` and `.exe`; out of
  scope per the note above.)

## Effort summary

| Target | Effort | Notes |
|--------|--------|-------|
| Windows path through the abstraction | **Done** | `WindowsPlatformServices`; behavior identical to pre-refactor. |
| Linux / X11 | Medium | Global hotkeys (`XGrabKey`) + foreground-window via EWMH. Best-documented non-Windows path. |
| macOS | Medium | Carbon/Cocoa hotkeys + Accessibility-based focus handling. |
| Wayland | Hard / partially blocked | No standard global hotkey; may need desktop-portal integration and still be degraded. |

## Next steps

The abstraction is in place, so the remaining work is incremental and isolated:

1. Implement `X11PlatformServices` (and later `MacPlatformServices`) against
   `IPlatformServices`, and add the OS branch in the `App.Init()` seam.
2. Swap the assembly-wide platform attribute for per-class `[SupportedOSPlatform]`
   guards (see the abstraction section).
3. Handle build/packaging: per-RID publish and the `.exe`-name assumption above.
