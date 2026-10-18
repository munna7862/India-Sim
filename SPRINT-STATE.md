# Sprint State

- Project: Rashtra: Sovereign Republic (Desktop Remake)
- Status: Phase 00 in progress; Sprint 00-02 completed; ready for Sprint 00-03.
- Active phase: 00 (Reference Research, Scope & Clean-Room Design)
- Active sprint: 00-02 (Clean-room design and legal boundaries) -> Completed; Next: 00-03 (V1 acceptance criteria and technical spike)
- Sprint status: Completed (Outputs verified and documented; ready for Sprint 00-03)
- Last verified commit: 27af9c6 (chore: remove redundant agents/ folder in favor of native .agents/skills/)
- Last CI run: None

## Current objective
Prove the desktop technical stack, validate .NET 8 LTS integration, define measurable V1 acceptance gates, and prepare for Phase 01 bootstrap.

## Blockers / decisions
- Reference Game Access Limitation: Direct Android APK execution is not supported natively in this Windows agent environment without an emulator. Publicly verified developer documentation, store descriptions, and gameplay captures were synthesized into `REFERENCE-INVENTORY.md` and `FEATURE-PARITY-MATRIX.md`. A companion checklist (`docs/research/REFERENCE-RESEARCH-CHECKLIST.md`) is provided for the human owner to log live-session numerical coefficients.
- Technical Spike (Sprint 00-03): Pinned Godot 4 .NET release and .NET 8 SDK LTS compatibility validation underway.
- Map Data Licensing: Resolved in Sprint 00-02 via ADR-002: Natural Earth Admin 0 Public Domain vector datasets selected.
- Branding & Clean-Room Policy: Resolved in Sprint 00-02 via `docs/legal/CLEAN-ROOM-POLICY.md` and working title "Rashtra: Sovereign Republic".

## Sprint ledger
| Sprint | Status | Evidence |
|---|---|---|
| 00-01 | Completed | `docs/research/REFERENCE-INVENTORY.md`, `docs/research/FEATURE-PARITY-MATRIX.md`, `docs/research/REFERENCE-RESEARCH-CHECKLIST.md` |
| 00-02 | Completed | `docs/legal/CLEAN-ROOM-POLICY.md`, `docs/legal/ASSET-AND-DATA-PROVENANCE.md`, `docs/adr/ADR-002-clean-room-and-licensing.md`, `.gitignore` |
| 00-03 | Ready / Next | `plans/sprints/00-03.md` |
| 01-01 | Not started | None |
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
