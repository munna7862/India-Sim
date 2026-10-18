# Crash and Diagnostics Guide

- **Product:** Rashtra: Sovereign Republic (Windows Desktop Remake)
- **Version:** 1.0.0 (Release)
- **Engine / Runtime:** .NET 8 LTS / Godot 4.3 .NET
- **Distribution:** Standalone Windows 64-bit

---

## 1. System Requirements & Clean-Machine Setup

| Requirement | Minimum Specification | Recommended Specification |
|---|---|---|
| Operating System | Windows 10 64-bit (Version 1809+) | Windows 11 64-bit |
| Processor | Dual-Core 2.0 GHz x64 | Quad-Core 3.0 GHz x64 |
| Memory (RAM) | 4 GB | 8 GB |
| Graphics | DirectX 11 / Vulkan compatible GPU | Dedicated GPU with 2GB VRAM |
| Storage | 500 MB available local disk space | High-speed SSD |
| Network | **None required (100% Offline Single-Player)** | Optional (for updates/patches) |

---

## 2. Standard File System Locations

Rashtra stores user preferences, save files, and diagnostic crash logs under the standard Windows user profile:

- **Root Data Directory:**  
  `%APPDATA%\RashtraSovereignRepublic\` (e.g. `C:\Users\<User>\AppData\Roaming\RashtraSovereignRepublic\`)
- **Save Slots:**  
  `%APPDATA%\RashtraSovereignRepublic\saves\`  
  - Slot 1: `slot_1.json`, `slot_1.json.bak`
  - QuickSave: `quicksave.json`, `quicksave.json.bak`
  - AutoSave: `autosave.json`, `autosave.json.bak`
- **Crash & Diagnostic Logs:**  
  `%APPDATA%\RashtraSovereignRepublic\logs\`  
  - Format: `crash_YYYYMMDD_HHMMSS_<id>.log`
- **User Settings:**  
  `%APPDATA%\RashtraSovereignRepublic\settings.json`

---

## 3. Corrupted Save Recovery

Rashtra utilizes an atomic write protocol (`.tmp` write -> `.bak` backup -> `.json` commit). If an unexpected power failure, sudden restart, or OS write interruption corrupts a save file:

1. Open `%APPDATA%\RashtraSovereignRepublic\saves\`.
2. Locate the affected slot (e.g., `slot_1.json`).
3. Check if `slot_1.json.bak` exists.
4. Copy or rename `slot_1.json.bak` to `slot_1.json`.
5. Relaunch Rashtra and load the slot from the Load Game menu.
6. The built-in `DiagnosticsService.RestoreSaveFromBackup(slotId)` API can also programmatically restore corrupted saves automatically.

---

## 4. Diagnostics Collection

If you encounter an unhandled exception or unexpected behavior:

1. Look in `%APPDATA%\RashtraSovereignRepublic\logs\` for the latest `crash_*.log`.
2. The log contains:
   - Application version & build metadata
   - Windows OS and .NET CLR version
   - Complete stack trace with inner exceptions
   - Gameplay context and timestamp
3. Submit the log alongside reproduction steps to the GitHub repository issue tracker.
4. Note: Diagnostic dumps are strictly local and sanitized; no personal credentials, telemetry, or system network tokens are ever transmitted or recorded.

---

## 5. Offline Air-Gap Verification

Rashtra is designed from the ground up for full offline sovereignty:
- Zero telemetry tracking.
- Zero analytics pingbacks.
- Zero network socket or HTTP listener dependencies.
- Can run indefinitely in an air-gapped, isolated environment.
