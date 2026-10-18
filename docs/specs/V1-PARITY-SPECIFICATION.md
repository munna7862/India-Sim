# V1 Gameplay Parity Specification & Desktop Platform Baseline

- **Project:** Rashtra: Sovereign Republic (Desktop Remake)
- **Reference Game:** *India Simulator 2 Premium* (Oxiwyle)
- **Status:** Ratified in Sprint 00-03
- **Authoritative Personas:** Product Owner, SDET/QA Engineer, Game/Simulation Architect

---

## 1. What "Same Gameplay" Means (Feature-by-Feature)

Version 1 is committed to **functional gameplay parity**. The user experience must faithfully reproduce the mechanics, inputs, state transformations, feedback, and constraints observed in the reference title.

### 1.1 Core Simulation Rules & Action Outcomes
- **Rule Determinism:** Given identical starting state, identical calendar progression, and identical player commands, the simulation engine produces deterministic, reproducible outcomes.
- **Atomic Command Validation:** Every player action (appointing a minister, signing a treaty, ordering troops, constructing a factory) is evaluated through an authoritative domain pipeline before state mutation occurs:
  1. Actor validity & sovereign jurisdiction check.
  2. Precondition check (diplomatic standing, technological prerequisites).
  3. Cost validation (adequate Treasury balance, required raw materials).
  4. Cooldown / timer status.
  5. State transition application & event emission.
- **Invalid Action Handling:** Invalid commands must be rejected gracefully with clear, typed domain failure reasons (e.g., `InsufficientFunds`, `TreatyActive`, `AlreadyAppointed`) without throwing unhandled exceptions or corrupting state.

### 1.2 Presidential Task Flows & Progression
- **Hierarchical Objectives:** Principal strategic goals decompose into discrete, trackable sub-tasks.
- **Sub-task Evaluation:** Progress is evaluated reactively or upon calendar tick against current authoritative state counters (e.g., `HospitalsBuilt >= 20`).
- **Reward Non-Duplication:** Each completed task can only award its completion grant (Treasury bonus, Approval points, Achievement ID) once per game session.

### 1.3 Economy Loop & Monthly Accounting Invariants
- **Monthly Turn Resolution:** On the first day of each calendar month, the economy engine executes an invariant accounting ledger:
  $$\text{Net Delta} = \text{Tax Revenue} + \text{Trade Income} - \text{Ministry Upkeep} - \text{Military Maintenance} - \text{Debt Service}$$
- **Resource Depletion & Stockpiling:**
  - Production from facilities (Farms, Mines, Derricks, Power Plants) is credited to national stockpiles.
  - Domestic civilian consumption (Food, Electricity, Consumer Goods) is deducted. Deficits trigger immediate civilian welfare and approval penalties.
- **Debt & Bankruptcy:** Unserviced debt increases interest obligations and penalizes international credit ratings.

### 1.4 Military Arsenal, Upkeep & Army Drills
- **Roster Parity:** Supports all verified weapon categories (>10 types across Infantry, Armor, Aviation, Navy, Strategic Rockets).
- **Recruitment Pipeline:** Training consumes Treasury funds and material resources (Iron, Oil) over a designated duration.
- **Army Drills:** Periodic military exercise that consumes a one-time financial fee and applies a timed combat readiness buff to armed forces.
- **Combat Resolution:** When in a state of war, engagements are resolved by comparing combat power, readiness modifiers, and unit composition, producing casualty reports and territory shifts.

### 1.5 Diplomacy & Bilateral Treaties
- **Relations Spectrum:** Fixed scale from -100 (War) to +100 (Allied).
- **Treaty Expiry & Cancellation:** Non-Aggression Pacts remain active for their stipulated duration. Unilateral violation triggers severe domestic approval loss and global diplomatic denunciation.
- **Commercial Contracts:** Bilateral trade agreements persist monthly until canceled or until the exporting nation suffers a supply deficit.

### 1.6 Dynamic Events & Crisis Resolution
- **Random Trigger Engine:** Monthly evaluation of event triggers weighted by national health, poverty, inflation, and random seed.
- **Multi-Choice Decision Trade-offs:** Emergency popups require deliberate player selection, balancing immediate Treasury expenditure against long-term Approval and Stability ratings.

### 1.7 Persistence & Save/Load Invariants
- **Complete Domain Serialization:** A saved game captures the full simulation state snapshot: national indicators, calendar date, resource stockpiles, cabinet appointments, building inventories, active treaties, task progress, and military rosters.
- **Round-Trip Invariant:** Serializing to JSON and deserializing back must produce an identical domain state object (`State(Load(Save(State))) == State`).

---

## 2. Windows Desktop Platform Specification

The remake adapts the mobile touch interface into an information-dense, high-precision desktop experience:

| Specification | Desktop Requirement | Rationale |
|---|---|---|
| **Target OS** | Windows 10 (version 1809+) & Windows 11 (x64) | Primary gaming desktop platforms. |
| **Baseline Resolution** | 1920 × 1080 (Full HD, 16:9 aspect ratio) | Modern desktop standard. UI adapts down to 1280 × 720 and up to 4K (3840 × 2160). |
| **Window Modes** | Borderless Fullscreen, Exclusive Fullscreen, and Resizable Windowed | Player preference for multitasking. |
| **Frame Rate** | 60 FPS target; VSync enabled by default | Smooth map panning, UI transitions, and low CPU/GPU power consumption. |
| **Input Schema** | Primary: Mouse (Click, Drag, Scroll Wheel for map zoom). Keyboard hotkeys for power users (Space = Pause/Resume, 1/2/3 = Speed, Esc = Close drawer/pause). | Desktop usability first. Tooltips on hover for all buttons and metrics. |
| **Network & Connectivity**| 100% Offline, Single-Player | Zero network calls, zero online telemetry, zero cloud DRM dependencies. |
| **Save Location** | Windows standard: `%APPDATA%\RashtraSovereignRepublic\saves\` | Standard Windows desktop user data directory. |
| **Packaging & Delivery** | Standalone portable ZIP and Windows MSI / Inno Setup installer | Frictionless installation and clean uninstallation. |

---

## 3. Test Strategy & Acceptance Gate Template

Every feature implemented across Phases 01–09 must pass through this acceptance template:

### Feature Acceptance Verification Template

```markdown
### Parity Gate: [Feature ID & Name]
- **Target Sprint:** [e.g., Sprint 04-01]
- **Specialist Owner:** [e.g., Government/Tasks Specialist]
- **Reference Evidence:** [Reference Inventory section & confidence status]

#### Verification Checklist
- [ ] **Unit Tests:** Command validation, boundary conditions, edge cases (zero/negative values).
- [ ] **State Invariant Tests:** Domain state mutations are mathematically consistent and deterministic.
- [ ] **Save/Load Test:** Feature state serializes and deserializes without loss.
- [ ] **UI Presentation:** Desktop HUD / drawer displays authoritative domain state; tooltips and hotkeys functional.
- [ ] **Build Validation:** Project compiles cleanly with 0 warnings/errors in Release configuration.
```
