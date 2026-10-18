# Desktop UI Design System: Rashtra: Sovereign Republic

- **Project:** Rashtra: Sovereign Republic (Windows Desktop Strategy Remake)
- **Primary Persona:** Desktop UI/UX Specialist
- **Status:** Ratified in Sprint 01-02
- **Target Resolution:** 1920 × 1080 (Responsive from 1280 × 720 to 3840 × 2160)

---

## 1. Design Philosophy: Tactical Geopolitical Command

The UI translates the mobile tap-and-reward flow of the reference game into a **premium, information-dense PC command center**:
- **Clarity over Clutter:** High-contrast tactical dark theme with clean visual hierarchy.
- **Immediate State Visibility:** Core sovereign indicators (Treasury, Population, Approval, Living Standard, Date/Clock) are anchored in the persistent Top HUD.
- **Deep Hover Feedback:** Hovering over any metric, minister, or action reveals a comprehensive tooltip showing underlying formulas, ongoing upkeep, and keyboard shortcuts.
- **Explicit Action Confirmation:** Major financial, diplomatic, or military decisions trigger a pre-execution summary dialog displaying costs, prerequisites, and projected net impact.

---

## 2. Color System & Design Tokens

### 2.1 Surfaces & Backgrounds
| Token | Hex Value | Purpose |
|---|---|---|
| `bg-canvas` | `#0B0F17` | Root application background and deep space around the world map. |
| `bg-surface-base` | `#111827` | Base layer for persistent navigation bars, side panels, and drawers. |
| `bg-surface-elevated` | `#1F2937` | Cards, ministry tiles, task accordions, and dialog containers. |
| `bg-surface-hover` | `#374151` | Interactive elements on mouse hover. |
| `border-subtle` | `#374151` | Subtle divider lines, card outlines, table borders. |
| `border-focus` | `#3B82F6` | Keyboard focus ring and selected country boundary highlight. |

### 2.2 Brand & Geopolitical Accents
| Token | Hex Value | Purpose |
|---|---|---|
| `accent-saffron` | `#FF9933` | Primary brand accent, diplomatic emphasis, milestone celebrations. |
| `accent-navy` | `#1D4ED8` | Military command, naval fleet indicators, technology research. |
| `accent-emerald` | `#10B981` | Positive budget surplus, healthcare index, friendly diplomatic status. |
| `accent-gold` | `#F59E0B` | Treasury counter, commercial trade routes, achievements. |
| `accent-crimson` | `#EF4444` | Deficit warning, wartime state, assassination alert, critical unrest. |

### 2.3 Typography Colors
| Token | Hex Value | Purpose |
|---|---|---|
| `text-primary` | `#F9FAFB` | High-contrast display titles, counter values, primary button text. |
| `text-secondary` | `#D1D5DB` | Subtitles, body descriptions, table headers, minister titles. |
| `text-muted` | `#9CA3AF` | Inactive dates, tooltips hints, keyboard shortcut tags. |
| `text-disabled` | `#4B5563` | Unavailable actions, locked tech requirements. |

---

## 3. Typography Scale

The type system pairs **Rajdhani** (technical, angular, geopolitical headers) with **Inter** (neutral, ultra-legible desktop data rendering):

| Level | Font Family | Size | Weight | Line Height | Usage |
|---|---|---|---|---|---|
| `Display 1` | Rajdhani | 32px | Bold (700) | 36px | Main menu title, victory/game-over screens. |
| `Heading 1` | Rajdhani | 24px | SemiBold (600) | 28px | Drawer titles (e.g., "Ministry of Health", "Armed Forces"). |
| `Heading 2` | Rajdhani | 20px | SemiBold (600) | 24px | Card headers, section dividers, event modal titles. |
| `Stat Value` | Rajdhani | 18px | Bold (700) | 20px | Numerical counters in Top HUD ($50.0B, 1.4B, 65%). |
| `Body Large` | Inter | 15px | Medium (500) | 20px | Dialogue options, primary button labels. |
| `Body Regular` | Inter | 13px | Regular (400) | 18px | Task descriptions, policy rules, treaty terms. |
| `Caption / Monospace` | Inter | 11px | Regular (400) | 14px | Tooltip footnotes, date formatting, table data cells. |

---

## 4. Spacing, Layout Grid & Scalability

### 4.1 Base 4px Spacing Scale
- `space-1`: 4px (Tight padding within badges and tags)
- `space-2`: 8px (Inner button padding, icon-to-label gap)
- `space-3`: 12px (Card padding, list item separation)
- `space-4`: 16px (Standard panel margins, drawer internal gutters)
- `space-6`: 24px (Section separation, modal internal margins)
- `space-8`: 32px (Major layout margins)

### 4.2 Desktop Viewport Layout
The game layout is structured into 4 persistent and contextual layers:
1. **Top Header HUD (Persistent, Height: 56px):**
   - Left: Game Title, Date, Speed Controls (||, >, >>, >>>).
   - Center: Primary National Metrics (Treasury, Population, Approval %, Living Standard %).
   - Right: Notification Bell (with active count badge), Settings cog, Pause menu.
2. **Left Navigation Rail (Persistent, Width: 64px collapsed, 240px expanded):**
   - Icon-driven vertical rail: [1] Overview, [2] Government, [3] Tasks, [4] Economy, [5] Diplomacy, [6] Military, [7] Security, [8] Events.
   - Keyboard hotkeys `F1`–`F8` or numbers `1`–`8` provide instant drawer access.
3. **Central World Map Viewport (Flexible Canvas):**
   - Renders 2D interactive world polygons with camera pan/zoom controls.
   - Clamped zoom limits: Min (full globe view), Max (regional boundary inspection).
4. **Right Contextual Drawer (Dynamic, Width: 420px):**
   - Slides smoothly over the right viewport edge when an entity is selected (selected nation, active ministry, selected task).
   - Allows map inspection to remain visible while managing domestic sectors.

---

## 5. Component Specifications

### 5.1 Pre-Action Confirmation Modal
Before any action that expends Treasury, alters treaties, or launches military operations, a standardized confirmation modal is presented:
- **Title:** Action name (e.g., *"Appoint Minister of Health"*).
- **Prerequisite Check:** Green checkmark if met, red X if missing.
- **Cost Summary:**
  - One-time cost: `-$15,000,000` (Red if unaffordable, Gold if affordable).
  - Monthly upkeep impact: `+$250,000 / month`.
- **Projected Impact:** Expected sector index gain (e.g., `+1.5% Health Efficiency`).
- **Controls:** Cancel button (`Esc`) and Confirm button (`Enter`).

### 5.2 Dynamic Emergency Modal
- **Visual Alert:** Banner color dictated by severity (Red for Disasters, Amber for Protests).
- **Situation Brief:** Scenario description and stylized original vector illustration.
- **Decision Matrix:** 2 to 3 actionable choice cards.
  - Hovering a choice highlights its immediate financial cost vs public approval consequences.
- **Countdown Timer:** Non-pausable or timed events show an active progress bar indicating time before default decision is enacted.

### 5.3 Tooltip System
- Appears on hover after a 200ms delay.
- Dark semi-opaque background (`#1F2937`) with subtle border (`#374151`).
- Contains:
  - Metric Title (e.g., *"Standard of Living"*).
  - Current value and monthly trend arrow (`+0.4% / month`).
  - Breakdown equation showing contributions from Health, Education, Culture, and Infrastructure.
  - Shortcut hint (e.g., *"Click to open Welfare overview"*).
