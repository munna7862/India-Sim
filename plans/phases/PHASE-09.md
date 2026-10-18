# Phase 09: Parity QA, Windows Packaging & V1 Release

## Objective
Deliver the verified V1 reference behaviours for this phase with original graphics and desktop-friendly interactions.

## Status: COMPLETED

## Sprint sequence
- [x] **09-01: Feature parity audit and bug bash**: Run a structured comparison against the reference game. (Completed: `ParityAuditAndReleaseTests.cs`, 34/34 features Parity Passed, multi-year stress tests, debt crisis recovery, full combat cycle, save corruption recovery).
- [x] **09-02: Windows packaging, clean-machine test and recovery**: Deliver a reliable desktop build. (Completed: `DiagnosticsService.cs`, `WindowsPackagingAndDiagnosticsTests.cs`, `package.ps1`, `dist/Rashtra-v1.0.0-win-x64.zip`, `SHA256SUMS.txt`, `CRASH-AND-DIAGNOSTICS-GUIDE.md`).
- [x] **09-03: V1 release gate and V2 backlog capture**: Ship the faithful remake before introducing enhancements. (Completed: `RELEASE-NOTES-v1.0.0.md`, `DESKTOP-CONTROLS-GUIDE.md`, `docs/V2-BACKLOG.md` 5 modular domains, 218/218 passing automated tests, 0 warnings/errors, clean-checkout build).

## Phase Evidence
- **Feature Parity Matrix:** 34/34 systems verified and certified as `Parity Passed`.
- **Automated Tests:** 218/218 unit, integration, persistence, and performance benchmark tests passing.
- **Packaging Pipeline:** Standalone Windows x64 distribution bundle generated with SHA256 integrity verification (`package.ps1`).
- **Release Documentation:** Complete Release Notes (`RELEASE-NOTES-v1.0.0.md`), Desktop Controls Guide (`DESKTOP-CONTROLS-GUIDE.md`), and Crash & Diagnostics Guide (`CRASH-AND-DIAGNOSTICS-GUIDE.md`).
- **V2 Quarantine:** All non-parity enhancements isolated to `docs/V2-BACKLOG.md`.
- **Content Validation:** Validated 6 countries, 1 scenario, 1 map dataset, 8 tasks, 5 domestic events, 6 world events, 6 economic policies, 12 military units, 8 achievements with 0 warnings/errors.
