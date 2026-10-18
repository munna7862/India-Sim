# Sprint State

- Project: Rashtra: Sovereign Republic (Desktop Remake)
- Status: Phase 00 (Reference Research, Scope & Clean-Room Design) Completed! Transitioning to Phase 01.
- Active phase: 01 (Windows Desktop Shell & Original Visual System)
- Active sprint: 01-01 (Desktop project bootstrap and CI)
- Sprint status: Ready to begin (Phase 00 acceptance criteria verified; 11/11 tests green)
- Last verified commit: 6e977b6 (docs: complete Sprint 00-02 clean-room policy, asset provenance, and ADR-002)
- Last CI run: Local test run `dotnet test src/WorldState.sln` passed (11 passed, 0 failed)

## Current objective
Bootstrap the Windows desktop host project (`src/WorldState.Desktop`), configure automated GitHub Actions CI, and connect the Godot desktop client to the verified pure .NET domain simulation core.

## Blockers / decisions
- Reference Game Access Limitation: Direct Android APK execution is not supported natively in this Windows agent environment without an emulator. Publicly verified developer documentation, store descriptions, and gameplay captures were synthesized into `REFERENCE-INVENTORY.md` and `FEATURE-PARITY-MATRIX.md`. A companion checklist (`docs/research/REFERENCE-RESEARCH-CHECKLIST.md`) is provided for the human owner to log live-session numerical coefficients.
- Technical Spike (Sprint 00-03): COMPLETED & PASSED. Pure .NET 8 LTS domain library and test suite created, verified with 11 automated unit and round-trip tests passing in 73ms.
- Map Data Licensing: Resolved in Sprint 00-02 via ADR-002 (Natural Earth Admin 0 Public Domain vector datasets selected).
- Branding & Clean-Room Policy: Resolved in Sprint 00-02 via `docs/legal/CLEAN-ROOM-POLICY.md` (Working title: "Rashtra: Sovereign Republic").

## Sprint ledger
| Sprint | Status | Evidence |
|---|---|---|
| 00-01 | Completed | `docs/research/REFERENCE-INVENTORY.md`, `docs/research/FEATURE-PARITY-MATRIX.md`, `docs/research/REFERENCE-RESEARCH-CHECKLIST.md` |
| 00-02 | Completed | `docs/legal/CLEAN-ROOM-POLICY.md`, `docs/legal/ASSET-AND-DATA-PROVENANCE.md`, `docs/adr/ADR-002-clean-room-and-licensing.md`, `.gitignore` |
| 00-03 | Completed | `src/WorldState.sln`, `src/WorldState.Domain/`, `src/WorldState.Domain.Tests/` (11/11 tests pass), `docs/specs/V1-PARITY-SPECIFICATION.md` |
| 01-01 | Ready / Next | `plans/sprints/01-01.md` |
| 01-02 | Not started | None |
| 01-03 | Not started | None |
| 02-01 | Not started | None |
| 02-02 | Not started | None |
| 02-03 | Not started | None |
| 03-01 | Not started | None |
| 03-02 | Not started | None |
| 03-03 | Not started | None |
| 04-01 | Not started | None |
| 04-02 | Not started | None |
| 04-03 | Not started | None |
| 05-01 | Not started | None |
| 05-02 | Not started | None |
| 05-03 | Not started | None |
| 06-01 | Not started | None |
| 06-02 | Not started | None |
| 06-03 | Not started | None |
| 07-01 | Not started | None |
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
