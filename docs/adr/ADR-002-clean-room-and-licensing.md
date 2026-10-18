# ADR-002: Clean-Room Boundaries, Working Title, and Geospatial Licensing

- **Status:** Accepted
- **Date:** Phase 00, Sprint 00-02
- **Deciders:** Product Owner, Lead Game Architect, DevOps / Release Engineer, Sprint Coordinator

---

## Context

The objective of Version 1 is to reproduce the functional gameplay experience of *India Simulator 2 Premium* on Windows desktop. However, to guarantee total intellectual property integrity, prevent trademark infringement, and eliminate legal risk, the project must establish unambiguous clean-room boundaries. Additionally, the project requires an independent working title, a vector map data source with an unencumbered commercial license, and strict rules prohibiting the shipment of reference game assets.

---

## Decisions

1. **Clean-Room Engineering Mandate:**
   - The team will strictly follow clean-room software re-implementation principles.
   - Non-copyrightable functional mechanics (turn cycles, 13 cabinet ministries, resource accounting, tasks, diplomacy, unit recruitment) will be replicated functionally.
   - All code, UI artwork, icons, typography, audio, and narrative text will be authored 100% independently from scratch.
   - Reference gameplay screenshots and videos will be quarantined in `.gitignore`-excluded folders and never packaged into distributed game builds.

2. **Working Title & Branding Disambiguation:**
   - Adopt **"Rashtra: Sovereign Republic"** as the primary working title to completely differentiate the product from Oxiwyle's trademarked titles (*"India Simulator 2"*, *"President Simulator"*).
   - Use desktop-focused tactical grand strategy aesthetics (Ashoka geometric vector emblems, dark glassmorphism) rather than mobile arcade/tap presentation.

3. **Geospatial Vector Data Source:**
   - Select **Natural Earth Admin 0 Countries** (1:110m and 1:50m scale vectors) as the authoritative world map source.
   - Natural Earth is explicitly in the **Public Domain**, freeing the project from the copyleft/share-alike restrictions of OpenStreetMap (ODbL) and commercial proprietary map services.

4. **Typography & Iconography Licensing:**
   - Pinned typography: *Inter* (UI/body) and *Rajdhani* (headers/metrics) under the **SIL Open Font License 1.1**.
   - Pinned iconography: Custom vectors supplemented by *Lucide/Feather* vectors under **MIT/ISC**.

---

## Consequences

### Positive
- Total freedom from intellectual property, trademark, or copyright infringement claims.
- Zero copyleft obligations on game binaries or content datasets.
- Clean separation between internal reference notes and shipped game assets.
- A distinctive, premium desktop-first identity tailored for mouse/keyboard PC gaming.

### Negative / Trade-offs
- Requires generating or curating 100% original graphics and iconography rather than importing existing mobile sprites.
- Natural Earth vector geometries require pre-processing (simplification and coordinate transformation to Godot 2D space) during Phase 02.
