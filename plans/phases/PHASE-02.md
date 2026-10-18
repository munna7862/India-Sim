# Phase 02: World Map, Country Selection & Scenario Start

## Objective
Deliver the verified V1 reference behaviours for this phase with original graphics and desktop-friendly interactions.

## Status: COMPLETED

## Sprint sequence
- [x] **02-01: Country registry and start-state data**: Create the data model required to start a game as an available country. (Completed: `CountryDefinition`, `CountryRegistry`, 6 country datasets, 40 tests).
- [x] **02-02: World map interaction and selection**: Build a readable map with country selection, hover and inspection. (Completed: `MapGeometry`, `MapProjection`, `GeoJsonMapLoader`, `CountrySelectionViewModel`, `world_countries.geojson`, 52 tests).
- [x] **02-03: New game setup and initial world state**: Implement the scenario start path and ensure repeatable initialization. (Completed: `ScenarioDefinition`, `NewGameConfig`, `GameInitializer`, `NewGameSetupViewModel`, `CountrySelect.tscn`, 70 tests).

## Phase evidence summary
- **Automated Tests:** 70/70 domain unit, integration, and round-trip tests passing.
- **Content Validation:** 6 country definitions, 1 scenario (`modern_2026.json`), and 1 map dataset (`world_countries.geojson`) fully verified with 0 errors.
- **Client Flow:** Complete desktop flow from MainMenu -> CountrySelect (with search and inspection) -> GameShell (with HUD and map title) -> MainMenu.
- **Licensing & Provenance:** Natural Earth Admin 0 1:110m (Public Domain) vector geometry registered and verified under `GEO-01`.
- **Authoritative State:** 100% deterministic initialization via seed with strict separation between pure .NET domain logic and Godot visual presentation.
