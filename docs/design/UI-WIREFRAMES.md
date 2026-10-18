# Desktop UI Wireframes & Layout Blueprints

- **Project:** Rashtra: Sovereign Republic (Windows Desktop Remake)
- **Primary Persona:** Desktop UI/UX Specialist
- **Target Resolution:** 1920 × 1080 (Responsive)

---

## Blueprint 1: Primary Desktop Game Shell & Map Viewport

```text
+-------------------------------------------------------------------------------------------------------------------------+
| [RASHTRA] | 15 JAN 2026 | [ || ] [ > ] [ >> ] [ >>> ] |  TREASURY: $50.0B  |  POP: 1.40B  |  APPR: 65%  |  LIVING: 55%  |  [!] 2 | [COG] [ESC] |
+-------------------------------------------------------------------------------------------------------------------------+
| [NAV]  |                                                                                           | [DRAWER / PANEL]    |
|        |                                                                                           |                     |
| [1] OV |                                                                                           | SELECT AN ENTITY    |
| [2] GOV|                                     WORLD MAP VIEWPORT                                    | OR COUNTRY ON MAP   |
| [3] TSK|                                                                                           |                     |
| [4] ECO|                    [ Interactive 2D Political Vector Map (Natural Earth) ]                | ------------------- |
| [5] DIP|                                                                                           | Quick Stats:        |
| [6] MIL|                     Selected: India (Republic of India)                                   | - Status: Sovereign |
| [7] SEC|                     Borders: Green (Allied), Red (Hostile), Yellow (Neutral)              | - Stability: 70%    |
| [8] EVT|                                                                                           | - Active Wars: None |
|        |                                                                                           | - GDP Growth: +6.2% |
|        |                                                                                           |                     |
|        |                                                                                           | [ Open Overview ]   |
|--------|                                                                                           |                     |
| [?] HLP| [ - ] Zoom Slider [ + ]                         Camera: 28.61° N, 77.20° E (New Delhi)    | [ Close Panel (Esc)]|
+-------------------------------------------------------------------------------------------------------------------------+
| FOOTER: [F1] Overview  [F2] Cabinet  [F3] Tasks  [F4] Economy  [F5] Diplomacy  [F6] Military  [Space] Pause  [Esc] Menu   |
+-------------------------------------------------------------------------------------------------------------------------+
```

---

## Blueprint 2: Government & Cabinet Ministries Drawer (Right Drawer: 420px)

```text
+------------------------------------------------------------+
|  CABINET MINISTRIES                                  [ X ] |
|  National Governance & Sector Administration               |
+------------------------------------------------------------+
|  CABINET EFFICIENCY: 82%          TOTAL SALARIES: $3.2M/mo |
+------------------------------------------------------------+
|  [+] MINISTRY OF HEALTH                                    |
|      Minister: Dr. A. Sharma (Competence: 85%)             |
|      Funding:  [ Low | Med | HIGH | Max ] ($75M/mo)        |
|      Status:   +1.4% Healthcare Index / mo                 |
|      Action:   [ Replace Minister ]   [ View Projects ]    |
|------------------------------------------------------------|
|  [+] MINISTRY OF EDUCATION                                 |
|      Minister: Prof. K. Menon (Competence: 78%)            |
|      Funding:  [ Low | MED | High | Max ] ($50M/mo)        |
|      Status:   Literacy: 77.8% (+0.2% / mo)                |
|      Action:   [ Replace Minister ]   [ View Projects ]    |
|------------------------------------------------------------|
|  [+] MINISTRY OF DEFENSE                                   |
|      Minister: Gen. V. Singh (Competence: 92%)             |
|      Funding:  [ Low | Med | High | MAX ] ($100M/mo)       |
|      Status:   Readiness: 88%  |  Active Units: 145,000    |
|      Action:   [ Replace Minister ]   [ Army Drills ]      |
|------------------------------------------------------------|
|  [+] MINISTRY OF FINANCE & CENTRAL BANK                    |
|      Minister: S. Patel (Competence: 88%)                  |
|      Tax Policy: Income (20%) | Corp (25%) | VAT (15%)     |
|      Action:   [ Adjust Tax Rates ]   [ Issue Debt ]       |
+------------------------------------------------------------+
| [ Close Drawer (Esc) ]            [ Cabinet Reform Policy] |
+------------------------------------------------------------+
```

---

## Blueprint 3: Presidential Tasks & Objectives Drawer

