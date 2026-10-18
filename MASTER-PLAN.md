# India Simulator 2-Inspired Desktop Remake: Master Plan

## Project intent

Build a Windows desktop, clean-room remake of the gameplay experience of **India Simulator 2 Premium**, using newly designed graphics and a desktop-first UI. Version 1 prioritises feature and behaviour parity. Version 2 is a separate follow-up that introduces deeper simulation and improvements.

This is a functional remake inspired by the reference, not a copy of its source code or proprietary assets. Do not ship original screenshots, textures, icons, text, branding, or extracted code unless permission/licensing explicitly permits it.

## Reference facts and evidence policy

The referenced Softonic listing describes a president/country strategy simulator with a world map, government institutions, tasks and subtasks, and player-selected strategy. Treat the listing as an orientation, not a complete specification. The exact in-game behaviour must be documented by observing a legally obtained version of the game.

For every feature, record:
- observed steps and screenshots/notes for private analysis
- inputs, prerequisites, costs, timers, state changes and feedback
- normal, boundary, failure and save/load behaviour
- status: verified, inferred, unknown, out of scope
- parity test and acceptance criteria

Do not invent rules to fill gaps. Unknowns remain explicit until verified.

## Product definition

- **Platform:** Windows desktop
- **Genre:** Country/president strategy simulation
- **Version 1 goal:** Reproduce the reference's verified gameplay systems and loops, with original graphics and desktop-friendly interaction.
- **Version 2 goal:** Add deeper state-level simulation, richer economics, more autonomous AI, better diplomacy, improved military/logistics and other enhancements as a separate scope.
- **Connectivity:** Assume offline/single-player unless reference evidence or product decisions require otherwise.
- **Visual policy:** New original visual system; do not use proprietary reference assets without permission.

## Recommended technology

- **Candidate engine:** Godot 4 .NET with C# for Windows desktop.
- **Simulation domain:** Plain .NET/C# project, independent from Godot scenes.
- **Game client:** Godot scenes, map presentation, UI, input and desktop packaging.
- **Content:** JSON or equivalent validated data for countries, actions, tasks, events, institutions and scenarios.
- **Testing:** xUnit or compatible .NET test runner for domain rules; in-engine integration tests and desktop smoke tests; screenshot tests for stable UI states.
- **Delivery:** Git, CI, pinned engine/.NET versions, reproducible Windows builds.
- **Decision gate:** Validate engine and map prototype in Sprint 00-03 before finalising. Do not over-engineer V1 for V2.

## Core architecture rule

Authoritative gameplay state and rules must not live in UI widgets or Godot scene nodes. UI sends validated commands to the simulation domain; the domain changes state and emits events; UI renders the resulting state. This makes gameplay rules testable and keeps the V1 remake maintainable.

## V1 scope

V1 scope is defined by the verified feature parity matrix. Expected categories to investigate include:
- country selection and world map
- government institutions and national indicators
- tasks, subtasks and achievements
- budget/economy and development actions
- diplomacy and commercial agreements
- military, recruitment, deployment and conflict actions
- espionage/security actions where present
- domestic/world events and notifications
- game time, speed controls, progression and save/load
- settings and all required navigation

Only implement a category after verifying it exists and understanding its behaviour in the reference. The desktop UI may improve usability but must not change the underlying game rules in V1.

## V1 non-goals

- New state-level simulation not present in the reference
- Deep economic model, new demographic model, or advanced supply chain
- New military logistics/combat systems
- Fully autonomous strategic AI beyond reference behaviour
- Multiplayer, online services, mod marketplace
- 3D city simulation unless verified as part of the reference; graphic changes should initially focus on map/UI/2D presentation
- Rebalancing the game or changing the progression loop

## V2 boundary

Maintain a separate V2 backlog. V2 can add state-level India simulation, deeper economy/population, more capable diplomacy, AI nation goals, infrastructure/logistics, richer 3D views and improved sandbox tools. None of those features should delay or contaminate the V1 parity release.

## Phases

See `plans/PHASE-INDEX.md`. The roadmap contains 10 phases and 30 sprints. Every sprint includes tasks, acceptance criteria, test evidence, dependencies, risks, and persona assignment.

## Agent execution workflow

1. Read `AGENTS.md`, `SPRINT-STATE.md`, the active phase, and active sprint.
2. Review the reference evidence and parity matrix for the feature being implemented.
3. Propose a small implementation plan and identify touched systems/tests.
4. Assign a specialist persona; keep one integration owner.
5. Implement only the sprint scope.
6. Run unit/integration tests, content validation, build, and relevant UI smoke tests.
7. Perform independent review and fix findings.
8. Update parity matrix, sprint state, evidence, risks, and follow-up tasks.
9. Request human approval for scope changes, external asset/data licensing, engine changes, and releases.
10. Continue only when acceptance criteria pass. Do not mark a sprint done based on generated code alone.

## Definition of done

- Verified reference behaviour implemented or approved as a limitation.
- Acceptance criteria have evidence.
- Tests pass and regression coverage is updated.
- Original/legally usable assets only; provenance and licences recorded.
- Save/load compatibility checked for state changes.
- Documentation and parity matrix updated.
- No unresolved release-blocking defect.
- `SPRINT-STATE.md` reflects actual verified status.

## First milestone

A player can launch the Windows app, choose a country, enter the world map, open a government/task screen, perform one verified action, advance time if applicable, see the expected consequence, and save/reload. This proves the core remake architecture before the rest of the reference systems are built.
