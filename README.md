# Rashtra: Sovereign Republic (Engine-Free Modern Remake)

A modern, engine-free, clean-room re-implementation of the geopolitical presidential strategy gameplay of *India Simulator 2 Premium*, featuring high-density vector cartography, rich dark-mode UI, procedural Web Audio, and pure domain rules.

---

## 🌐 Instant Play in Browser (Zero Download & Zero Install)

Play immediately in any desktop or mobile browser via GitHub Pages:
👉 **[Launch Rashtra Online](https://munna7862.github.io/India-Sim/)**

---

## 🎮 Standalone Windows Desktop App (< 10 MB)

**No game engines, Godot, or command prompt needed!**

1. Run **`Launch-Desktop.bat`** (or compile via `npm run package:desktop`).
2. Double-click **`release\Rashtra-Desktop-x64\Rashtra.exe`** (just 0.86 MB, uses native Windows Edge WebView2 runtime).
3. **Controls:**
   - **Panels:** `F1` (Cabinet), `F2` (Tasks), `F3` (Events), `F4` (Economy), `F5` (QuickSave), `F6` (Diplomacy), `F7` (Military), `F8` (Achievements), `F9` (QuickLoad), `Esc` (Close Panel).
   - **Simulation Speed:** `Space` (Pause/Resume), `1` (1x Normal), `2` (2x Fast), `3` (3x Ultra).
   - **World Map:** Left-click + drag to pan, mouse scroll to zoom in/out, click country for dossier.

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
├── domain/                  # Pure TypeScript domain engine: rules, clock, command pipeline
│   ├── clock/               # Deterministic 1-day/30-day cadence SimulationClock
│   ├── commands/            # 25+ domain commands (Budget, Facilities, Ministers, War, Trade)
│   ├── models/              # GameState, Country, Minister, DiplomaticStance, ResourceLedger
│   └── services/            # SaveManager, LocalStorage/Memory/File storage providers
├── ui/                      # Modern React 18 + Vanilla CSS desktop interface
│   ├── components/          # TopHUD, WorldMap (D3 SVG), BottomNavBar, DomesticEventModal
│   ├── panels/              # EconomyPanel (F4), GovernmentPanel (F1), DiplomacyPanel (F6)...
│   └── services/            # Procedural Web Audio API sound synthesizer
tests/                       # 67 Vitest tests validating domain engine mathematical parity
desktop/                     # Standalone Windows WebView2 runner (0.86 MB Rashtra.exe)
content/                     # Validated JSON data definitions (countries, tasks, events, units)
```

---

## Development & Build Commands

### Engine-Free Web & Desktop App (TypeScript / React)
```powershell
# Install Node dependencies
npm install

# Run Vitest automated test suite (67 tests, 100% domain parity)
npm test

# Launch local development server (http://localhost:3000)
npm run dev

# Build production web bundle (dist-web/)
npm run build

# Package standalone Windows desktop release (release/Rashtra-Desktop-x64/Rashtra.exe)
npm run package:desktop
```

### Pure .NET Domain & CLI Validation
```powershell
# Restore & build .NET solution
dotnet restore src/WorldState.sln
dotnet build src/WorldState.sln -c Release /p:TreatWarningsAsErrors=true

# Run .NET domain xUnit tests (218 tests)
dotnet test src/WorldState.sln -c Release --no-build

# Validate game content schemas
dotnet run --project tools/ContentValidator/ContentValidator.csproj -c Release --no-build
```

