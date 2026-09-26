# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

A photo slideshow screensaver for Windows and macOS, built with **.NET 10 + Avalonia 12**. On Windows it is a real screensaver (`.scr`). On macOS it is a full-screen app you launch yourself, because a real macOS screensaver has to be a native `.saver` bundle, which .NET can't produce.

## Build and run

The .NET 10 SDK is installed at `/usr/local/share/dotnet`, via `brew install --cask dotnet-sdk`. It may not be on `PATH` in a non-login shell.

```bash
dotnet build PhotoCollectionSaver.sln
dotnet run --project PhotoCollectionSaver          # macOS: no args starts the slideshow
dotnet run --project PhotoCollectionSaver -- /s    # Windows-style "show" mode
dotnet publish PhotoCollectionSaver/PhotoCollectionSaver.csproj -c Release -r win-x64
```

The Windows publish works from macOS. It produces a self-contained single-file `PhotoCollectionSaver.exe` plus a copy named `PhotoCollectionSaver.scr` in `bin/Release/net10.0/win-x64/publish/`. The copy comes from the `CopyAsScreensaver` target in the csproj.

There are no tests and no lint configuration. Running the app takes over the whole screen. Any key press, click, or mouse movement of more than 5px quits it.

## Architecture

- **`Program.cs`** parses the Windows screensaver arguments: `/s` (show), `/p <hwnd>` (preview), and `/c` or `/c:<hwnd>` (configure). Arguments are case-insensitive, and `-` works as well as `/`. With no arguments, the app configures on Windows but shows the slideshow on macOS. Only show mode is implemented; preview and configure are TODOs.
- **`App.axaml.cs`** opens one `SlideshowWindow` per screen. Avalonia only exposes `Screens` through a window, so the first window is created before any screen is known.
- **`SlideshowWindow`** is a full-screen, cursorless window that cross-fades (`TransitioningContentControl`) to the next photo every 10s. Photos are decoded off the UI thread at the window's pixel height (`Bounds.Height * RenderScaling`). Only the current and previous bitmaps stay alive.
  - Pointer movement is ignored for 1s after the window opens, because going full screen can move the window under a stationary cursor.
  - `Topmost` is only set on Windows. On macOS a topmost window can't go full screen.
- **`PhotoLibrary`** holds the top-level photos in one folder and hands them out in shuffled order. All windows share one instance, so each monitor shows different photos. For now the folder is the OS Pictures folder, which will be replaced by settings. Skia can't decode HEIC.

## Platform quirks (observed on macOS)

- `Screen.Bounds` is in points, and `Screen.Scaling` reports 1 even on Retina. Use the window's `RenderScaling` for pixel sizes.
- On notched MacBooks, full-screen windows sit below the menu bar area. For example, the window is 1710×1073 at y=34 on a 1710×1107 screen.
- `Environment.SpecialFolder.MyPictures` ignores `$HOME` on macOS, so you can't redirect it for testing by overriding `HOME`.
