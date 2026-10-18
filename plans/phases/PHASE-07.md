# Phase 07: Achievements, Events, Notifications & Progression (COMPLETED)

## Objective
Deliver the verified V1 reference behaviours for this phase with original graphics and desktop-friendly interactions.

## Sprint sequence
- **07-01: Achievements and progression** (COMPLETED): Recreate the observed achievement and reward progression. (`content/achievements/achievements.json`, `AchievementRegistry`, `ClaimAchievementRewardCommand`, `AchievementsViewModel`, `AchievementsPanel`).
- **07-02: World events, alerts and event history** (COMPLETED): Complete the event feedback loop. (`content/events/world_events.json`, `WorldEventRegistry`, `DismissNotificationCommand`, `EventHistoryViewModel`, `EventsPanel`).
- **07-03: Cross-system gameplay parity** (COMPLETED): Test how tasks, economy, government, diplomacy and military interact. (`CrossSystemGameplayParityTests.cs`, mid-simulation persistence, end-to-end progression loop).

## Phase evidence
- Feature Parity Matrix updated with `ACH-01` and `EVT-02` marked Parity Passed (`docs/research/FEATURE-PARITY-MATRIX.md`).
- Automated tests passing: 182/182 tests green across domain rules, persistence, and cross-system parity.
- Content validation: 6 countries, 1 scenario, 1 map dataset, 8 tasks, 5 domestic events, 6 world events, 6 policies, 12 military units, 8 achievements validated with 0 errors.
- Desktop UI: Hotkeys F8 (Events) and F10 (Achievements), unread badge counts, action navigation shortcuts, and reward claiming verified in `GameShell`.
- Build verification: `build.ps1` runs clean on Windows desktop with 0 warnings and 0 errors.
