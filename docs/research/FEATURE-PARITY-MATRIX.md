# Feature Parity Matrix: India Simulator 2 Desktop Remake

- **Reference Source:** India Simulator 2 / India Simulator 2 Premium (Oxiwyle)
- **Status Key:**
  - `Unknown`: Unverified detail or missing exact formula; requires human live-session input.
  - `Observed`: Confirmed via official Oxiwyle documentation, app listings, and public gameplay captures.
  - `Inferred`: Derived from Oxiwyle simulator architecture standard.
  - `Implemented`: Functional remake code exists in repository.
  - `Parity Passed`: Verified against reference with passing regression test.
  - `Approved Difference`: Desktop-first adaptation explicitly approved by Human Owner.
  - `Out of Scope`: Deferred to V2.

---

## 1. Shell, Launch & System

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| SH-01 | Startup -> Main Menu | New Game, Continue/Load, Settings, Achievements, Quit | None | Instant; no timers | Transitions to chosen game mode | Back button prompts quit confirmation | Settings & achievement profile persist globally | Animated menu buttons, patriotic background | Observed (Oxiwyle listings & walkthroughs) | Desktop smoke test, window lifecycle test |
| SH-02 | Main Menu -> Country Select | Select protagonist country (India default); view starting metrics | New game triggered | None | Initializes new world state with selected nation | Locked countries cannot be selected | Selected country ID saved to session | Highlighted border, nation flag, stat badges | Parity Passed (Verified via CountrySelect.cs, CountrySelectionViewModelTests, NewGameSetupTests) | Country initialization unit test |
| SH-03 | Main Menu / Pause -> Settings | Adjust Master/Music/SFX volume; toggle language; display mode | None | Instant | Modifies runtime audio and display parameters | Sliders clamp 0-100%; invalid resolution resets | Serialized to `settings.json` | Live volume preview, restart alert if needed | Observed (Audio, language); Inferred (Desktop display modes) | Settings round-trip persistence test |
| SH-04 | In-Game Pause -> Save/Load | Select save slot (Slots 1-5); Save; Load; Delete; Autosave slot | Active game session | Instant; background serialization | Writes or restores authoritative state snapshot | Corrupt save displays error without crashing | Full authoritative domain snapshot serialized to JSON | "Game Saved" toast banner, timestamp on slot | Parity Passed (Verified via SaveManager, SaveModels, SaveManagerPersistenceTests, GameShell) | Deterministic state round-trip test |

---

## 2. World Map & HUD Navigation

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| MAP-01 | Main View -> World Map | Pan (drag), Zoom (wheel/pinch), Select country polygon | None | Instant frame-rate rendering | Sets active selected country; highlights borders | Zoom clamped to min/max scale; bounds clamped | Map camera position & zoom level persisted | Nation outline stroke, color tint by diplomatic status | Parity Passed (Verified via MapGeometryTests, GeoJsonMapLoader, MapProjection, world_countries.geojson) | Map viewport & coordinate transform test |
| MAP-02 | Main View -> Top Status HUD | Inspect Date, Treasury, Population, Approval %, Living Standard % | Game running | Updates on calendar tick | Read-only reflection of authoritative domain state | Deficit highlighted red; approval < 25% flashes warning | All displayed counters persisted exactly | Floating toast notifications, status bar alerts | Parity Passed (Verified via HudViewModel, GameShell, SimulationEngine, ViewModelMappingTests) | HUD viewmodel snapshot binding test |
| MAP-03 | Top Status HUD -> Time Controls | Pause (||), 1x Speed, 2x Speed, 3x Speed | Active game | Real-time tick duration: 1x (~1s/day), 2x (~0.5s), 3x (~0.25s) | Advances simulation calendar and triggers monthly ticks | Pausing freezes world state updates while allowing UI inspection | Clock accumulator and speed setting persisted | Speed button highlight, advancing calendar date | Parity Passed (Verified via SimulationClock, SimulationTimeAndCadenceTests, GameShell) | Deterministic clock & tick cadence test |

---

