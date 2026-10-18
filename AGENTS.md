# Agent Rules: Desktop Remake V1

## Mission
Implement a clean-room Windows desktop remake of the verified gameplay experience of India Simulator 2 Premium, with original graphics. V1 parity comes before V2 improvements.

## Mandatory startup
1. Read `MASTER-PLAN.md`, `SPRINT-STATE.md`, active phase and sprint.
2. Read relevant feature entries in `docs/research/FEATURE-PARITY-MATRIX.md`.
3. Inspect existing code/tests before editing.
4. State a short plan, affected modules, tests, uncertainties, and risks.

## Non-negotiable rules
- Do not copy proprietary source code, extracted code, original assets, branding, or substantial expressive text without permission.
- Use reference gameplay as evidence for functional behaviour, not as a source of shipped assets.
- Mark behaviours verified, inferred, or unknown; never invent mechanics to fill gaps.
- V1 is not the improved sequel. Keep enhancements in `docs/V2-BACKLOG.md`.
- Keep authoritative rules outside UI widgets and scene nodes.
- Use stable IDs and data-driven action/task/event definitions where practical.
- Add regression tests for bugs and new rules.
- Never claim tests passed unless actually run.
- Check licences/provenance before importing external assets or map data.

## Persona routing
- Product Owner: scope, feature parity, priorities.
- Reference Researcher: screen/action inventory and evidence.
- Game/Simulation Architect: state models, command pipeline, time, persistence.
- World Map/Data Specialist: country registry, map data and licences.
- Domain Specialist: economy, government, tasks, diplomacy or military.
- Desktop UI/UX Specialist: visual system and mouse/keyboard usability.
- SDET/QA: tests, parity audit, regression and release evidence.
- DevOps/Build Engineer: CI, Windows builds and packaging.
- Sprint Coordinator: sequencing and sprint state.

## Approval gates
Human approval required for scope changes, new engine dependencies, external asset/data licence decisions, save-breaking changes, and release.

## Completion protocol
Run focused tests, relevant regression, build and content validation. Have another persona review. Update sprint state with real evidence, unresolved issues and next action.
