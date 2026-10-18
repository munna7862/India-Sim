# Phase 06: Diplomacy, Military & International Actions

## Objective
Deliver the verified V1 reference behaviours for this phase with original graphics and desktop-friendly interactions.

## Sprint sequence
- **06-01: Diplomacy and commercial agreements**: COMPLETED. Recreated bilateral foreign relations, diplomatic gifts/aid (Small, Medium, Large tiers), Non-Aggression Pacts with duration and mutual enforcement, and commercial export/import trade contracts integrated into the monthly fiscal and resource ledger (`SendDiplomaticGiftCommand`, `SignNonAggressionPactCommand`, `SignTradeContractCommand`, `CancelTradeContractCommand`, `DiplomacyViewModel`).
- **06-02: Military inventory, recruitment and deployment**: COMPLETED. Recreated data-driven military unit roster across 12 distinct unit types in Land, Air, and Naval branches (`content/military/military_units.json`, `MilitaryUnitRegistry`, `RecruitMilitaryUnitsCommand`, `DisbandMilitaryUnitsCommand`, monthly military unit maintenance upkeep in `ExecuteMonthlyLedgerCommand`, and `MilitaryViewModel`).
- **06-03: Conflict, espionage and security actions**: COMPLETED. Recreated state of war declarations with NAP enforcement and trade severance (`DeclareWarCommand`), deterministic offensive combat resolution with drill readiness buffs, casualties, and battle reports (`ResolveCombatCommand`), formal peace treaties (`SignPeaceTreatyCommand`), presidential protection funding (`FundPresidentialProtectionCommand`), counter-terrorism operations (`LaunchCounterTerrorismCommand`), and covert espionage ops (`DispatchCovertOperationCommand`).

## Required phase evidence
- Updated feature parity matrix with evidence and status (DIP-01..DIP-04, MIL-01..MIL-04, SEC-01..SEC-04 marked Parity Passed).
- Passing relevant automated tests and build (`165/165` tests passing in `WorldState.Domain.Tests`, 0 warnings, 0 errors).
- Clean-room content schema validation (`ContentValidator` verified 6 countries, 1 scenario, 1 map dataset, 8 tasks, 5 events, 6 policies, 12 military units).
- Godot desktop integration verified in `GameShell.cs` and `GameShell.tscn` with interactive Diplomacy (F6) and Military (F7) panels.
- Save/load round-trip persistence verified with zero state loss.
