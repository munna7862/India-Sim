# Sprint State

- Project: Rashtra: Sovereign Republic (Desktop Remake)
- Status: Phase 06 completed; Ready for Phase 07.
- Active phase: 06 (Diplomacy, Military & International Actions) -> COMPLETED
- Active sprint: 06-03 (Conflict, espionage and security actions) -> COMPLETED; Next: 07-01 (Achievements and progression)
- Sprint status: Completed (All 3 Phase 06 sprints completed: Diplomacy and commercial agreements, Military inventory, recruitment and deployment, Conflict, espionage and security actions; 165/165 tests green)
- Last verified commit: Phase 06 completion
- Last CI run: Local `.\build.ps1` execution passed (165/165 tests pass, ContentValidator verified 6 countries, 1 scenario, 1 map dataset, 8 tasks, 5 domestic events, 6 economic policies, 12 military units, 0 warnings/errors)

## Current objective
Transition to Phase 07 (Achievements, Events, Notifications & Progression) and execute Sprint 07-01 (Achievements and progression).

## Blockers / decisions
- Reference Game Access Limitation: Direct Android APK execution is not supported natively in this Windows agent environment without an emulator. Publicly verified developer documentation, store descriptions, and gameplay captures were synthesized into `REFERENCE-INVENTORY.md` and `FEATURE-PARITY-MATRIX.md`. A companion checklist (`docs/research/REFERENCE-RESEARCH-CHECKLIST.md`) is provided for the human owner to log live-session numerical coefficients.
- Technical Spike & Toolchain (ADR-003): .NET 8 LTS + pure domain solution + automated CI verified across Windows and Ubuntu.
- UI Design System & Navigation (Phase 01): Completed (`NavigationService`, `UserSettings`, `SettingsService`, and UI wireframes).
- Country Registry Data (Sprint 02-01): Completed (`CountryRegistry`, `CountryDefinition`, and 6 data-driven country definitions: `IND`, `USA`, `CHN`, `RUS`, `GBR`, `PAK`).
- World Map & Selection (Sprint 02-02): Completed (`MapGeometry`, `MapProjection`, `GeoJsonMapLoader`, `CountrySelectionViewModel`, Natural Earth Public Domain dataset).
- Scenario Start & Initial State (Sprint 02-03): Completed (`ScenarioDefinition`, `NewGameConfig`, `GameInitializer`, `NewGameSetupViewModel`, `CountrySelect.tscn`).
- Action Pipeline & Task Model (Sprint 03-01): Completed (`SimulationEngine`, `GameEvent`, `GameTask`, commands for funding, facilities, loans, task rewards).
- Deterministic Time & Timed Operations (Sprint 03-02): Completed (`SimulationClock` with frame-stall safeguards, `TimedOperation`, `CooldownTracker`, `ConductArmyDrillsCommand`, monthly accounting rollover).
- Versioned Persistence & Desktop Integration (Sprint 03-03): Completed (`SaveManager`, `SaveSlotMetadata`, atomic write with `.tmp` and `.bak`, slots 1-5, QuickSave F5, QuickLoad F9, AutoSave, `MainMenu` Continue integration).
- Cabinet Ministries & Ideologies (Sprint 04-01): Completed (`IdeologyDefinition`, `ChangeIdeologyCommand`, `AppointMinisterCommand`, `DismissMinisterCommand`, `MinisterRosterService`, `GovernmentViewModel`, `CabinetPanel`).
- Presidential Tasks & Objectives (Sprint 04-02): Completed (`content/tasks/tasks_modern.json`, `TaskRegistry`, `TasksViewModel`, prerequisite task unlocking, automated evaluation against facilities and indicators, `TasksPanel`).
- Domestic Events & National Indicators (Sprint 04-03): Completed (`content/events/domestic_events.json`, `DomesticEventRegistry`, `ActiveEventPrompt`, `ResolveEventChoiceCommand`, `LaunchAntiCorruptionCampaignCommand`, `UpdateNationalIndicators()`, `EmergencyDialog`).
- National Budget & Revenue (Sprint 05-01): Completed (`MonthlyBudgetReport`, `ExecuteMonthlyLedgerCommand`, `AdjustTaxRateCommand`, `TakeLoanCommand`, `RepayLoanCommand`, variable interest rates by credit rating AAA-D, sovereign debt distress alerts).
- Economic Policies & Development Actions (Sprint 05-02): Completed (`content/policies/economic_policies.json`, `EconomicPolicyRegistry`, `EnactEconomicPolicyCommand`, `RevokeEconomicPolicyCommand`, OilWells, UraniumMines, facility and population resource consumption balance).
- Economic Loop Parity & Desktop Shell (Sprint 05-03): Completed (`EconomyViewModel`, `GameShell.cs` / `GameShell.tscn` `EconomyPanel`, F4 shortcut, multi-month deterministic progression, `EconomyBudgetTests.cs` with 145/145 passing tests).
- Diplomacy & Commercial Agreements (Sprint 06-01): Completed (`Diplomacy.cs`, `SendDiplomaticGiftCommand.cs`, `SignNonAggressionPactCommand.cs`, `SignTradeContractCommand.cs`, `CancelTradeContractCommand.cs`, monthly trade flow, `DiplomacyViewModel.cs`).
- Military Inventory & Recruitment (Sprint 06-02): Completed (`content/military/military_units.json` 12 units, `MilitaryUnitRegistry.cs`, `RecruitMilitaryUnitsCommand.cs`, `DisbandMilitaryUnitsCommand.cs`, monthly military upkeep, `MilitaryViewModel.cs`).
- Conflict & Security Actions (Sprint 06-03): Completed (`DeclareWarCommand.cs`, `ResolveCombatCommand.cs` with drill bonus & homeland defense, `SignPeaceTreatyCommand.cs`, `FundPresidentialProtectionCommand.cs`, `LaunchCounterTerrorismCommand.cs`, `DispatchCovertOperationCommand.cs`, `DiplomacyMilitaryTests.cs` with 165/165 passing tests).
- Map Data Licensing (ADR-002): Natural Earth Admin 0 Public Domain vector datasets integrated under `GEO-01`.
- Branding & Clean-Room Policy: Resolved in Sprint 00-02 via `docs/legal/CLEAN-ROOM-POLICY.md` (Working title: "Rashtra: Sovereign Republic").

