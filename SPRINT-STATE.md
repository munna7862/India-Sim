# Sprint State

- Project: Rashtra: Sovereign Republic (Engine-Free Modern Remake)
- Status: Phase 10 started; Migrating to React + TypeScript + Vite architecture.
- Active phase: 10 (TypeScript Engine-Free Architecture & Modern Web/Desktop Migration)
- Active sprint: 10-01 (TypeScript Simulation Core & Vitest Parity Suite)
- Sprint status: In Progress (Bootstrapping Vite + TypeScript, porting domain rules, models, clock, commands, and Vitest suite)
- Last verified commit: fa14cf7 (Phase 09 V1.0.0 Golden Master Release)
- Last CI run: 218/218 .NET xUnit tests pass, Golden Master release v1.0.0 tagged and published.

## Current objective
Bootstrap the React + TypeScript + Vite workspace (`web/`), transcribe authoritative domain rules (`src/domain/`), and establish 100% test parity with Vitest matching the 218 verified domain test scenarios.

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
- Achievements and Progression (Sprint 07-01): Completed (`content/achievements/achievements.json` 8 achievements, `AchievementRegistry.cs`, `ClaimAchievementRewardCommand.cs`, `AchievementsViewModel.cs`, `AchievementsPanel`, `AchievementsProgressionTests.cs`).
- World Events & Newsfeed History (Sprint 07-02): Completed (`content/events/world_events.json` 6 world events, `WorldEventRegistry.cs`, `WorldEventDefinition.cs`, `DismissNotificationCommand.cs`, `EventHistoryViewModel.cs`, `EventsPanel`, `WorldEventsAndNotificationsTests.cs`).
- Cross-System Gameplay Parity (Sprint 07-03): Completed (`CrossSystemGameplayParityTests.cs` testing end-to-end loops across all systems, mid-simulation timed operations, emergency prompts, foreign treaties, and save/load persistence; 182/182 tests green).
- Map Skin, Palettes & Interactive Map (Sprint 08-01): Completed (`MapTheme.cs`, `MapPalette`, `MapThemeRegistry` with 3 palettes, stance-based diplomatic coloring, `WorldMapViewModel.cs`, `WorldMapControl.cs`, `WorldMapViewModelTests.cs`).
- Desktop Layouts & Information Density (Sprint 08-02): Completed (`DesktopLayoutHelper.cs`, responsive tiering, task & nation filtering, hotkeys F1-F10/Space/1-3, search bars, action confirmation dialogs, `SettingsDialog.tscn` upgrades, `DesktopLayoutAndSettingsTests.cs`).
- Visual Regression & Performance (Sprint 08-03): Completed (`MapPerformanceTests.cs` benchmark, projection caching, simulation invariants, 208/208 tests green, `build.ps1` passed).
- Parity Audit & Bug Bash (Sprint 09-01): Completed (`ParityAuditAndReleaseTests.cs`, multi-year simulation stress test, sovereign debt crisis & recovery, wartime conflict cycle, save corrupt recovery, offline air-gap audit; 34/34 features Parity Passed).
- Windows Packaging & Diagnostics (Sprint 09-02): Completed (`DiagnosticsService.cs`, `WindowsPackagingAndDiagnosticsTests.cs`, `package.ps1`, `CRASH-AND-DIAGNOSTICS-GUIDE.md`, `dist/Rashtra-v1.0.0-win-x64.zip`, `SHA256SUMS.txt`).
- Release Gate & V2 Backlog (Sprint 09-03): Completed (`RELEASE-NOTES-v1.0.0.md`, `DESKTOP-CONTROLS-GUIDE.md`, `docs/V2-BACKLOG.md` with 5 prioritized domains, all 218 tests passing, 0 warnings/errors).
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
| 07-01 | Completed | `content/achievements/achievements.json`, `AchievementRegistry.cs`, `ClaimAchievementRewardCommand.cs`, `AchievementsViewModel.cs`, `AchievementsPanel`, `AchievementsProgressionTests.cs` |
| 07-02 | Completed | `content/events/world_events.json`, `WorldEventRegistry.cs`, `DismissNotificationCommand.cs`, `EventHistoryViewModel.cs`, `EventsPanel`, `WorldEventsAndNotificationsTests.cs` |
| 07-03 | Completed | `CrossSystemGameplayParityTests.cs` (182/182 tests pass, `build.ps1` green) |
| 08-01 | Completed | `MapTheme.cs`, `MapPalette`, `WorldMapControl.cs`, `WorldMapViewModelTests.cs` |
| 08-02 | Completed | `DesktopLayoutHelper.cs`, `GameShell.cs`, `CountrySelect.cs`, `SettingsDialog.cs`, `DesktopLayoutAndSettingsTests.cs` |
| 08-03 | Completed | `MapPerformanceTests.cs`, `PHASE-08.md`, `FEATURE-PARITY-MATRIX.md`, `ASSET-AND-DATA-PROVENANCE.md` (208/208 tests pass, `build.ps1` green) |
| 09-01 | Completed | `ParityAuditAndReleaseTests.cs`, `docs/research/FEATURE-PARITY-MATRIX.md` (34/34 features Parity Passed, 214 tests pass) |
| 09-02 | Completed | `DiagnosticsService.cs`, `WindowsPackagingAndDiagnosticsTests.cs`, `package.ps1`, `CRASH-AND-DIAGNOSTICS-GUIDE.md`, `SHA256SUMS.txt` (218 tests pass) |
| 09-03 | Completed | `RELEASE-NOTES-v1.0.0.md`, `DESKTOP-CONTROLS-GUIDE.md`, `docs/V2-BACKLOG.md`, `plans/phases/PHASE-09.md` (V1.0.0 Golden Master certified) |

## Update protocol
Record actual commands, test outcomes, screenshots/reports, commit/PR, unresolved issues and next action. Do not mark a sprint done unless acceptance criteria pass.
