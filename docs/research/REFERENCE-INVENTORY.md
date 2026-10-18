# Reference Feature Inventory: India Simulator 2 (Oxiwyle)

- **Reference Title:** India Simulator 2 / India Simulator 2 Premium
- **Original Developer:** Oxiwyle
- **Target Platform:** Mobile (Android / iOS)
- **Remake Platform:** Windows Desktop
- **Research Status:** Clean-room functional reference compilation based on published developer specifications, official app descriptions, verified walkthroughs, and gameplay video captures. Direct local execution on developer workstation is constrained due to Android environment dependency; a companion human verification checklist is provided in `docs/research/REFERENCE-RESEARCH-CHECKLIST.md`.
- **Confidence Legend:**
  - `Observed`: Directly confirmed via official developer descriptions, verified gameplay recordings, or published walkthroughs.
  - `Inferred`: Logically derived from core Oxiwyle simulator architecture and adjacent titles (*President Simulator*, *Modern Age 1 & 2*, *Russia Simulator 2*).
  - `Unknown`: Exact numeric coefficients, formulas, timers, or UI transitions pending live APK inspection.

---

## 1. Launch, Shell & System Screens

### 1.1 Main Menu
- **Navigation Path:** App Startup -> Splash Screen (Oxiwyle logo) -> Main Menu
- **Visual Presentation:** Country-themed presidential title screen with high-contrast menu buttons and background patriotic/geopolitical imagery.
- **Available Actions:**
  - `New Game`: Initializes a new simulation campaign. Opens Country Selection / Setup.
  - `Continue / Load Game`: Opens saved games slot selector or resumes latest autosave.
  - `Settings`: Opens audio, display, language, and system configuration.
  - `Achievements`: Opens list of unlocked and locked presidential achievements.
  - `Exit / Quit`: Closes the application (on desktop; on mobile returns to OS).
- **Persistence:** High scores, global achievement progress, settings stored in local app storage.
- **Confidence Status:** `Observed` (Menu items verified); `Inferred` (Specific slot management details); `Unknown` (Autosave frequency during menu transitions).

### 1.2 Country Selection Screen
- **Navigation Path:** Main Menu -> `New Game` -> Country Selection
- **Visual Presentation:** World map view or scrollable country list with flags and baseline national stats.
- **Available Actions:**
  - Select Country: In *India Simulator 2*, India is pre-selected as the protagonist nation; other countries are selectable or unlockable depending on game mode / premium status.
  - Inspect Country Stats: Shows starting Population, Treasury/Gold balance, Territorial size, Initial Quality of Living, Geopolitical Rank.
  - Confirm / Start: Launches the new campaign with the chosen nation's baseline state.
- **Preconditions:** New game start; locked nations require achievement or premium unlock in original (for remake: configurable).
- **Confidence Status:** `Observed` (India primary nation, starting stats display); `Unknown` (Exact initial values for all secondary nations).

### 1.3 Settings Screen
- **Navigation Path:** Main Menu / In-game Pause -> Settings
- **Available Actions:**
  - Audio Volume: Separate or combined sliders for Background Music (BGM) and Sound Effects (SFX).
  - Language: Multi-language selection (English, Hindi, Russian, etc.).
  - Visual / Display: Graphics quality, notification toggles, fullscreen/windowed (desktop adaptation).
  - Save / Reset Data: Cloud save sync (mobile) or local reset.
- **Confidence Status:** `Observed` (Audio, language, notifications); `Inferred` (Desktop windowed/fullscreen adaptation required).

---

## 2. Main Gameplay Screen & World Map HUD

### 2.1 Top Dashboard / Status Bar (Always Visible)
- **Navigation Path:** Top viewport header across all gameplay tabs.
- **Displayed Metrics:**
  - **Date & Time:** Day, Month, Year counter.
  - **Game Speed Controls:** Pause (||), 1x Speed (>), 2x Fast-Forward (>>), 3x/Max (>>>).
  - **Treasury / National Funds (Gold/Currency icon):** Current liquid balance in millions/billions. Color indicator for positive/negative balance.
  - **Population (People icon):** Total national population count with growth/decline indicator.
  - **President Approval Rating (% icon):** Public approval scale (0% to 100%). Dropping below critical threshold (~20-30%) risks coup or forced resignation.
  - **Quality of Living / Standard of Living (% icon):** Aggregate civilian welfare indicator (0% to 100%), improved via healthcare, education, culture, and infrastructure.
  - **Alerts / Notifications Indicator:** Bell icon with counter for active crises, events, or completed tasks.
- **Feedback & Edge Cases:**
  - Deficit spending produces red treasury numbers and warning notifications.
  - Severe drops in approval trigger civil unrest and strikes.