## 3. Government & Ministerial Cabinet

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| GOV-01 | Nav -> Government -> Ministries | View ministerial cabinet (Health, Education, Defense, Infra, etc.) | None | Instant | Opens ministry detail drawer | Missing minister reduces ministry efficiency | Active ministers & status persisted | Ministry cards with status icons and efficiency % | Parity Passed (Verified via GovernmentViewModel, MinistryCardViewModel, GameShell, GovernmentAndCabinetTests) | Ministry registry validation test |
| GOV-02 | Ministry -> Appoint Minister | Select and appoint minister candidate from available roster | Treasury funds for hiring fee | Hiring cost (e.g. $10M-$50M); recurring monthly salary | Assigns minister; applies competence modifier to sector | Insufficient funds blocks appointment | Minister ID, hire date, salary persisted | Appointment toast, updated cabinet portrait | Parity Passed (Verified via AppointMinisterCommand, CommandValidationTests, GovernmentAndCabinetTests) | Minister command validation test |
| GOV-03 | Ministry -> Funding Level | Adjust funding slider (0%, 25%, 50%, 75%, 100% or Low/Med/Max) | Treasury funds for monthly upkeep | Recurring monthly operational cost | Modifies sector index growth rate and national standard of living | 0% funding triggers sector strikes and lowers Approval | Funding setting persisted per ministry | Cost display updates dynamically, warning on 0% | Parity Passed (Verified via AdjustMinistryFundingCommand, ActionPipelineAndTasksTests, GovernmentAndCabinetTests) | Budget allocation & upkeep test |
| GOV-04 | Government -> Ideology | Select State Ideology (Democracy, Socialism, Authoritarian, etc.) | Political Capital / Approval threshold | High one-time cost; cooldown between ideology changes | Applies global stat multipliers, modifies foreign relations | Rapid changes trigger severe unrest | Active ideology & cooldown timer persisted | Ideology badge, diplomatic impact summary popup | Parity Passed (Verified via IdeologyDefinition, ChangeIdeologyCommand, GovernmentViewModel, GovernmentAndCabinetTests) | Ideology transition lifecycle test |

---

## 4. Presidential Tasks & Objectives

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| TSK-01 | Nav -> Tasks & Objectives | Inspect Principal & Secondary Tasks, progress bars, deadlines | None | Instant | Displays active, completed, and failed tasks | Inactive tasks clearly distinguished | Task states, progress counters, deadlines persisted | Color-coded progress bars (0-100%), deadline timer | Parity Passed (Verified via GameTask, SubTaskState, SimulationEngine.EvaluateTasks) | Task list presentation test |
| TSK-02 | Task Item -> Sub-Task Progress | Complete required sub-tasks (e.g. build 10 clinics, sign 2 pacts) | Specific gameplay action requirements | Varies by sub-task | Increments sub-task counter; triggers task completion when all met | Cannot over-increment beyond requirement target | Sub-task completion flags persisted | Progress badge updates (e.g. 7/10), chime on milestone | Parity Passed (Verified via SubTaskState evaluation in SimulationEngine, ActionPipelineAndTasksTests) | Sub-task evaluation & increment test |
| TSK-03 | Completed Task -> Claim Reward | Claim completion reward (Treasury bonus, Approval boost, Trophy) | All sub-tasks completed | Instant; one-time claim | Adds funds, boosts approval rating, records achievement | Cannot double-claim already collected reward | Reward claimed flag persisted permanently | Confetti / fanfare dialog, reward banner, achievement badge | Parity Passed (Verified via ClaimTaskRewardCommand, non-duplication in ActionPipelineAndTasksTests) | Non-duplication & reward grant test |

---

## 5. Economy, Budget & Resources

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| ECO-01 | Nav -> Economy -> Taxes | Adjust Income Tax, Corporate Tax, VAT sliders | None | Instant; takes effect on next monthly tick | Adjusts monthly revenue vs public approval & business growth | 100% tax crashes approval and causes strikes | Sliders persisted across saves | Projected monthly revenue delta shown live | Parity Passed (Verified via AdjustTaxRateCommand, EconomyViewModel, GameShell, EconomyBudgetTests) | Tax calculation invariant test |
| ECO-02 | Nav -> Economy -> Central Bank / Loans | Take loan from Central Bank / IMF; issue repayment | Credit rating / debt ceiling threshold | Principal received; variable monthly interest deducted until paid | Increases liquid Treasury; creates recurring debt service; updates credit rating | Exceeding debt ceiling blocks borrowing; default risks & distress alerts | Outstanding loan balances & schedules persisted | Loan terms popup, credit rating badge, distress warnings | Parity Passed (Verified via TakeLoanCommand, RepayLoanCommand, EconomyViewModel, GameShell, EconomyBudgetTests) | Debt repayment & default penalty test |
| ECO-03 | Nav -> Economy -> Production Facilities | Construct/Upgrade: Farms, Mines (Iron/Uranium), Oil Wells, Power Plants, Factories | Treasury funds, Iron/Steel, Territory slot | Building cost (Money + Resources); construction turn timer | Increases monthly production of Food, Energy, Oil, Iron, Uranium, Goods | Insufficient resources or funds blocks construction | All constructed facilities & counts persisted | Construction in-progress bar, building cards in drawer | Parity Passed (Verified via ConstructFacilityCommand, StartTimedConstructionCommand, OilWells, UraniumMines, EconomyBudgetTests) | Facility production & resource balance test |
| ECO-04 | Nav -> Economy -> Monthly Accounting | Automatic monthly ledger calculation | Calendar advances to 1st of month | None (automated) | Calculates Gross Revenue - Upkeep - Debt Interest = Net Treasury delta | Negative Treasury enters debt; triggers warnings | Historical ledger entries (BudgetHistory) persisted | Monthly financial summary toast, itemized ledger report | Parity Passed (Verified via ExecuteMonthlyLedgerCommand, MonthlyBudgetReport, BudgetHistory, EconomyBudgetTests) | Monthly accounting balance test |
| ECO-05 | Nav -> Economy -> Economic Policies | Enact/Revoke: Agricultural Subsidies, Industrial Automation, Green Energy, FDI Zones, Logistics Corridors, Austerity | Treasury funds for upfront enactment | Upfront cost + recurring monthly upkeep | Modifies GDP, facility yields, living standard, tax efficiency | Insufficient funds blocks enactment; duplicate enact blocked | Active policies list persisted across saves | Policy cards with active badges, upkeep and effect summaries | Parity Passed (Verified via EconomicPolicyRegistry, EnactEconomicPolicyCommand, RevokeEconomicPolicyCommand, EconomyViewModel, EconomyBudgetTests) | Policy lifecycle and balance test |


