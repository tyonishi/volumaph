# Visualization Quick Guide

This short guide explains how to use the new visualization features in VoluMaph (Treemap and Sunburst), how to change color themes, and how to navigate large datasets using zoom and bookmarks.

Last updated: 2026-01-05

## 1. Overview

VoluMaph provides two visual modes for exploring disk usage:

- Treemap: rectangular partitioning that represents files/folders as area-proportional rectangles.
- Sunburst: radial view that represents folder depth as concentric rings and segment arcs.

Both views support:
- Size-based color mapping with selectable palettes
- Mouse-driven zoom and pan
- Click-to-drill-down and breadcrumb navigation
- Tooltips and context actions (open in Explorer, properties)

## 2. Switching modes

- Use the visualization mode toggle in the main toolbar to switch between Treemap and Sunburst.
- The currently selected node in the folder tree is used as the root for the visualization.

## 3. Color themes

- Open the color theme selector in the toolbar to switch palettes (Cool, Warm, Forest, Ocean, Sunset).
- The Color Legend control shows the mapping from size percentage to color.
- Automatic theme selection: Visualization will select an appropriate theme based on folder depth heuristics (can be overridden by the toolbar selector).

## 4. Zoom & Pan

- Mouse wheel: zoom in/out centered on cursor.
- Click + drag (left-button): pan the viewport.
- Keyboard shortcuts:
  - `Home` — Reset zoom to fit
  - `End` — Zoom to selected node
  - `Esc` — Exit interaction / reset
- Zoom history:
  - Use the Go Back / Go Forward commands in the visualization toolbar to step through previous zoom states.

## 5. Drill-down and breadcrumbs

- Click a rectangle (Treemap) or segment (Sunburst) to drill down into that folder. The visualization will center and zoom to the selected node.
- Breadcrumbs appear at the top of the visualization to indicate the current path and allow quick navigation back up.

## 6. Tooltips and context menu

- Hover a node to see a tooltip with: name, human‑readable size, percentage, and full path.
- Right-click a node to open the context menu with actions including: Open in Explorer, Copy Path, Show Properties.

## 7. Bookmarks

- You can save the current zoom/pan state as a bookmark. Bookmarks persist in application settings and can be returned to from the toolbar.

## 8. Performance tips and known limitations

- For very large trees (100k+ nodes), layout computation is performed in Core, but rendering can still be costly. If you experience slow UI on very large datasets, consider:
  - Narrowing the visualization root to a subfolder
  - Using filters to reduce visible nodes
  - Running profiling and reporting performance issues to the team

- Accessibility: basic keyboard shortcuts are supported; screen-reader support is being iterated and improved.

## 9. Troubleshooting

- If colors or layout appear incorrect, try toggling themes or reloading the view.
- If the app becomes unresponsive during an extremely large layout, cancel the operation and run a scan with a narrower root.

## 10. Feedback

Please open issues or PRs for bugs, usability improvements, or feature requests related to visualization. Include sample data or reproduction steps where possible.
