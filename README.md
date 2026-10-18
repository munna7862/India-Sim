# Rashtra: Sovereign Republic (Desktop Remake)

A Windows desktop, clean-room re-implementation of the geopolitical presidential strategy gameplay of *India Simulator 2 Premium*, featuring original graphics, a desktop-friendly UI, and pure .NET domain rules.

---

## 🎮 Quick Start: How to Play (Standalone Windows)

**No game engines, SDKs, or installations required!**

1. **Download:** Go to [GitHub Releases (v1.0.0)](https://github.com/munna7862/India-Sim/releases/tag/v1.0.0) and download **`Rashtra-v1.0.0-win-x64.zip`**.
2. **Extract:** Right-click the `.zip` file and select **Extract All...** (to any folder on your PC).
3. **Launch:** Open the extracted folder and double-click **`Rashtra.exe`** (or **`Play-Rashtra.bat`**).
4. **Controls:**
   - **Panels:** `F1` (Cabinet), `F2` (Tasks), `F3` (Events), `F4` (Economy), `F5` (QuickSave), `F6` (Diplomacy), `F7` (Military), `F8` (Achievements), `F9` (QuickLoad), `F10` (Settings).
   - **Simulation Speed:** `Space` (Pause/Resume), `1` (1x), `2` (2x), `3` (3x).
   - **World Map:** Left-click + drag to pan, mouse scroll to zoom.

---

## Quick Navigation

- **Master Execution Plan:** [`MASTER-PLAN.md`](file:///c:/Workspace/India-Sim/MASTER-PLAN.md)
- **Authoritative Sprint State:** [`SPRINT-STATE.md`](file:///c:/Workspace/India-Sim/SPRINT-STATE.md)
- **Phase Index (Phases 00–09):** [`plans/PHASE-INDEX.md`](file:///c:/Workspace/India-Sim/plans/PHASE-INDEX.md)
- **Feature Parity Matrix:** [`docs/research/FEATURE-PARITY-MATRIX.md`](file:///c:/Workspace/India-Sim/docs/research/FEATURE-PARITY-MATRIX.md)
- **Clean-Room & Legal Policy:** [`docs/legal/CLEAN-ROOM-POLICY.md`](file:///c:/Workspace/India-Sim/docs/legal/CLEAN-ROOM-POLICY.md)
- **Release Notes (v1.0.0):** [`RELEASE-NOTES-v1.0.0.md`](file:///c:/Workspace/India-Sim/RELEASE-NOTES-v1.0.0.md)
- **Desktop Controls Guide:** [`docs/guides/DESKTOP-CONTROLS-GUIDE.md`](file:///c:/Workspace/India-Sim/docs/guides/DESKTOP-CONTROLS-GUIDE.md)
- **Crash & Diagnostics Guide:** [`docs/guides/CRASH-AND-DIAGNOSTICS-GUIDE.md`](file:///c:/Workspace/India-Sim/docs/guides/CRASH-AND-DIAGNOSTICS-GUIDE.md)
- **Asset & Data Provenance:** [`docs/legal/ASSET-AND-DATA-PROVENANCE.md`](file:///c:/Workspace/India-Sim/docs/legal/ASSET-AND-DATA-PROVENANCE.md)
- **V2 Deferred Backlog:** [`docs/V2-BACKLOG.md`](file:///c:/Workspace/India-Sim/docs/V2-BACKLOG.md)

---

## Architecture Overview

```text
src/
├── WorldState.Domain/       # Pure .NET 8 LTS: authoritative rules, clock, command pipeline
├── WorldState.Domain.Tests/ # xUnit test suite (218 tests: unit, integration, benchmarks)
└── WorldState.Desktop/      # Godot 4 .NET client: UI, map presentation, input, audio
content/                     # Validated JSON data definitions (countries, tasks, events, units)
tools/ContentValidator/      # Headless schema and data validator CLI
dist/                        # Packaged Windows desktop standalone distributions (.zip & SHA256)
```

---

## Clean-Checkout Build & Test (Windows)

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (LTS)
- Windows 10/11 (x64)
- (Optional for client) [Godot 4.3 .NET](https://godotengine.org/download/windows/)

### Build & Run Tests (One Command)
Run the automated build script from the repository root:
```powershell
.\build.ps1
```

### Windows Desktop Packaging
To create a standalone redistributable package with versioning, SHA256 checksums, and documentation:
```powershell
.\package.ps1 -Configuration Release -Version 1.0.0
```

### Direct .NET CLI Execution
```powershell
# Restore & build solution in Release configuration
dotnet restore src/WorldState.sln
dotnet build src/WorldState.sln -c Release /p:TreatWarningsAsErrors=true

# Run domain unit & round-trip tests
dotnet test src/WorldState.sln -c Release --no-build

# Validate game content schemas
dotnet run --project tools/ContentValidator/ContentValidator.csproj -c Release --no-build
```
