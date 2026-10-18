# Reference Research & In-Game Verification Checklist

- **Target Game:** India Simulator 2 / India Simulator 2 Premium (Oxiwyle)
- **Target Audience:** Human Owner / In-Game Researcher with access to an Android/iOS device running the reference title.
- **Purpose:** Provide an exhaustive, step-by-step observational protocol to eliminate all remaining `Unknown` markers in `docs/research/FEATURE-PARITY-MATRIX.md`.

---

## Observational Protocol Instructions

When logging observations from a live game session:
1. **Device & Build Info:** Record device model, OS version, app version (e.g., v1.0.x), and language setting.
2. **Screenshots:** Capture private reference screenshots of each menu, confirmation dialog, tooltip, and warning banner. (Keep strictly in private analysis notes; do not commit proprietary assets to public release).
3. **Exact Numbers:** Record starting numbers, exact costs, slider minimums/maximums, and calendar step durations.
4. **Edge Cases:** Deliberately trigger edge cases (e.g., zero gold, maximum taxes, declaring war on a superpower, 0% approval).

---

## Section 1: Shell, Launch & Flow

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 1.1 | App Startup -> Splash | What logos or legal screens appear before the main menu? What is the transition timing? | | [ ] |
| 1.2 | Main Menu | List all main menu buttons in order. Does a "Resume" button appear when an active save exists? | | [ ] |
| 1.3 | Country Selection | Can the player select countries other than India on Turn 1? Are any countries locked behind achievements/premium? What starting indicators are shown? | | [ ] |
| 1.4 | New Game Initialization | Is there a difficulty selector (Easy/Normal/Hard)? Does the game start immediately or show a backstory popup? | | [ ] |
| 1.5 | Settings Menu | What specific audio sliders exist (Master, Music, SFX)? What languages are selectable? Is there an autosave toggle or frequency option? | | [ ] |
| 1.6 | Save / Load Screen | How many manual save slots exist? Can saves be renamed? What metadata is shown on each slot (Date, Treasury, Country)? | | [ ] |

---

## Section 2: HUD, Map & Simulation Clock

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 2.1 | Header Status Bar | What exact icons and numbers appear on the top bar? (Treasury, Pop, Approval, Living standard, etc.) In what order? | | [ ] |
| 2.2 | Simulation Clock | What is the calendar format (DD/MM/YYYY, MM/YYYY)? How many real-time seconds elapse per in-game day/month on 1x, 2x, and 3x speeds? | | [ ] |
| 2.3 | Pause Behavior | When paused, can the player still queue orders, adjust sliders, and recruit troops? | | [ ] |
| 2.4 | World Map Navigation | Does the map wrap horizontally (360 degrees)? What are the zoom limits? How are borders and selected territories highlighted? | | [ ] |
| 2.5 | Country Tap / Selection | Tapping another country on the map: Does it open a bottom sheet, a modal dialog, or switch the active tab? What info is displayed? | | [ ] |

---

## Section 3: Government, Cabinet & Ideology

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 3.1 | Ministries List | What is the complete list of ministries in the game? Are all available immediately or unlocked over time? | | [ ] |
| 3.2 | Minister Appointment | How are ministers appointed? Is there a candidate pool with names/stats, or a simple "Appoint" button with a fixed fee? | | [ ] |
| 3.3 | Minister Stats & Salary | What attributes do ministers possess (Loyalty, Skill, Corruption)? What is their ongoing monthly cost? | | [ ] |
| 3.4 | Ministry Funding Sliders | What are the funding levels or steps (e.g., 0% to 100%, or Low/Med/High)? What happens when funding is set to zero? | | [ ] |
| 3.5 | Sector Projects | What specific one-time projects exist under each ministry (e.g., under Health: Build Clinics, Vaccinations)? What are their exact costs and bonuses? | | [ ] |
| 3.6 | Ideology Screen | What ideologies are selectable? What are the prerequisites or costs to adopt a new ideology? What are the exact passive effects? | | [ ] |

---

## Section 4: Presidential Tasks & Objectives

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 4.1 | Task Display | How are tasks displayed (List, Accordion, Tree)? Is there a separate "Principal Tasks" vs "Secondary Tasks" tab? | | [ ] |
| 4.2 | Sub-Tasks Tracking | How is progress visualized (Progress bar, X/Y counter)? Does partial progress give partial rewards? | | [ ] |
| 4.3 | Completion & Rewards | Does the reward collect automatically or require clicking a "Claim" button? What happens to completed tasks (archived vs removed)? | | [ ] |
| 4.4 | Task Failure & Timers | Are any tasks time-limited? What alert or visual feedback signals a looming deadline or task failure? | | [ ] |