```text
+------------------------------------------------------------+
|  PRESIDENTIAL TASKS & ACHIEVEMENTS                   [ X ] |
|  Strategic Roadmaps and National Milestones                |
+------------------------------------------------------------+
|  [ ACTIVE TASKS (3) ]          [ COMPLETED ARCHIVE (12) ]  |
+------------------------------------------------------------+
|  TASK #01: ECONOMIC SUPERPOWER                             |
|  Target: Build national industrial resilience.             |
|  Progress: [=========================>          ] 70%      |
|                                                            |
|  Sub-Tasks:                                                |
|  [X] Construct 30 Power Plants (30/30)                     |
|  [X] Achieve $50B National Treasury                        |
|  [ ] Construct 50 Manufacturing Factories (40/50)          |
|                                                            |
|  Reward on Completion: +$5.0B Treasury, +5% Approval       |
|------------------------------------------------------------|
|  TASK #02: UNIVERSAL HEALTHCARE DRIVE                      |
|  Target: Expand clinic coverage across all states.         |
|  Progress: [==================>                 ] 50%      |
|                                                            |
|  Sub-Tasks:                                                |
|  [X] Appoint Minister of Health                            |
|  [ ] Construct 20 Regional Hospitals (10/20)               |
|                                                            |
|  Reward on Completion: +3.0% Standard of Living            |
|------------------------------------------------------------|
|  TASK #03: NON-AGGRESSION COALITION                        |
|  Target: Secure regional borders through bilateral pacts.  |
|  Progress: [====================================] 100%     |
|                                                            |
|  Sub-Tasks:                                                |
|  [X] Sign 3 Non-Aggression Pacts (3/3)                     |
|                                                            |
|  STATUS: READY TO CLAIM!                                   |
|  [ CLAIM REWARD: +$1.0B & DIPLOMACY TROPHY ]               |
+------------------------------------------------------------+
| [ Close (Esc) ]                                            |
+------------------------------------------------------------+
```

---

## Blueprint 4: Action Confirmation Modal (Exposing Costs & Invariants)

```text
+------------------------------------------------------------+
|                      CONFIRM ACTION                        |
|         APPOINT MINISTER OF DEFENSE: GEN. V. SINGH         |
+------------------------------------------------------------+
|  CANDIDATE DOSSIER:                                        |
|  - Name: General Vikram Singh                              |
|  - Competence Rating: 92% (High combat effectiveness)      |
|  - Prior Service: Chief of Armed Forces                    |
|                                                            |
|  PREREQUISITES:                                            |
|  [OK] Treasury balance sufficient                          |
|  [OK] Defense Ministry position vacant                     |
|  [OK] National Stability >= 50%                            |
|                                                            |
|  FINANCIAL IMPACT:                                         |
|  - One-Time Hiring Fee:  -$15,000,000 (Deducted now)       |
|  - Ongoing Monthly Cost:  +$500,000 / month (Salary)       |
|  - Current Treasury:      $50,000,000,000                  |
|  - Treasury After Fee:    $49,985,000,000                  |
|                                                            |
|  PROJECTED SECTOR GAIN:                                    |
|  +4.5% Monthly Armed Forces Readiness                      |
+------------------------------------------------------------+
|     [ CANCEL (Esc) ]              [ CONFIRM ACTION (Enter) ]|
+------------------------------------------------------------+
```

---

## Blueprint 5: Emergency Crisis Decision Dialog

```text
+-----------------------------------------------------------------------+
|  [!] CRITICAL NATIONAL EMERGENCY: PROLONGED MONSOON DROUGHT           |
+-----------------------------------------------------------------------+
|  A severe drought has struck the agricultural grain belt. Food output |
|  has fallen by 35%, threatening national famine and public protests.  |
|                                                                       |
|  Active Timer: [============================>     ] 18s Remaining     |
+-----------------------------------------------------------------------+
|  CHOICE A: COMPREHENSIVE EMERGENCY RELIEF FUND                        |
|  - Authorize $2.5B in emergency food imports and farmer subsidies.    |
|  - Cost: -$2,500,000,000 Treasury                                     |
|  - Outcome: +5% Public Approval, Food deficit completely offset.       |
|  [ Select Option A (1) ]                                              |
|-----------------------------------------------------------------------|
|  CHOICE B: TARGETED GRAIN RATIONING & LOCAL AID                       |
|  - Release strategic food reserves and provide localized tax relief.  |
|  - Cost: -$500,000,000 Treasury                                       |
|  - Outcome: -2% Public Approval, Food deficit halved.                 |
|  [ Select Option B (2) ]                                              |
|-----------------------------------------------------------------------|
|  CHOICE C: RELY ON MARKET MECHANISMS (NO FEDERAL SPENDING)            |
|  - Take no federal action; allow commercial prices to adjust.         |
|  - Cost: $0 Treasury                                                  |
|  - Outcome: -12% Public Approval, severe civilian unrest and strikes. |
|  [ Select Option C (3) ]                                              |
+-----------------------------------------------------------------------+
```
