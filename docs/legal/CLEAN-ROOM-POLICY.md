# Clean-Room Engineering & Legal Boundary Policy

- **Project:** Windows Desktop Strategy Remake
- **Working Title:** **Rashtra: Sovereign Republic** *(Alternative Working Titles: Bharat Sovereign, Republic Sim: India)*
- **Reference Game:** *India Simulator 2 / India Simulator 2 Premium* (Developed by Oxiwyle)
- **Status:** Ratified in Sprint 00-02
- **Authoritative Personas:** Product Owner, DevOps/Release Engineer, Game/Simulation Architect

---

## 1. Core Clean-Room Doctrine

This project is an independent, clean-room software re-implementation of the functional gameplay experience observed in the reference game, tailored for Windows desktop.

Under established international copyright doctrine (including 17 U.S.C. § 102(b), the Indian Copyright Act 1957 § 13, and the Berne Convention), **game rules, mathematical models, operational systems, and abstract mechanics are not copyrightable**. Copyright protects only the **creative expression** (specific artwork, sprites, 3D models, audio tracks, voice acting, UI layout drawings, written prose, and proprietary source code).

### The Golden Rule
> **"We recreate the functional mechanics and state loops; we independently author 100% of the code, visual assets, UI layouts, typography, audio, and narrative text."**

---

## 2. Branding & Identity Disambiguation

To eliminate any risk of trademark infringement, brand confusion, or passing-off:

1. **Title Separation:**
   - The project shall **never** use the titles *"India Simulator 2"*, *"Simulator of India"*, or *"Oxiwyle"* in any public marketing, store listing, window title, or executable metadata.
   - Primary working title: **Rashtra: Sovereign Republic**.
   - Subtitle: *Geopolitical Grand Strategy*.
2. **Visual Branding:**
   - Custom original iconography (National emblems, Ashoka Chakra-inspired geometric vectors, clean-line modern desktop insignia).
   - Distinctive desktop visual language (dark glassmorphism, geopolitical tactical command palette) rather than mobile tap-and-reward styling.
3. **Trademark Clearance:**
   - Pre-release legal review will verify the final public commercial title against trademark registries.

---

## 3. Functional Mechanics vs. Expressive Elements

The following demarcation governs all development:

| Domain | Permissible Functional Recreation (Clean-Room Scope) | Prohibited Expressive Copying (Must Be Independently Authored) |
|---|---|---|
| **Code** | Independently written C# domain models, state machines, math formulas, command handlers. | Decompiled reference code, reverse-engineered bytecode, or decompiled IL/C# scripts. |
| **User Interface** | Functional layout concepts: Top status bar, collapsible drawers, interactive 2D world map, sliders. | Copying reference UI sprite textures, icons, buttons, window skins, or exact layout pixel geometry. |
| **Icons & Art** | Generic concept icons: Heart for Health, Book for Education, Shield for Defense, Coin for Treasury. | Copying, tracing, or screenshot-ripping reference SVG/PNG icon assets. |
| **Typography** | Clean, readable desktop sans-serif fonts for data density. | Extracting proprietary embedded TTF/OTF font files from reference APK. (Use SIL OFL fonts like *Inter* or *Rajdhani*). |
| **Map & Geography** | World country boundaries, geopolitical state status, capital coordinates. | Copying reference texture map skins or proprietary tile sets. (Use Natural Earth Public Domain vectors). |
| **Audio & Music** | Ambience appropriate for geopolitical simulation: UI clicks, paper rustles, subtle drones. | Extracting reference BGM/SFX audio files. (Use original recordings or CC0/permissive audio). |
| **Game Text & Dialogue** | Conceptual scenarios: Drought in agricultural sector, border skirmish, ministerial corruption. | Copying exact verbatim narrative strings, event descriptions, or localization tables from reference APK. |
| **Progression Rules** | 13 cabinet ministries, turn-based monthly accounting, resource loops (Food, Oil, Iron, Uranium). | Copying exact unique naming quirks or proprietary trademarks. |

---

## 4. Handling Reference Media & Gameplay Captures

During research and QA verification:
1. **Quarantine:** All reference gameplay recordings, screenshots, and decompiled APK teardown notes must reside exclusively in quarantined directories:
   - `reference_media/`
   - `reference_captures/`
   - `scratch/reference/`
2. **Git Exclusion:** These folders are strictly excluded from git tracking via `.gitignore`.
3. **Zero Asset Contamination:** No file originating from the reference game APK may ever be placed into `src/`, `content/`, or game resource folders (`res://` in Godot).
4. **Build Verification:** CI/DevOps build scripts must include an automated asset provenance check that fails the build if untracked binary assets without approved license metadata are detected.

---

## 5. Asset Rules & Licensing Requirements

Every asset incorporated into the project repository must satisfy one of the following provenance categories:

1. **Category A — Original Authoring:**
   - Created from scratch by the project team (e.g., custom C# code, vector icons created in Inkscape/Figma, original shaders, UI themes).
2. **Category B — Permissive Open Source / Public Domain:**
   - Permissive licenses explicitly verified: MIT, Apache 2.0, CC0 1.0 Universal, Unlicense, or Public Domain.
   - Example: **Natural Earth** vector boundary data (Public Domain, unrestricted commercial use).
3. **Category C — Permissive Fonts (SIL Open Font License):**
   - Free for commercial and non-commercial desktop game embedding (e.g., *Inter*, *Rajdhani*, *Outfit*).
4. **Prohibited Licenses:**
   - Assets under GPL (with copyleft affecting game binaries), CC-BY-NC (Non-Commercial), CC-BY-ND (No Derivatives), or unverified scrapings from the web are strictly forbidden.

All approved assets must be cataloged in [`docs/legal/ASSET-AND-DATA-PROVENANCE.md`](file:///c:/Workspace/India-Sim/docs/legal/ASSET-AND-DATA-PROVENANCE.md).

---

## 6. Legal Decision Log & Approval Gates

Any uncertainty regarding asset licensing, naming, or border representation must be escalated to the Human Owner through the following formal gates:

- **Gate 1:** Working title approval and trademark clearance before marketing/packaging.
- **Gate 2:** Vector map dataset approval before Phase 02 geometry import.
- **Gate 3:** External audio/font bundle license verification before Phase 08 polish.
- **Gate 4:** Final pre-release clean-room audit before public distribution.