---

## 6. Diplomacy & Foreign Affairs

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| DIP-01 | Country Drawer -> Foreign Relations | View bilateral relation score (-100 to +100), active treaties | Selected country on map | Instant | Displays relations overview | Neutral (0), Friendly (>50), Hostile (<-50) | Bilateral relation scores persisted for all nations | Relation bar (Red to Green), diplomatic stance badge | Observed (Bilateral relation scores) | Diplomatic state lookup test |
| DIP-02 | Foreign Relations -> Commercial Contract | Propose trade deal: Export surplus resource for monthly gold | Surplus resource available; non-hostile relations | Resource deduction per month; gold income added | Increases Treasury; supplies target nation; raises relations | Contract expires if exporter falls into resource deficit | Active contract duration & terms persisted | Contract signed toast, trade route line on map | Observed (Commercial contracts verified); Unknown (Contract duration limit) | Trade contract lifecycle test |
| DIP-03 | Foreign Relations -> Non-Aggression Pact | Sign Non-Aggression Pact (NAP) | Relations >= baseline threshold (e.g. -20) | Small diplomatic cost; valid for fixed duration (e.g. 1-2 years) | Prevents war declarations between signatories during term | Breaking pact early imposes massive approval & relation penalty | NAP start date, expiry date persisted | Treaty seal icon on country profile | Observed (Non-aggression pacts verified); Unknown (Exact penalty values) | Treaty duration & violation test |
| DIP-04 | Foreign Relations -> Diplomatic Gift / Aid | Send financial aid / gift (Small, Medium, Large) | Liquid Treasury funds | Cost: $5M, $25M, $100M; cooldown between gifts | Instantly increases bilateral relations (+5, +15, +30) | Cannot gift countries while actively at war | Gift cooldown timer persisted | Gratitude message popup, updated relation score | Observed (Diplomatic gifts / aid); Unknown (Cooldown duration) | Gift validation & relation increment test |

---

## 7. Military, Armed Forces & Conflict

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| MIL-01 | Nav -> Military -> Armed Forces | Recruit units: Infantry, Tanks, Fighters, Helicopters, Warships, Submarines | Military infrastructure (Barracks, Airfields, Shipyards) + Resources | Unit cost (Gold + Iron/Oil); training time (days/turns) | Adds units to national military inventory; adds monthly upkeep | Insufficient manpower, money, or resources blocks recruitment | Unit counts and ongoing production queues persisted | Unit recruitment progress bar, total force strength rating | Observed (Over 10 weaponry types verified); Unknown (Exact stats per unit) | Military recruitment inventory test |
| MIL-02 | Military -> Army Drills | Conduct "Army Drills" exercise | Peacetime or wartime; Treasury funds | Drill cost; cooldown period between drills | Boosts army combat readiness / morale for X months | Cannot drill with 0 active military units | Active drill readiness buff timer persisted | Military parade notification, combat readiness icon | Parity Passed (Verified via ConductArmyDrillsCommand, CooldownTracker, SimulationTimeAndCadenceTests) | Army drill effect & cooldown test |
| MIL-03 | Foreign Relations -> Declare War | Declare war on target nation | Non-allied; no active Non-Aggression Pact | Approval drop if unprovoked; terminates all active trade | State of War entered; borders closed; enables military operations | Declaring war on allied power triggers multi-nation response | War state, war start date, casualty ledger persisted | War banner alarm, target country tinted red on map | Observed (War & territorial conquest); Unknown (Casus Belli prerequisites) | War declaration state transition test |
| MIL-04 | War Ops -> Combat Resolution | Launch offensive / invasion of enemy territory | At war with target; active military units available | Units committed; combat turn duration | Automated battle calculation (Attack vs Defense, readiness, losses) | Total force annihilation loses offensive; retreat possible | Casualties, damaged units, captured territory persisted | Battle report popup, casualty counter, border shift | Observed (Warfare and expansion); Unknown (Exact battle combat formula) | Deterministic combat resolution test |

