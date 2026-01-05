# MVP Next Tasks (after Phase 4)

Updated: 2026-01-05

High-priority remaining items:

1. Documentation: create a short user guide for visualization features (`docs/user-guides/visualization.md`).
2. CI: integrate visualization tests into CI and stabilize UI/XAML tests for headless CI execution.
3. Performance: run profiling and benchmarks on large datasets (100k / 1M nodes) to identify rendering and memory hotspots.

Medium / Low priority:

- Accessibility: verify screen reader support and keyboard-only workflows for visualization controls.
- Plugin samples: provide a sample using `IColorThemeExtension` to demonstrate plugging in custom palettes.
- Release: prepare Windows installer and finalize release notes.

Notes:
- Start by adding visualization tests to CI to surface environment-specific failures.
- Draft the user guide as a short hands-on tutorial (tasks + shortcuts + known limits) before expanding into a detailed manual.
