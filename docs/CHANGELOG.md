# CHANGELOG

## [Unreleased]

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
