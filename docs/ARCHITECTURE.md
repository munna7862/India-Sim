# Architecture Overview

## Runtime flow
1. Player clicks a desktop UI action.
2. UI constructs a typed command.
3. Domain validates actor, target, prerequisites, costs and timing.
4. Domain applies the action and records events.
5. Time-driven systems process timers, tasks, production, diplomacy and events at the observed cadence.
6. UI refreshes from an authoritative state snapshot.
7. Save service persists all required state with a schema version.

## Proposed solution structure
- `src/WorldState.Domain/`: authoritative rules and state, plain .NET.
- `src/WorldState.Domain.Tests/`: unit, integration and invariant tests.
- `src/WorldState.Desktop/`: Godot .NET scenes, map, UI, input and packaging.
- `content/`: countries, institutions, actions, tasks, events and scenario definitions.
- `tools/ContentValidator/`: stable ID, reference and schema checks.
- `.agents/`: coordinator and specialist instructions.
- `plans/`: phases and sprints.
- `docs/research/`: parity matrix and reference observations.

## Important
Godot is the candidate, not a blind commitment. Confirm the .NET-enabled engine and SDK work together in Sprint 00-03. Keep the simulation domain engine-independent. Avoid implementing a deep V2 architecture before the V1 vertical slice is proven.
