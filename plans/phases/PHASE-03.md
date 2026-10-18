# Phase 03: Core Simulation, Time & Save/Load

## Objective
Deliver the verified V1 reference behaviours for this phase with original graphics and desktop-friendly interactions.

## Sprint sequence
- **03-01: Simulation state and action pipeline**: COMPLETED (`SimulationEngine`, `GameEvent`, `GameTask`, `AdjustMinistryFundingCommand`, `ConstructFacilityCommand`, `TakeLoanCommand`, `RepayLoanCommand`, `ClaimTaskRewardCommand`; `ActionPipelineAndTasksTests.cs`).
- **03-02: Game time, timers and speed controls**: COMPLETED (Deterministic `SimulationClock`, `TimedOperation`, `CooldownTracker`, `StartTimedConstructionCommand`, `ConductArmyDrillsCommand`, monthly rollover in `SimulationEngine`; `SimulationTimeAndCadenceTests.cs`).
- **03-03: Save/load and persistence**: COMPLETED (`SaveManager`, `SaveSlotMetadata`, atomic write with `.tmp` and `.bak`, slots 1-5, QuickSave F5, QuickLoad F9, AutoSave, `MainMenu.cs` Continue integration; `SaveManagerPersistenceTests.cs`).

## Required phase evidence
- Updated feature parity matrix with evidence and status (`SH-04`, `MAP-02`, `MAP-03`, `GOV-02..03`, `TSK-01..03`, `ECO-01..04`, `MIL-02` marked Parity Passed).
- Passing relevant automated tests and build (`.\build.ps1` passed, 97/97 tests green, 0 warnings, 0 errors).
- Screenshots / UI verification of GameShell top status HUD, speed controls, and MainMenu Continue slot display.
- Updated risks, unresolved questions, and V2-only ideas.
- Human review of phase exit criteria.
