# ADR-003: Pinned Toolchain, Solution Structure, and Automated CI Pipeline

- **Status:** Accepted
- **Date:** Phase 01, Sprint 01-01
- **Deciders:** DevOps / Release Engineer, Lead Game Architect, Sprint Coordinator

---

## Context

To ensure reproducible builds, deterministic test outcomes, and seamless team collaboration across Windows developer workstations and CI runners, the project requires an authoritative pinned toolchain, a multi-project solution structure, and an automated continuous integration pipeline.

---

## Decisions

1. **Pinned Runtime & Language Baseline:**
   - **.NET SDK:** 8.0 (LTS, specifically 8.0.x with C# 12).
   - **Godot Candidate Engine:** Godot 4.3 .NET (or Godot 4.2+ .NET LTS).
   - **Nullable Reference Types:** Enabled with warnings treated as errors in Release configurations.

2. **Multi-Project Solution Structure (`src/WorldState.sln`):**
   - `src/WorldState.Domain/`: Pure .NET 8 class library. Contains all authoritative state, simulation models, command validation pipelines, and business rules. Completely isolated from Godot scene nodes or UI engines.
   - `src/WorldState.Domain.Tests/`: xUnit test project. Fast, headless, deterministic unit tests, invariant checks, and save/load round-trip validation.
   - `src/WorldState.Desktop/`: Godot .NET client host (Phase 01 UI shell and map presentation).
   - `tools/ContentValidator/`: Headless .NET CLI utility that validates all game content JSON definitions in `content/` against strict schema rules.

3. **Continuous Integration Pipeline:**
   - GitHub Actions workflow (`.github/workflows/ci.yml`) matrix-testing on `windows-latest` and `ubuntu-latest`.
   - Pipeline enforces: `dotnet restore` -> `dotnet build -c Release /p:TreatWarningsAsErrors=true` -> `dotnet test` -> `ContentValidator`.
   - Any compiler warning or test regression blocks merging to `main`.

4. **Reproducible Local Verification Script:**
   - Single-command local runner: `.\build.ps1` (PowerShell).

---

## Consequences

### Positive
- Strict architectural boundaries guaranteed: Domain rules can be tested in milliseconds without running a graphical engine.
- Zero drift between Windows local developer builds and remote CI execution.
- Content errors (malformed JSONs, invalid country stats) are caught at build time before runtime crashes occur.
- Fast iteration loop (full test suite runs in under 2 seconds).