- **Confidence Status:** `Observed` (HUD layout, primary indicators, speed controls); `Unknown` (Exact formula for Quality of Living aggregation).

### 2.2 Interactive World Map
- **Navigation Path:** Main Viewport (central screen area).
- **Visual Presentation:** 2D interactive world map with nation boundaries, national colors, capitals, and geopolitical status overlays.
- **User Actions:**
  - Pan / Zoom: Mouse drag / scroll wheel (desktop), pinch-to-zoom (mobile).
  - Select Nation: Clicking any country opens its Foreign Relations / Diplomacy drawer.
  - Visual Status Layers: Friendly (Green), Neutral (Yellow/Gray), Hostile (Orange), At War (Red).
  - Territorial Annexation: Conquered territories change border colors to the occupying power.
- **Confidence Status:** `Observed` (Map interaction, country selection, diplomatic color coding); `Inferred` (Desktop mouse controls); `Unknown` (Disputed boundary display nuances in reference APK).

---

## 3. Government & Institutions System

### 3.1 Ministerial Cabinet Management
- **Navigation Path:** Navigation Menu -> Government -> Ministries
- **Core Ministries:**
  1. *Ministry of Health:* Constructs clinics, hospitals, epidemic response, raises life expectancy.
  2. *Ministry of Education:* Schools, universities, literacy programs, technological research speed.
  3. *Ministry of Infrastructure:* Roads, transport hubs, regional connectivity, utility networks.
  4. *Ministry of Culture:* Theaters, festivals, national heritage sites, morale boosts.
  5. *Ministry of Sports:* Stadiums, national training facilities, international tournament hosting (World Cup, Olympics).
  6. *Ministry of Tourism:* Landmark preservation, resort zones, foreign tourist influx, commercial revenue.
  7. *Ministry of Justice & Courts:* Legal administration, rule of law, anti-crime sentencing.
  8. *Ministry of Internal Affairs / Police:* Local police stations, crime reduction, riot suppression.
  9. *Ministry of Defense:* Armed forces readiness, military base administration, drill operations.
  10. *Security Service / Intelligence:* Secret service, counter-espionage, assassination protection.
  11. *Ministry of Finance / Central Bank:* Tax policy, debt issuance, IMF loan management.
  12. *Ministry of Industry & Construction:* Factory oversight, raw material refinement, building projects.
  13. *Ministry of Environment:* Pollution control, renewable transition, disaster recovery.
- **User Actions per Ministry:**
  - Appoint / Replace Minister: Select a candidate with specific loyalty, competence, and salary attributes.
  - Adjust Ministry Budget: Slide funding level (Low, Medium, High, Max) which scales operational output and monthly cost.
  - Execute Sector Reforms / Projects: One-time capital projects (e.g., "Build 50 Polyclinics", "Modernize Highway Grid").
- **State Changes:** Higher funding improves sector index (e.g. Health index) and raises national Quality of Living; low funding triggers worker strikes and lowers approval.
- **Confidence Status:** `Observed` (Ministry list, appointment mechanic, public sector projects); `Unknown` (Exact algorithmic formula linking funding level to monthly index increase).

### 3.2 Ideology & State Policy
- **Navigation Path:** Navigation Menu -> Government -> Ideology
- **Options Available:**
  - Democratic / Secular Republic
  - Socialist / Planned Economy
  - Nationalist / Authoritarian
  - Theocratic / Religious State
- **Mechanics:**
  - Changing ideology costs substantial political capital / approval and may destabilize diplomatic relations with contrasting power blocs.
  - Grants specific passive buffs (e.g., Socialism boosts health/equality at the cost of private industry revenue; Capitalism boosts tax revenues at the cost of baseline inequality).
- **Confidence Status:** `Observed` (Ideology selection exists); `Inferred` (Specific buff values based on Oxiwyle system standard); `Unknown` (Exact cooldown period and transition requirements between ideologies).

---

## 4. Presidential Tasks & Objectives System

### 4.1 Principal Tasks & Sub-Tasks
- **Navigation Path:** Navigation Menu -> Tasks & Objectives
- **Structure:**
  - Hierarchical structure: Major Strategic Goal -> Multiple Component Sub-tasks.
  - Examples:
    - *Economic Miracle:* Sub-tasks: Achieve $50B GDP surplus; construct 20 power plants; maintain inflation under 5%.
    - *Superpower Defense:* Sub-tasks: Train 100,000 active troops; build 10 military bases; conduct 5 successful army drills.
    - *Universal Welfare:* Sub-tasks: Increase literacy to 90%; construct 200 regional hospitals; eradicate extreme poverty.
- **Task Lifecycle:**
  - `Not Started` -> `In Progress` -> `Completed` -> `Claimed / Archived`.
  - Failure conditions: Time-limited tasks fail if the deadline expires, imposing an approval or economic penalty.
