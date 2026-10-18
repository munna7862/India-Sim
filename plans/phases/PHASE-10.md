# Phase 10: TypeScript Engine-Free Architecture & Modern Web/Desktop Migration

## Objective
Migrate **Rashtra: Sovereign Republic** from the Godot 4 .NET game engine to an **engine-free TypeScript & Modern Web architecture**, enabling instant zero-install browser play (via GitHub Pages) and an ultra-lightweight standalone desktop executable (< 10 MB) while preserving 100% simulation and rule parity.

## Architecture & Benefits
1. **Zero Game Engine Dependencies:** Players no longer need to download a 133 MB Godot runtime, manage .NET Mono assemblies, or run graphics drivers that require engine wrappers.
2. **Instant Play in Browser (GitHub Pages):** Anyone with a web browser (Chrome, Edge, Firefox, Safari) can play immediately via a link with **zero downloads or installation**.
3. **Ultra-Lightweight Desktop (.exe):** Can be bundled via Tauri / Windows WebView2 into a standalone executable under **10 MB** (vs 133 MB previously).
4. **100% Content & Rule Parity:** Reuses all existing JSON content (`content/`) and ports pure domain logic and all 218 test cases into Vitest.

---

## Sprint Sequence

### Sprint 10-01: TypeScript Simulation Core & Vitest Parity Suite
- **Deliverables:**
  - `src/domain/models/`: `GameState`, `Country`, `Minister`, `ResourceLedger`, `MilitaryUnit`, etc.
  - `src/domain/commands/`: All 25+ game commands (budget, facilities, ministers, military, diplomacy).
  - `src/domain/clock/`: `SimulationClock` (deterministic 1-day/30-day cadence, speed 1x/2x/3x, pause).
  - `src/domain/loaders/`: Content loader importing existing `content/*.json` files.
  - `tests/`: 218 automated Vitest test cases replicating 100% of xUnit assertions.

### Sprint 10-02: Interactive Vector World Map & Geopolitical HUD
- **Deliverables:**
  - Dynamic projection of `content/map/world_countries.geojson` using SVG / Canvas.
  - Smooth pan & zoom (mouse drag, wheel, double-click).
  - Country hover tooltips, click selection, and diplomatic stance coloring (Homeland, War, Friendly, Neutral).
  - 3 map visual themes: Tactical Dark, Strategic Navy, Amber Cartographic.
  - Top Status Bar: National Treasury, Total Population, Approval Rating, Living Standard, Stability, and Clock controls (`Space`, `1`, `2`, `3`).

### Sprint 10-03: Governance, Tasks & Economy Desktop Panels (F1–F4)
- **Deliverables:**
  - **F1 Cabinet & Ideology:** Minister appointment/dismissal, competence ratings, salary costs, portfolio funding sliders (Health, Education, Defense, etc.), and ideology selection.
  - **F2 Presidential Tasks:** Multi-tier objectives, quantitative progress tracking, prerequisite unlocking, grant claim fanfare.
  - **F3 Domestic Events & Newsfeed:** Emergency modal dialogs, trade-off decision choices, anti-corruption campaigns, presidential security funding.
  - **F4 National Economy & Budget:** Tax policy sliders (Income, Corporate, VAT), IMF/Central Bank loan issuance/repayment, industrial facilities, economic policy toggles, and monthly accounting ledger.

### Sprint 10-04: Diplomacy, Military Warfare & Achievements (F6–F8)
- **Deliverables:**
  - **F6 Diplomacy:** Foreign country relations, bilateral opinion scores, monthly commodity trade contracts, Non-Aggression Pacts, foreign aid packages.
  - **F7 Military & Conflict:** 12 unit roster recruitment, monthly upkeep, army drill exercises, war declarations, deterministic combat resolution, and peace treaties.
  - **F8 Achievements:** 8 national achievements with live progress tracking and prestige rewards.

### Sprint 10-05: Persistence, Web Audio, GitHub Pages & Standalone Desktop Packaging
- **Deliverables:**
  - Atomic persistence with 5 save slots, QuickSave (`F5`), QuickLoad (`F9`), auto-save, and JSON export/import.
  - Keyboard navigation suite (`F1`-`F10`, `Space`, `1`-`3`, `Esc`).
  - Web Audio API procedural sound system for clicks, alerts, fanfare, and drills.
  - GitHub Pages CI/CD workflow: automatically builds and deploys to `https://munna7862.github.io/India-Sim/`.
  - Lightweight standalone desktop release build (< 10 MB).