## Sprint ledger
| Sprint | Status | Evidence |
|---|---|---|
| 00-01 | Completed | `docs/research/REFERENCE-INVENTORY.md`, `docs/research/FEATURE-PARITY-MATRIX.md`, `docs/research/REFERENCE-RESEARCH-CHECKLIST.md` |
| 00-02 | Completed | `docs/legal/CLEAN-ROOM-POLICY.md`, `docs/legal/ASSET-AND-DATA-PROVENANCE.md`, `docs/adr/ADR-002-clean-room-and-licensing.md`, `.gitignore` |
| 00-03 | Completed | `src/WorldState.sln`, `src/WorldState.Domain/`, `src/WorldState.Domain.Tests/` (11/11 tests pass), `docs/specs/V1-PARITY-SPECIFICATION.md` |
| 01-01 | Completed | `.github/workflows/ci.yml`, `.editorconfig`, `build.ps1`, `tools/ContentValidator/`, `docs/adr/ADR-003-toolchain-and-ci.md`, `README.md` |
| 01-02 | Completed | `docs/design/DESKTOP-UI-DESIGN-SYSTEM.md`, `docs/design/UI-WIREFRAMES.md`, `src/WorldState.Domain/ViewModels/` (23/23 tests pass) |
| 01-03 | Completed | `src/WorldState.Desktop/` (Godot 4 .NET client), `src/WorldState.Domain/Navigation/`, `src/WorldState.Domain/Settings/`, `src/WorldState.Domain.Tests/NavigationAndSettingsTests.cs` (29/29 tests pass) |
| 02-01 | Completed | `src/WorldState.Domain/Data/`, `content/countries/` (6 verified countries), `src/WorldState.Domain.Tests/CountryRegistryTests.cs` (40/40 tests pass) |
| 02-02 | Completed | `src/WorldState.Domain/Map/`, `content/map/world_countries.geojson`, `src/WorldState.Domain.Tests/MapGeometryTests.cs`, `CountrySelectionViewModelTests.cs` (52/52 tests pass) |
| 02-03 | Completed | `src/WorldState.Domain/Scenarios/`, `content/scenarios/modern_2026.json`, `src/WorldState.Desktop/scenes/CountrySelect.tscn`, `NewGameSetupTests.cs` (70/70 tests pass) |
| 03-01 | Completed | `src/WorldState.Domain/Commands/`, `Models/GameEvent.cs`, `Models/GameTask.cs`, `ActionPipelineAndTasksTests.cs` (82/82 tests pass) |
| 03-02 | Completed | `SimulationClock.cs`, `TimedOperation.cs`, `Cooldowns.cs`, `ConductArmyDrillsCommand.cs`, `SimulationTimeAndCadenceTests.cs` (88/88 tests pass) |
| 03-03 | Completed | `src/WorldState.Domain/Services/SaveManager.cs`, `SaveModels.cs`, `GameShell.cs`, `MainMenu.cs`, `SaveManagerPersistenceTests.cs` (97/97 tests pass) |
| 04-01 | Completed | `Ideology.cs`, `ChangeIdeologyCommand.cs`, `MinisterRosterService.cs`, `GovernmentViewModel.cs`, `GovernmentAndCabinetTests.cs` (108/108 tests pass) |
| 04-02 | Completed | `content/tasks/tasks_modern.json`, `TaskRegistry.cs`, `TasksViewModel.cs`, `PresidentialTasksTests.cs` (115/115 tests pass) |
| 04-03 | Completed | `content/events/domestic_events.json`, `DomesticEventRegistry.cs`, `ResolveEventChoiceCommand.cs`, `DomesticEventsAndStabilityTests.cs` (124/124 tests pass) |
| 05-01 | Completed | `MonthlyBudgetReport.cs`, `ExecuteMonthlyLedgerCommand.cs`, `AdjustTaxRateCommand.cs`, `TakeLoanCommand.cs`, `RepayLoanCommand.cs`, `EconomyBudgetTests.cs` (136/136 tests pass) |
| 05-02 | Completed | `content/policies/economic_policies.json`, `EconomicPolicy.cs`, `EnactEconomicPolicyCommand.cs`, `RevokeEconomicPolicyCommand.cs`, `ContentValidator` (140/140 tests pass) |
| 05-03 | Completed | `EconomyViewModel.cs`, `GameShell.cs`, `GameShell.tscn`, `EconomyBudgetTests.cs` (145/145 tests pass, `build.ps1` green) |
| 06-01 | Completed | `Diplomacy.cs`, `SendDiplomaticGiftCommand.cs`, `SignNonAggressionPactCommand.cs`, `SignTradeContractCommand.cs`, `CancelTradeContractCommand.cs`, `DiplomacyViewModel.cs` |
| 06-02 | Completed | `content/military/military_units.json`, `MilitaryUnitRegistry.cs`, `RecruitMilitaryUnitsCommand.cs`, `DisbandMilitaryUnitsCommand.cs`, `MilitaryViewModel.cs` |
| 06-03 | Completed | `DeclareWarCommand.cs`, `ResolveCombatCommand.cs`, `SignPeaceTreatyCommand.cs`, `FundPresidentialProtectionCommand.cs`, `LaunchCounterTerrorismCommand.cs`, `DispatchCovertOperationCommand.cs`, `DiplomacyMilitaryTests.cs` (165/165 tests pass) |
| 07-01 | Ready / Next | `plans/sprints/07-01.md` |
| 07-02 | Not started | None |
| 07-03 | Not started | None |
| 08-01 | Not started | None |
| 08-02 | Not started | None |
| 08-03 | Not started | None |
| 09-01 | Not started | None |
| 09-02 | Not started | None |
| 09-03 | Not started | None |

## Update protocol
Record actual commands, test outcomes, screenshots/reports, commit/PR, unresolved issues and next action. Do not mark a sprint done unless acceptance criteria pass.
