# Phase 04: Government, Tasks & Internal Affairs

## Objective
Deliver the verified V1 reference behaviours for this phase with original graphics and desktop-friendly interactions.

## Sprint sequence
- **04-01: Government institutions and internal dashboard**: COMPLETED. Recreated ministerial cabinet (13 ministries), candidate roster service, appointment/dismissal commands, funding adjustments, ideology reforms (5 ideologies), efficiency calculations, and desktop CabinetPanel overlay.
- **04-02: Tasks, objectives and subtasks**: COMPLETED. Implemented task definitions, data-driven loading (`content/tasks/tasks_modern.json`), principal vs secondary tasks, prerequisite task unlocking, automated evaluation against facilities and indicators, and desktop TasksPanel overlay.
- **04-03: Domestic events, approval and stability**: COMPLETED. Implemented data-driven domestic events catalog (`content/events/domestic_events.json`), emergency prompts, branching choice resolution, anti-corruption campaigns, national indicator drift calculations, and desktop EmergencyDialog overlay.

## Required phase evidence
- Updated feature parity matrix: `GOV-01`, `GOV-02`, `GOV-03`, `GOV-04`, `TSK-01`, `TSK-02`, `TSK-03`, `SEC-02`, and `EVT-01` transitioned to `Parity Passed`.
- Passing automated tests & build: `build.ps1` succeeded with 124/124 tests passing, zero compiler warnings/errors, and ContentValidator passing across 6 countries, 1 scenario, 1 map dataset, 8 tasks, and 5 domestic events.
- Architecture: Authoritative state kept strictly inside domain model; Godot UI acts as pure presentation layer.
- Save/load verified: Ideology, cabinet ministers, tasks/subtasks, claimed rewards, active event prompts, and event cooldowns verified across JSON serialization/deserialization.