- **Rewards:**
  - Instant Treasury grant.
  - Permanent or temporary national Approval boost.
  - In-game Achievement point unlock.
- **Confidence Status:** `Observed` (Task hierarchy, sub-tasks, completion tracking, achievement linkage); `Unknown` (Complete roster of all 50+ individual tasks in the original script).

---

## 5. Economy, Production & Central Bank

### 5.1 Central Bank & Fiscal Policy
- **Navigation Path:** Navigation Menu -> Economy -> Central Bank
- **User Controls:**
  - Tax Rate Sliders: Income Tax, Corporate Tax, Value Added Tax (VAT), Resource Royalty Tax.
  - Loans & Debt:
    - Borrowing from Central Bank or International Monetary Fund (IMF).
    - Repayment schedule with interest rate.
    - Risk: Over-borrowing leads to sovereign default warning, currency devaluation, and credit rating downgrade.
- **Confidence Status:** `Observed` (Tax rate adjustments, loans from IMF/banks); `Unknown` (Exact interest calculation formula and default penalty threshold).

### 5.2 Resource Production & Facilities
- **Navigation Path:** Navigation Menu -> Economy -> Production
- **Primary Resource Types:**
  1. *Food (Agriculture):* Grain, livestock, fisheries. Consumed by national population monthly; deficits cause famine and rapid approval drop.
  2. *Electricity / Energy:* Megawatt/Gigawatt capacity produced by thermal, hydroelectric, solar, and nuclear power plants. Required to operate factories and cities.
  3. *Oil / Petroleum:* Extracted via oil wells/derricks. Essential for vehicle fleets, domestic industry, and high-value export.
  4. *Iron & Steel:* Extracted via mines, refined in smelters. Essential for infrastructure projects and military equipment.
  5. *Uranium:* Mined in specialized deposits. Required for nuclear power plants and advanced strategic weaponry.
  6. *Consumer Goods:* Manufactured in light factories to sustain living standards.
- **Production Buildings:**
  - Farms / Agricultural Complexes
  - Mines (Iron, Uranium)
  - Oil Derricks / Refineries
  - Power Stations (Thermal, Hydro, Green/Solar, Nuclear)
  - Industrial Factories
- **User Actions:**
  - Build X units of facility (costs Treasury funds + Iron/Steel + time).
  - Upgrade facility efficiency.
  - Demolish / Decommission obsolete facilities.
- **Confidence Status:** `Observed` (Resource roster: Food, Oil, Iron, Electricity, Uranium; facility construction loop); `Unknown` (Exact construction turn times and unit input/output ratios).

---

## 6. Diplomacy & Foreign Affairs

### 6.1 Bilateral Relations & International Actions
- **Navigation Path:** Navigation Menu -> Diplomacy OR Click Country on World Map
- **Actions Available per Country:**
  - `Trade Agreement / Commercial Contract`: Export surplus resources (e.g., Oil, Food) in exchange for monthly Treasury income; import deficit goods.
  - `Non-Aggression Pact`: Formal treaty preventing hostilities for a specified duration (e.g., 12-24 months).
  - `Mutual Defense Alliance`: Formal military partnership where allies join defensive wars.
  - `Diplomatic Gift / Financial Aid`: One-time transfer of funds to raise relations score (+5 to +20).
  - `Cultural / Trade Delegation`: Low-cost recurring action to improve bilateral standing.
  - `Denounce / Expel Diplomats`: Decreases relations; acts as prelude to conflict.
  - `Declare War`: Opens formal state of war.
- **Diplomatic Status Scale:**
  - Scale: -100 (Hostile / War) to +100 (Allied / Unbreakable).
- **Confidence Status:** `Observed` (Commercial contracts, non-aggression pacts, war declaration, relation scale); `Unknown` (Exact decay rate of relations per month).

---

## 7. Military, Warfare & Defense Operations

### 7.1 Military Hardware & Unit Roster (>10 Unit Types)
- **Navigation Path:** Navigation Menu -> Military -> Armed Forces
- **Unit Categories & Types:**
  1. *Infantry:* Regular Conscripts, Professional Soldiers, Special Operations Forces.
  2. *Armored Warfare:* Tanks, Armored Personnel Carriers (APCs), Mobile Artillery.
  3. *Aviation:* Fighter Jets (air superiority), Attack Helicopters, Strategic Bombers.
  4. *Naval Fleet:* Warships (Frigates/Destroyers), Submarines, Aircraft Carriers.
  5. *Strategic Rocket Forces:* Surface-to-Air Missile (SAM) defense systems, Ballistic Missiles, Nuclear Warheads.
- **Military Facilities Required:**
  - Barracks & Arsenals (Infantry & Artillery)
  - Tank Plants / Depots (Armor)
  - Military Airfields & Hangars (Aviation)
  - Naval Shipyards & Docks (Fleet)
