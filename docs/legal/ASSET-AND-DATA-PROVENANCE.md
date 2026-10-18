# Asset and Data Provenance Register

- **Project:** Rashtra: Sovereign Republic (Desktop Remake)
- **Document Status:** Authoritative Asset & License Tracker
- **Maintained By:** DevOps / Release Engineer & World Map / Data Specialist

---

## 1. Registry Policy

Every third-party dependency, external dataset, vector shape, font, and audio sample incorporated into the repository or distributed package must have a verified entry in this register. Unregistered or proprietary assets will fail the CI release audit.

---

## 2. Approved Provenance Registry

| Asset ID | Category | Asset Name & Version | Source & Origin | License Type | Commercial Use Approved? | Attribution Required? | Verification Status | Notes & Usage |
|---|---|---|---|---|---|---|---|---|
| ENG-01 | Engine | Godot Engine 4.x .NET | [godotengine.org](https://godotengine.org) | MIT License | Yes | Yes (Include MIT notice in credits) | Approved | Windows desktop host, UI, scene graph, rendering. |
| ENG-02 | Runtime | .NET 8 SDK / BCL | [github.com/dotnet](https://github.com/dotnet) | MIT License | Yes | Yes (Standard Microsoft MIT) | Approved | Pure domain logic, unit tests, serialization. |
| GEO-01 | Geospatial | Natural Earth Admin 0 — Countries (1:110m & 1:50m) | [naturalearthdata.com](https://www.naturalearthdata.com) | Public Domain (CC0 equivalent) | Yes | No (Voluntary credit appreciated) | Approved | Baseline world country polygons, boundaries, and sovereign IDs. |
| FNT-01 | Typography | Inter Variable Font Family | [github.com/rsms/inter](https://github.com/rsms/inter) | SIL Open Font License 1.1 (OFL) | Yes | Yes (Preserve OFL copyright text) | Approved | Primary desktop UI, dashboards, data tables, and tooltips. |
| FNT-02 | Typography | Rajdhani Font Family | [Indian Type Foundry / Google Fonts](https://github.com/itfoundry/rajdhani) | SIL Open Font License 1.1 (OFL) | Yes | Yes (Preserve OFL copyright text) | Approved | Display headers, geopolitical title branding, and military gauges. |
| ICN-01 | Iconography | Lucide Icons / Feather | [lucide.dev](https://lucide.dev) | ISC / MIT License | Yes | Yes (License notice in third-party credits) | Approved | Clean vector symbols for budget, health, defense, energy, and alerts. |
| GEO-02 | Country Data | ISO 3166-1 Country Code Standard | International Organization for Standardization | Public Geopolitical Standard | Yes | No | Approved | Canonical 3-letter alpha-3 country identifiers (e.g. IND, USA, CHN). |
| AUD-01 | Audio | Custom & CC0 Ambient Sound Library | Curated Freesound CC0 / Original synthesis | CC0 1.0 Universal / Original | Yes | No (Optional) | Pending Phase 08 | Desktop button clicks, map zooms, alert chimes, ambient drones. |
| COD-01 | Codebase | Original Remake Source Code | Internal clean-room implementation | MIT License | Yes | Yes (Repository License) | Approved | 100% independent C# domain and Godot integration scripts. |

---

## 3. Map Data & Boundary Verification Policy

1. **Dataset Selection: Natural Earth vs OpenStreetMap:**
   - **Decision:** Natural Earth is selected as the primary dataset.
   - **Rationale:** Natural Earth cultural vectors are explicitly dedicated to the **Public Domain**. Unlike OpenStreetMap (which carries the Open Database License / ODbL requiring database share-alike provisions), Natural Earth data can be modified, packaged into binary game scenes, and distributed without any copyleft obligations.
2. **Disputed Boundaries:**
   - The game client will render boundaries in accordance with recognized international administrative datasets, with configurable display options in game settings if regional compliance requires tailored views.
   - Polygon geometries will be normalized to WGS84 coordinates and simplified for real-time 60 FPS desktop panning/zooming.

---

## 4. Verification Check Before Sprint 01-01

- [x] All candidate third-party licenses (MIT, SIL OFL, Public Domain, ISC) are permissive and commercial-friendly.
- [x] Copyleft licenses (GPL, AGPL, CC-BY-SA, ODbL) are strictly excluded from engine binaries and content data.
- [x] Attribution notices drafted for the in-game "Credits / Legal Notices" screen.
