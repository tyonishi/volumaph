# CHANGELOG

## [Unreleased]


## [0.2.0] - 2026-01-05

Overview
- This is a minor release focused on performance, usability, and stability. It includes scanner and UI improvements, bug fixes, and developer tooling updates. The release is backward-compatible (no breaking API changes).

Highlights
- Faster scanning: Optimizations in the scanning engine reduce overall scan time and improve responsiveness for large filesystems.
- Improved visualization: Treemap rendering and UI interactions are smoother and more responsive, with better handling on high‑DPI displays.
- Reduced memory footprint: Memory usage during deep or large scans has been lowered, improving reliability on constrained systems.
- Robustness: Improved error handling for permission/IO edge cases and more informative logging.
- Developer tooling: Added/expanded unit tests, CI improvements, and documentation updates to aid contributors and maintainers.

Other improvements
- More consistent size aggregation and percentage calculations in the analyzer.
- Improved progress reporting and cancellation responsiveness during long-running scans.
- Minor UX polish: clearer labels, improved keyboard navigation, and small accessibility improvements.

Bug fixes
- Addressed several stability and correctness issues encountered during deep or concurrent scans.
- Fixed UI inconsistencies when switching views or resizing the window.
- Resolved a set of edge-case errors reported during permission-denied scenarios.

Upgrade notes
- Version bumped to `0.2.0`. This is a non-breaking minor release — existing user data and configurations remain compatible.
- To build from source: run `dotnet build` and `dotnet test` to verify.
- If you encounter stale cached scan data after upgrading, clearing the app cache or re-running a fresh scan is recommended.

Contributors
- Thanks to everyone who contributed to this release. (Add PR numbers and contributor usernames here as needed.)

---

## 2026-01-05 — Phase 4: Advanced Visualization — v0.4.0
Branch: `feature/phase4-visualization` (HEAD: `b5809a5`)

Short summary:
- Phase 4 (Advanced Visualization) has been implemented. New visualization features include Treemap and Sunburst views, size-based color mapping, interactive zoom/pan, zoom history and bookmarks.

Added features:
- Treemap visualization (`src/VoluMaph.UI/Controls/TreemapControl.xaml(.cs)` + `VoluMaph.Core/Visualization/Layouts/SquarifiedTreemapLayout.cs`)
- Sunburst chart (`src/VoluMaph.UI/Controls/SunburstControl.xaml(.cs)` + `VoluMaph.Core/Visualization/Layouts/PolarSunburstLayout.cs`) — drill-down and breadcrumb navigation
- Size-based color mapping and multiple palettes (`VoluMaph.Core/Visualization/Color/SizeBasedColorMapper.cs`, `ColorThemeDefinitions.cs`)
- Color legend control (`src/VoluMaph.UI/Controls/ColorLegendControl.xaml(.cs)`) and toolbar theme selector
- Interactive zoom/pan with `ZoomPanBehavior`, `VisualizationHistory`, and `VisualizationBookmark`
- Integration of `VisualizationViewModel` into `MainViewModel` and toolbar controls for visualization mode and theme
- Unit and integration tests for visualization components (Core and UI)

Important notes:
- Layout calculations are performed in Core to keep UI rendering responsive; however, rendering large datasets will still load the UI thread—benchmarking and profiling are recommended for large-scale use (100k+ nodes).
- Some UI tests depend on rendering behavior and may need CI stabilization (headless or platform-specific test runners).

---