- **User Actions:**
  - Recruit / Manufacture Units: Spend Treasury + Raw Resources (Iron, Oil).
  - Army Drills: Conduct training drills to boost army readiness, combat effectiveness, and general morale.
  - Disband Units: Cut monthly maintenance costs.
- **Confidence Status:** `Observed` (Over 10 military weaponry types, drills mechanic, equipment construction); `Unknown` (Exact combat formula and dice/calculation mechanics during battles).

### 7.2 War & Conflict Mechanics
- **Navigation Path:** Triggered when at war with a target nation.
- **Operations:**
  - Launch Offensive: Direct army divisions into enemy territory.
  - Air Strikes: Bomb enemy industrial facilities or defense fortifications.
  - Naval Blockade: Cut off enemy maritime trade routes and commercial contracts.
  - Nuclear Strike: High-stakes strategic strike (severely degrades international reputation and triggers global sanctions).
- **Resolution:**
  - Total Defeat -> Territorial Annexation / Puppet Government / Peace Treaty with Reparations.
- **Confidence Status:** `Observed` (Wars, territorial expansion, annexations); `Unknown` (Exact damage calculation and occupation resistance formulas).

---

## 8. Internal Security, Espionage & Crisis Management

### 8.1 Presidential Security & Counter-Corruption
- **Navigation Path:** Navigation Menu -> Security
- **Systems:**
  - **Presidential Protection Detail:** Allocate funding to guard units, armored motorcades, safe houses. Low funding increases assassination risk during crises.
  - **Anti-Corruption Campaigns:** Direct special prosecutors and police units to arrest corrupt officials. Increases state revenue retention and boosts public trust.
  - **Counter-Terrorism:** Investigate, infiltrate, and eliminate insurgent cells operating within the territory.
- **Confidence Status:** `Observed` (Personal security funding, anti-corruption campaigns, counter-terrorism); `Unknown` (Exact trigger probability for assassination attempts).

### 8.2 Foreign Espionage
- **Navigation Path:** Navigation Menu -> Espionage
- **Actions:**
  - Recruit Spies & Intelligence Agents.
  - Dispatch Agents to Foreign Nations:
    - Gather military readiness data and troop counts.
    - Steal technological research.
    - Sabotage enemy production plants or power grids.
    - Incite political protests in enemy territory.
  - Counter-Espionage: Uncover and execute foreign spies operating inside India.
- **Confidence Status:** `Observed` (Espionage operations, enemy army reconnaissance, sabotage); `Unknown` (Success probability calculations and capture consequences).

---

## 9. Events, Emergencies & Notifications

### 9.1 Dynamic National Emergencies
- **Trigger:** Monthly turn processing based on random seed and national indicators.
- **Categories:**
  - Natural Disasters: Earthquakes, major monsoon floods, prolonged agricultural droughts.
  - Public Crises: Disease epidemics, industrial chemical leaks, infrastructure collapses.
  - Social & Political Unrest: General strikes, student riots, separatist uprisings.
- **Resolution Flow:**
  - Modal dialogue appears: Title, description, illustration, and 2-3 actionable choices.
  - Choice A: Rapid emergency relief (High Treasury cost, protects Approval).
  - Choice B: Partial aid / local management (Moderate cost, slight Approval drop).
  - Choice C: Ignore / security crackdown (Zero financial cost, severe Approval drop and unrest).
- **Confidence Status:** `Observed` (Emergency popups, multiple choice responses, cost/approval trade-offs); `Unknown` (Complete catalog of all event scripts).

---

## 10. Persistence, Saves & Simulation Clock

### 10.1 Simulation Time System
- **Cadence:** Discrete day/month calendar clock.
- **Speed Options:** Paused, 1x (Normal), 2x (Fast), 3x (Ultra).
- **Turn Updates:** Monthly economic accounting (taxes collected, maintenance deducted, production added to stockpiles, demographic changes applied).
- **Confidence Status:** `Observed` (Calendar clock, speed controls, monthly accounting); `Unknown` (Exact milliseconds per tick at each speed level).

### 10.2 Save & Load Management
- **Slots:** Multiple manual save slots plus automatic periodic save (Autosave).
- **Saved State Boundary:**
  - National treasury, resource stockpiles, population stats.
  - Complete world country registry with diplomatic relations and territorial boundaries.
  - Active minister appointments, funding levels, and completed reforms.
  - Military inventory, deployed forces, combat readiness score.
  - Active tasks, completed subtasks, and unlocked achievements.
  - Game calendar date and simulation speed setting.
- **Confidence Status:** `Observed` (Save/load existence); `Inferred` (Comprehensive state boundary needed for deterministic desktop parity); `Unknown` (Mobile proprietary save format details).
