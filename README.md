# Rashtra: Sovereign Republic (Desktop Remake)

A Windows desktop, clean-room re-implementation of the geopolitical presidential strategy gameplay of *India Simulator 2 Premium*, featuring original graphics, a desktop-friendly UI, and pure .NET domain rules.

---

## Quick Navigation

- **Master Execution Plan:** [`MASTER-PLAN.md`](file:///c:/Workspace/India-Sim/MASTER-PLAN.md)
- **Authoritative Sprint State:** [`SPRINT-STATE.md`](file:///c:/Workspace/India-Sim/SPRINT-STATE.md)
- **Phase Index (Phases 00–09):** [`plans/PHASE-INDEX.md`](file:///c:/Workspace/India-Sim/plans/PHASE-INDEX.md)
- **Feature Parity Matrix:** [`docs/research/FEATURE-PARITY-MATRIX.md`](file:///c:/Workspace/India-Sim/docs/research/FEATURE-PARITY-MATRIX.md)
- **Clean-Room & Legal Policy:** [`docs/legal/CLEAN-ROOM-POLICY.md`](file:///c:/Workspace/India-Sim/docs/legal/CLEAN-ROOM-POLICY.md)
- **Asset & Data Provenance:** [`docs/legal/ASSET-AND-DATA-PROVENANCE.md`](file:///c:/Workspace/India-Sim/docs/legal/ASSET-AND-DATA-PROVENANCE.md)
- **V2 Deferred Backlog:** [`docs/V2-BACKLOG.md`](file:///c:/Workspace/India-Sim/docs/V2-BACKLOG.md)

---

## Architecture Overview

```text
src/
├── WorldState.Domain/       # Pure .NET 8 LTS: authoritative rules, clock, command pipeline
├── WorldState.Domain.Tests/ # xUnit test suite (unit tests, invariants, save round-trip)
└── WorldState.Desktop/      # Godot 4 .NET client: UI, map presentation, input, audio
content/                     # Validated JSON data definitions (countries, tasks, ministries)
tools/ContentValidator/      # Headless schema and data validator CLI
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

Or execute directly via the .NET CLI:
```powershell
# Restore & build solution in Release configuration
dotnet restore src/WorldState.sln
dotnet build src/WorldState.sln -c Release /p:TreatWarningsAsErrors=true

# Run domain unit & round-trip tests
dotnet test src/WorldState.sln -c Release --no-build

# Validate game content schemas
dotnet run --project tools/ContentValidator/ContentValidator.csproj -c Release --no-build
```