---

## Section 5: Economy, Budget & Resources

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 5.1 | Tax Rate Controls | What types of taxes are adjustable? What are the slider limits (0% to 100%)? What is the impact of high taxes on Approval and GDP? | | [ ] |
| 5.2 | Loans & Banking | What lenders exist (Central Bank, IMF, Private)? What are the loan amounts, terms, and interest rates? What happens on default? | | [ ] |
| 5.3 | Resource Roster | Confirm exact resource types: Food, Oil, Iron, Electricity, Uranium, others? What unit of measurement is used? | | [ ] |
| 5.4 | Production Facilities | What is the full list of production buildings? What is the cost, build time, and output per building? | | [ ] |
| 5.5 | Deficit & Bankruptcy | What happens if the Treasury balance goes into negative? Can you spend money while in debt? Is there a game-over bankruptcy threshold? | | [ ] |

---

## Section 6: Diplomacy & Foreign Relations

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 6.1 | Diplomatic Actions List | List all interactive diplomatic options when inspecting a foreign nation. | | [ ] |
| 6.2 | Commercial Contracts | How are trade contracts formed? Does the player select specific resource amounts or a fixed tier? How long do contracts last? | | [ ] |
| 6.3 | Non-Aggression Pacts | What is the cost or relation requirement to sign a non-aggression pact? How long does it last? What is the penalty for breaking it early? | | [ ] |
| 6.4 | Diplomatic Gifts | What are the financial amounts for diplomatic gifts? How many relation points does each grant? Is there a cooldown? | | [ ] |
| 6.5 | Alliance Mechanics | What conditions are required for a military alliance? Do AI allies reliably join defensive and offensive wars? | | [ ] |

---

## Section 7: Military, Defense & Combat

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 7.1 | Complete Unit Roster | List all unit types available in the recruitment screen across Army, Navy, Air Force, and Strategic forces. | | [ ] |
| 7.2 | Unit Costs & Upkeep | What is the purchase cost (Money, Resources) and ongoing monthly upkeep for each unit type? | | [ ] |
| 7.3 | Army Drills Mechanic | How does the "Army Drills" button work? What is its financial cost, duration, cooldown, and exact combat readiness bonus? | | [ ] |
| 7.4 | Declaring War | What are the prerequisites to declare war? Does it require a Casus Belli or approval rating threshold? | | [ ] |
| 7.5 | Combat Resolution | How is battle resolved? Is there a tactical battle screen, an animation, or an instantaneous report? How are casualties reported? | | [ ] |
| 7.6 | War Outcomes | What options appear upon defeating an enemy nation (Annex territory, Puppet regime, Demand tribute, Peace)? | | [ ] |

---

## Section 8: Internal Security & Espionage

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 8.1 | Presidential Security | How is personal security funded? What indicators show security level (e.g., 0-100% or Stars)? | | [ ] |
| 8.2 | Assassination Attempts | What triggers an assassination attempt? Can the player make interactive choices during an attempt, or is it automated? | | [ ] |
| 8.3 | Anti-Corruption Operations | What buttons exist to fight corruption? What are their costs and cooling times? How much does corruption reduce budget income? | | [ ] |
| 8.4 | Terrorist Groups | How are terrorist cells displayed? How does counter-terrorism eliminate them? What happens if they are ignored? | | [ ] |
| 8.5 | Espionage Operations | What missions can spies perform? What is the recruitment cost of a spy? What is the capture risk and international fallout? | | [ ] |

---

## Section 9: Events, Crises & Game Over

| Item | Screen & Path | Observation Questions to Answer | Verified Value / Notes | Status |
|---|---|---|---|---|
| 9.1 | Emergency Frequency | How often do random events trigger? (e.g., once every 3 months, or random probability)? | | [ ] |
| 9.2 | Decision Mechanics | Record 3-5 concrete event examples: What was the situation, the available choices, and the exact consequences of each choice? | | [ ] |
| 9.3 | Game Over Conditions | What conditions result in a defeat / game-over screen? (Approval drop to 0%, military conquest, assassination, financial default)? | | [ ] |
| 9.4 | Victory Conditions | Is there a formal victory screen (e.g., Year 2050 reached, world conquest, task completion), or is it an endless sandbox? | | [ ] |
