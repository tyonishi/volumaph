# 📊 VoluMaph

**Fast disk usage visualization for Windows**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/tyonishi/volumaph/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

## 📖 About

VoluMaph (Volume + Map) is a lightweight, high-performance disk usage visualization tool for Windows 10/11. It scans your disk and visualizes folder structures to help you identify space-hungry files and directories quickly.

Inspired by tools like WizTree, VoluMaph focuses on:
- **Fast scanning** - Optimized for performance with large directories
- **Simple UI** - Clean, intuitive interface
- **Modern design** - Windows 11 Fluent Design inspired
- **No dependencies** - Built with .NET SDK only

## ✨ Features

### Current Features (Implemented)

#### Core Scanning & Analysis
- ✅ **Disk Scanning** - Scan any drive or folder with async operations
- ✅ **Real-time Progress** - Live scan progress updates with cancellation support
- ✅ **Size Aggregation** - Automatic recursive size calculation for folders
- ✅ **File/Folder Counting** - Accurate file and folder statistics
- ✅ **Duplicate Detection** - Find duplicates by size and by SHA-256 hash
- ✅ **Extension Analysis** - Analyze file type distribution with statistics

#### Filtering & Sorting
- ✅ **Advanced Filtering** - Filter by size range, extension, and file age
- ✅ **Date Filtering** - Filter by creation/modification date
- ✅ **Search** - Real-time case-insensitive search by name
- ✅ **Multiple Sorting Options** - Sort by size, name, or file count (ascending/descending)
- ✅ **Extension Filter** - Filter by specific file extensions

#### UI/UX
- ✅ **Tree View** - Visual folder hierarchy with icons and size display
- ✅ **Data Grid** - Sortable columns with context menu
- ✅ **Drag & Drop** - Scan folders by dragging them onto the window
- ✅ **Theme Support** - Dark/Light/High Contrast themes
- ✅ **Fluent Design** - Modern Windows 11 inspired UI with animations
- ✅ **Summary Cards** - Quick overview of total size, files, folders, largest file
- ✅ **Expandable Panels** - Collapsible summary and filter panels
- ✅ **Toast Notifications** - Non-intrusive status messages
- ✅ **Context Menus** - Right-click options for file operations
- ✅ **Keyboard Shortcuts** - F5 (refresh), Escape (cancel), Ctrl+E (CSV), Ctrl+T (theme)

#### Visualization (Phase 4)
- ✅ **Treemap visualization** - Rectangular partitioning (TreemapControl + SquarifiedTreemapLayout)
- ✅ **Sunburst chart** - Polar chart with drill-down and breadcrumb
- ✅ **Size-based color coding** - Multiple themes and gradients
- ✅ **Interactive zoom & pan** - Smooth zooming, history and bookmarks

#### Export & Operations
- ✅ **CSV Export** - UTF-8 with BOM for Excel compatibility
- ✅ **HTML Export** - Styled reports with CSS and responsive design
- ✅ **Open in Explorer** - Navigate to selected file/folder
- ✅ **Copy Path/Name** - Quick copy to clipboard
- ✅ **Screenshot** - Capture current view

## 📸 Screenshots

> *Screenshots coming soon!*

## 🚀 Getting Started

### Prerequisites

- **Windows 10** (version 1809 or later) or **Windows 11**
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (optional) or VS Code

### Installation

#### From Source

```bash
# Clone the repository
git clone https://github.com/tyonishi/volumaph.git
cd volumaph

# Build the solution
dotnet build

# Run the application
dotnet run --project src/VoluMaph.UI
```

#### Release Build

```bash
# Create a self-contained executable
dotnet publish src/VoluMaph.UI -c Release -r win-x64 --self-contained true
```

The executable will be in `src/VoluMaph.UI/bin/Release/net8.0-windows/win-x64/publish/`

## 📖 Usage

### Basic Usage

1. **Select Drive** - Choose a drive from the dropdown
2. **Click Scan** - Start scanning the selected drive
3. **Explore** - Navigate the folder tree in the left panel
4. **View Details** - See detailed file information in the right panel
5. **Filter & Sort** - Use the toolbar to filter and sort results

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+R` | Refresh drive list |
| `F5` | Re-scan current drive |
| `Ctrl+E` | Export to CSV |
| `Ctrl+H` | Export to HTML |

### Advanced Features

#### Filtering
Filter files by:
- **Size range** - Min/Max size in bytes
- **File extensions** - `.txt`, `.png`, etc.
- **Date modified** - Files modified after/before X days
- **Date created** - Files created after/before X days

#### Sorting
Sort folders and files by:
- Size (ascending/descending)
- Name (ascending/descending)
- File count

#### Export
Export scan results to:
- **CSV** - For spreadsheet analysis
- **HTML** - For formatted reports with styling

## 🔧 Development

### Project Structure

```
volumaph/
 ├─ src/
 │   ├─ VoluMaph.Core/          # Core logic (scanning, models, analysis)
 │   │   ├─ Analysis/            # Size aggregation, filtering, sorting
 │   │   ├─ Model/               # File system node models
 │   │   └─ Scanning/            # Scanner implementations
 │   ├─ VoluMaph.Infrastructure/ # Settings, logging
 │   │   ├─ Logging/
 │   │   └─ Settings/
 │   └─ VoluMaph.UI/            # WPF UI + MVVM
 │       ├─ Commands/           # Command implementations
 │       ├─ Themes/             # Dark/Light theme resources
 │       ├─ ValueConverters/    # Data binding converters
 │       └─ ViewModels/         # MVVM ViewModels
 ├─ tests/                      # Unit tests
 ├─ docs/                       # Documentation
 └── AGENTS.md                  # Agent development rules
```

### Building

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/VoluMaph.Core

# Clean build
dotnet clean && dotnet build
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test tests/VoluMaph.Core.Tests
dotnet test tests/VoluMaph.UI.Tests

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

**Test Coverage:**
- **VoluMaph.Core**: 34 tests covering models, scanning, and analysis
- **VoluMaph.UI**: 35 tests covering ViewModels and exports
- **Total**: 69 tests

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Quick Start for Contributors

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass (`dotnet test`)
6. Commit with clear messages
7. Push to your fork
8. Open a Pull Request

## 🐛 Reporting Issues

Found a bug? Please [open an issue](https://github.com/tyonishi/volumaph/issues/new?template=bug_report.md) with:
- Clear description of the problem
- Steps to reproduce
- Expected vs actual behavior
- Screenshots if applicable
- Your environment (OS, .NET version)

## 💡 Feature Requests

Have an idea? Please [open an issue](https://github.com/tyonishi/volumaph/issues/new?template=feature_request.md) with:
- Clear description of the feature
- Use case and benefits
- Possible implementation approach (if known)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by [WizTree](https://antibody-software.com/wiztree/) for the concept of fast disk scanning
- Built with [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) and [.NET 8](https://dotnet.microsoft.com/)
- MVVM pattern implementation
- Icons from [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)

---

Made with ❤️ by the VoluMaph team
