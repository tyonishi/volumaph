# Contributing to VoluMaph

Thank you for your interest in contributing to VoluMaph! This document provides guidelines and instructions for contributing to the project.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)

## 🤝 Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on what is best for the community
- Show empathy towards other community members

## 🚀 Getting Started

### Prerequisites

- **Windows 10/11** with latest updates
- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - [Download](https://git-scm.com/downloads)
- An IDE (Visual Studio 2022 or VS Code with C# extension)

### Setting Up Development Environment

1. **Fork the repository**

   ```bash
   # Fork the repo on GitHub first, then:
   git clone https://github.com/tyonishi/volumaph.git
   cd volumaph
   ```

2. **Add upstream remote**

   ```bash
   git remote add upstream https://github.com/original-owner/volumaph.git
   ```

3. **Build the solution**

   ```bash
   dotnet build
   ```

4. **Run tests**

   ```bash
   dotnet test
   ```

5. **Run the application**

   ```bash
   dotnet run --project src/VoluMaph.UI
   ```

## 🔄 Development Workflow

### 1. Create a Branch

Create a new branch for your work:

```bash
# Feature branch
git checkout -b feature/your-feature-name

# Bug fix branch
git checkout -b fix/your-bug-fix

# Documentation branch
git checkout -b docs/your-doc-update
```

Branch naming conventions:
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation changes
- `refactor/` - Code refactoring
- `test/` - Test additions or modifications
- `chore/` - Maintenance tasks

### 2. Make Changes

- Edit code following our [coding standards](#coding-standards)
- Add or update tests for your changes
- Update documentation if needed

### 3. Test Your Changes

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests for specific project
dotnet test tests/VoluMaph.Core.Tests
```

### 4. Commit Your Changes

Write clear, descriptive commit messages:

```bash
git add .
git commit -m "feat: add treemap visualization for disk usage"
```

Commit message format (Conventional Commits):
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, etc.)
- `refactor:` - Code refactoring
- `test:` - Test changes
- `chore:` - Maintenance tasks

Examples:
- `feat(scanner): add support for NTFS MFT scanning`
- `fix(ui): resolve crash when scanning empty directories`
- `docs(readme): update installation instructions for .NET 8`

### 5. Sync with Upstream

```bash
git fetch upstream
git rebase upstream/main
```

### 6. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a Pull Request on GitHub using the [PR template](.github/PULL_REQUEST_TEMPLATE.md).

## 📝 Coding Standards

We follow the coding standards defined in [AGENTS.md](AGENTS.md). Here's a summary:

### Naming Conventions

- **Classes**: `PascalCase` (e.g., `DirectoryScanner`, `FileNode`)
- **Methods**: `PascalCase` (e.g., `Scan`, `Analyze`)
- **Properties**: `PascalCase` (e.g., `FileSize`, `Percentage`)
- **Private fields**: `_camelCase` (e.g., `_rootFolder`, `_isScanning`)
- **Public/protected fields**: `PascalCase`
- **Local variables**: `camelCase` (e.g., `rootPath`, `fileSize`)
- **Constants**: `PascalCase` (e.g., `MaxFileSize`, `DefaultBufferSize`)
- **Interfaces**: `I` prefix + PascalCase (e.g., `IScanner`, `ILogger`)

### Code Style

- Use **4 spaces** for indentation
- Add **newline at end of file**
- Use **meaningful names** that clearly indicate intent
- **Avoid unnecessary comments** - let the code speak for itself
- Follow **async/await** best practices for all file operations

### Architecture Principles

- **Loose coupling** - Scanner engine and UI should be independent
- **CQRS** - Separate read (scanning) from write (UI updates)
- **Async first** - All file system operations must be asynchronous
- **MVVM pattern** - UI layer uses Model-View-ViewModel

### Example

```csharp
public sealed class DirectoryScanner : IScanner
{
    private readonly ILogger _logger;
    private readonly IProgress<ScanProgress>? _progress;

    public DirectoryScanner(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<FolderNode> ScanAsync(
        string rootPath,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var root = new FolderNode(rootPath);
            ScanDirectory(root, progress, cancellationToken);
            return root;
        }, cancellationToken);
    }

    private void ScanDirectory(
        FolderNode folder,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

## 🧪 Testing

### Test-Driven Development (TDD)

We follow TDD principles:

1. **Write tests first** based on expected behavior
2. **Run tests** to confirm they fail
3. **Implement code** to make tests pass
4. **Refactor** without changing tests
5. **Repeat** until all tests pass

### Test Organization

```
tests/
 └─ VoluMaph.Core.Tests/
     ├─ Analysis/
     │   ├── FolderAnalyzerTests.cs
     │   ├── ExtensionAnalyzerTests.cs
     │   └── DuplicateDetectorTests.cs
     ├─ Model/
     │   ├── FileNodeTests.cs
     │   └── FolderNodeTests.cs
     └─ Scanning/
         └── DirectoryScannerTests.cs
```

### Writing Tests

```csharp
public class FolderAnalyzerTests
{
    private readonly IFolderAnalyzer _analyzer;

    public FolderAnalyzerTests()
    {
        _analyzer = new FolderAnalyzer();
    }

    [Fact]
    public void AggregateFolderSizes_ShouldCalculateCorrectSize()
    {
        // Arrange
        var folder = CreateTestFolder();

        // Act
        _analyzer.AggregateFolderSizes(folder);

        // Assert
        Assert.Equal(1500, folder.Size);
        Assert.Equal(2, folder.FileCount);
    }
}
```

### Test Coverage

- Aim for **80%+ code coverage**
- All public methods should have tests
- Test both happy paths and edge cases
- Test error handling and exceptions

## 📤 Submitting Changes

### Pull Request Checklist

Before submitting a PR, ensure:

- [ ] Code follows our [coding standards](#coding-standards)
- [ ] All tests pass (`dotnet test`)
- [ ] New features include tests
- [ ] Documentation is updated (if needed)
- [ ] Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/)
- [ ] PR title follows the same format
- [ ] PR description is clear and includes:
  - Problem description
  - Solution approach
  - Testing done
  - Screenshots (for UI changes)

### Pull Request Review Process

1. **Automated checks** - CI/CD runs tests and linting
2. **Code review** - Maintainers review your changes
3. **Feedback** - Address review comments
4. **Approval** - PR approved and merged

### Review Guidelines

- Be open to feedback and suggestions
- Respond to comments in a timely manner
- Make requested changes or discuss alternatives
- Update PR description if scope changes

## 🐛 Reporting Bugs

Before reporting a bug, please:

1. **Search existing issues** to avoid duplicates
2. **Check if the bug is already fixed** in the latest version
3. **Reproduce the bug** to confirm it's consistent

### Bug Report Template

Use the [bug report template](.github/ISSUE_TEMPLATE/bug_report.md) which includes:

- **Title**: Clear, concise description of the bug
- **Description**: Detailed explanation of the issue
- **Steps to reproduce**: Step-by-step reproduction guide
- **Expected behavior**: What should happen
- **Actual behavior**: What actually happens
- **Environment**: OS version, .NET version, etc.
- **Screenshots**: Visual evidence if applicable
- **Additional context**: Any other relevant information

## 💡 Suggesting Features

We welcome feature suggestions! Before suggesting:

1. **Check existing issues** and roadmap
2. **Review project goals** - is this feature in scope?
3. **Consider complexity** - can it be implemented reasonably?

### Feature Request Template

Use the [feature request template](.github/ISSUE_TEMPLATE/feature_request.md) which includes:

- **Title**: Clear, concise feature description
- **Problem**: What problem does this solve?
- **Proposed solution**: How should it work?
- **Alternatives considered**: Other approaches evaluated
- **Additional context**: Mockups, examples, etc.

## 📚 Documentation

### Documentation Types

- **Code comments** - Only for complex logic (minimal)
- **XML documentation** - Public APIs
- **README** - User-facing documentation
- **docs/** - Developer documentation (design, architecture)
- **AGENTS.md** - Development guidelines

### Writing Documentation

- Keep it **simple and clear**
- Use **examples** where helpful
- **Update** docs when code changes
- Include **Japanese** content where appropriate

## 🏆 Recognition

Contributors will be recognized in:

- **README.md** - Contributors section
- **CHANGELOG.md** - Release notes
- **About dialog** - In the application

## ❓ Getting Help

- **GitHub Discussions** - For questions and ideas
- **GitHub Issues** - For bugs and feature requests
- **Documentation** - Check [docs/](docs/) folder
- **AGENTS.md** - Development rules and guidelines

## 📧 Contact

For questions not covered here, please:

- Open a **GitHub Discussion**
- Email: [support@capricornus.biz](mailto:support@capricornus.biz)

---

Thank you for contributing to VoluMaph! 🎉