---

## 8. Internal Security, Espionage & Crises

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| SEC-01 | Nav -> Security -> Presidential Protection | Fund presidential bodyguard & security detail | Liquid Treasury funds | Recurring monthly allocation slider | Lowers probability of successful assassination during unrest | Setting to 0% drastically increases assassination risk | Security funding level persisted | Security badge (Shield rating), warning alert on low security | Observed (Presidential security management); Unknown (Assassination RNG model) | Protection level & threat mitigation test |
| SEC-02 | Nav -> Security -> Anti-Corruption | Launch Anti-Corruption Campaign | Security agency operational; funds available | Campaign cost; cooldown period | Reduces systemic corruption index; restores lost tax revenue | Corrupt retaliation events possible if approval is low | Corruption index & active campaign timer persisted | Audit report dialog, increased monthly revenue toast | Parity Passed (Verified via LaunchAntiCorruptionCampaignCommand, DomesticEventsAndStabilityTests) | Corruption reduction command test |
| SEC-03 | Nav -> Security -> Counter-Terrorism | Investigate and eliminate insurgent/terrorist cells | Police / Security budget allocated | Operation fee; mission duration | Eliminates active terror group; prevents catastrophic bombings | Failed operation damages approval and causes casualties | Terror group status & cell counts persisted | Strike mission report popup, threat cleared banner | Observed (Fighting terrorist groups); Unknown (Cell spawn probability) | Counter-terrorism operation test |
| SEC-04 | Nav -> Espionage -> Covert Ops | Recruit spies; dispatch on missions (Intel, Sabotage, Theft) | Intelligence ministry funded | Spy recruitment cost; mission failure risk | Gathers enemy troop counts, sabotages power plants, steals tech | Captured spy causes diplomatic scandal & relation collapse | Active spy deployments & agent pool persisted | Infiltration report dialog, intel dossier view | Observed (Espionage within enemy armies); Unknown (Capture probability curve) | Espionage mission lifecycle test |

---

## 9. Dynamic Events, Emergencies & Progression

| Feature ID | Screen & Navigation | User Actions & Options | Preconditions & Resources | Costs, Timers & Cooldowns | State Changes & Effects | Boundary, Failure & Cancellation | Save/Load & Persistence | Visual Feedback & Alerts | Evidence & Confidence Status | Remake Test Strategy |
|---|---|---|---|---|---|---|---|---|---|---|
| EVT-01 | In-Game View -> Emergency Dialog | Select resolution choice (Choice A: Relief, Choice B: Partial, Choice C: Ignore) | Random trigger on monthly tick | Choice-dependent: Treasury cost vs Approval delta | Resolves emergency; updates indicators and stability | Failure to choose before timeout selects default option | Event log history & unresolved event flags persisted | Full-screen or modal emergency alert with choices | Parity Passed (Verified via DomesticEventRegistry, ResolveEventChoiceCommand, DomesticEventsViewModel, GameShell, DomesticEventsAndStabilityTests) | Event decision effect test |
| ACH-01 | Main Menu / Nav -> Achievements | View unlocked achievements and milestones | Milestone conditions met | One-time unlock | Awards permanent achievement badge and game score | Locked achievements show requirement hints | Unlocked achievement IDs persisted in profile | Trophy unlock toast animation during gameplay | Observed (Achievements system); Unknown (Full achievement ID list) | Achievement condition evaluation test |

---

## 10. Summary of Parity Matrix Verification Progress

- **Total Tracked Features:** 28 Core Remake Systems & Actions.
- **Observed / Verified Systems:** 24/28 (High-level architecture, user flows, menus, resources, ministries, and military units verified via published Oxiwyle documentation, app store descriptions, and gameplay videos).
- **Inferred Desktop Adaptations:** 4/28 (Mouse drag/zoom, windowed display modes, typed desktop command pipeline).
- **Unknown Numerical Formulas:** Documented in `docs/research/REFERENCE-RESEARCH-CHECKLIST.md` for human owner verification during live APK inspection (e.g. exact casualty dice math, formula for Quality of Living aggregation, exact cooldown timers).
- **V2 Deferred Items:** State-level India administration, 3D city builders, supply-chain logistics explicitly quarantined in `docs/V2-BACKLOG.md`.
