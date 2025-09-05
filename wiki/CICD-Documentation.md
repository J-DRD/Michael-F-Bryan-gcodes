# CI/CD Documentation

This document provides comprehensive information about the Continuous Integration and Continuous Deployment (CI/CD) setup for the Gcodes project using GitHub Actions.

## Table of Contents

1. [Overview](#overview)
2. [Workflow Files](#workflow-files)
3. [Build Workflow](#build-workflow)
4. [PR Validation Workflow](#pr-validation-workflow)
5. [Manual Test Workflow](#manual-test-workflow)
6. [Artifacts and Releases](#artifacts-and-releases)
7. [Configuration Details](#configuration-details)
8. [Troubleshooting CI/CD](#troubleshooting-cicd)
9. [Adding New Workflows](#adding-new-workflows)
10. [Migration from AppVeyor](#migration-from-appveyor)

## Overview

The Gcodes project uses GitHub Actions for all CI/CD operations. The system provides:

- **Automated Building**: Builds on every push to master and releases
- **Pull Request Validation**: Ensures PRs don't break the build or tests
- **Artifact Generation**: Creates downloadable build outputs
- **Test Execution**: Runs the full test suite with detailed reporting
- **Manual Testing**: On-demand workflow for debugging

### Workflow Triggers

| Workflow | Triggers |
|----------|----------|
| Build and Package | Push to `master`, Published releases |
| PR Validation | Pull requests to `master` |
| Manual Test | Manual dispatch only |

## Workflow Files

All workflow files are located in `.github/workflows/`:

```
.github/
├── workflows/
│   ├── build.yml           # Main build and package workflow
│   ├── pr-validation.yml   # Pull request validation
│   └── manual-test.yml     # Manual testing workflow
└── WORKFLOWS.md           # Detailed workflow documentation
```

## Build Workflow

**File**: `.github/workflows/build.yml`

### Purpose
- Builds the solution in Release configuration
- Creates zip archives of build outputs
- Uploads artifacts for download

### Trigger Conditions
```yaml
on:
  push:
    branches: [master]
  release:
    types: [published]
```

### Build Process
1. **Checkout**: Gets the latest source code
2. **Setup**: Configures MSBuild and NuGet
3. **Restore**: Downloads NuGet packages
4. **Build**: Compiles in Release mode
5. **Package**: Creates zip archives
6. **Upload**: Stores artifacts in GitHub

### Artifacts Created
| Artifact | Contains |
|----------|----------|
| `gcodes-lib-bin.zip` | Gcodes library binaries |
| `gcodes-console-bin.zip` | Console application binaries |
| `gcodes-test-bin.zip` | Test binaries |

### Usage Example
```yaml
name: Build and Package

on:
  push:
    branches: [master]
  release:
    types: [published]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
      
    - name: Setup NuGet
      uses: nuget/setup-nuget@v1
      
    - name: Restore packages
      run: nuget restore Gcode.sln
      
    - name: Build solution
      run: msbuild Gcode.sln /p:Configuration=Release
      
    - name: Create artifacts
      run: |
        Compress-Archive -Path "Gcodes/bin/Release/*" -DestinationPath "gcodes-lib-bin.zip"
        Compress-Archive -Path "Gcodes.Console/bin/Release/*" -DestinationPath "gcodes-console-bin.zip"
        Compress-Archive -Path "Gcodes.Test/bin/Release/*" -DestinationPath "gcodes-test-bin.zip"
      
    - name: Upload artifacts
      uses: actions/upload-artifact@v3
      with:
        name: build-artifacts
        path: "*.zip"
```

## PR Validation Workflow

**File**: `.github/workflows/pr-validation.yml`

### Purpose
- Validates pull requests before merging
- Ensures code builds successfully
- Runs all unit tests
- Reports test results

### Trigger Conditions
```yaml
on:
  pull_request:
    branches: [master]
```

### Validation Process
1. **Checkout**: Gets PR source code
2. **Setup**: Configures build environment
3. **Restore**: Downloads dependencies
4. **Build**: Compiles in Debug mode
5. **Test**: Runs xUnit tests
6. **Report**: Publishes test results

### Test Reporting
The workflow publishes detailed test results:
- **Test Summary**: Pass/fail counts
- **Failed Test Details**: Specific failures
- **Test Duration**: Performance metrics
- **Coverage Information**: If available

### Example Test Output
```
Test Results Summary:
✅ Passed: 45
❌ Failed: 2
⏸️ Skipped: 1
Total: 48 tests

Failed Tests:
- ParserTest.ParseComplexGcode: Expected 5 but was 4
- LexerTest.HandleSpecialCharacters: NullReferenceException
```

## Manual Test Workflow

**File**: `.github/workflows/manual-test.yml`

### Purpose
- On-demand testing and debugging
- Tests both Debug and Release configurations
- Continues on errors for diagnostic purposes
- Provides detailed output for troubleshooting

### Trigger
Manual dispatch only via GitHub Actions UI:
1. Go to Actions tab
2. Select "Manual Test Build"
3. Click "Run workflow"

### Features
- **Dual Configuration**: Builds both Debug and Release
- **Error Continuation**: Doesn't stop on test failures
- **Verbose Output**: Detailed logging for debugging
- **File Listing**: Shows build outputs

### Running Manual Tests
```bash
# Via GitHub UI: Actions → Manual Test Build → Run workflow

# Or via GitHub CLI
gh workflow run manual-test.yml
```

## Artifacts and Releases

### Downloading Artifacts
1. **Navigate**: Go to Actions tab
2. **Select**: Click on a completed workflow run
3. **Download**: Scroll to Artifacts section
4. **Extract**: Unzip downloaded files

### Artifact Contents
```
gcodes-lib-bin.zip:
├── Gcodes.dll
├── Gcodes.pdb
└── Gcodes.xml

gcodes-console-bin.zip:
├── Gcodes.Console.exe
├── Gcodes.Console.exe.config
├── Gcodes.dll
├── CommandLine.dll
├── Serilog.dll
└── Serilog.Sinks.Console.dll

gcodes-test-bin.zip:
├── Gcodes.Test.dll
├── Gcodes.dll
├── xunit.core.dll
└── (other test dependencies)
```

### Release Process
When a release is published:
1. **Build Workflow**: Automatically triggered
2. **Artifacts**: Generated and attached to release
3. **Documentation**: Should be updated manually

## Configuration Details

### Platform Requirements
All workflows run on `windows-latest` because:
- **.NET Framework 4.7.2**: Windows-only framework
- **MSBuild**: Requires Windows environment
- **VSTest**: Windows-specific testing tools

### Environment Setup
```yaml
- name: Setup MSBuild
  uses: microsoft/setup-msbuild@v1

- name: Setup NuGet
  uses: nuget/setup-nuget@v1

- name: Setup VSTest
  uses: darenm/Setup-VSTest@v1
```

### Dependencies
The workflows use these actions:
- `actions/checkout@v4`: Source code checkout
- `microsoft/setup-msbuild@v1`: MSBuild configuration
- `nuget/setup-nuget@v1`: NuGet setup
- `darenm/Setup-VSTest@v1`: Test runner setup
- `actions/upload-artifact@v3`: Artifact storage

### Build Commands
```bash
# Package restoration
nuget restore Gcode.sln

# Debug build
msbuild Gcode.sln /p:Configuration=Debug

# Release build
msbuild Gcode.sln /p:Configuration=Release

# Run tests
vstest.console.exe "Gcodes.Test\bin\Debug\Gcodes.Test.dll" --logger:trx
```

## Troubleshooting CI/CD

### Common Build Failures

#### NuGet Restore Failed
**Error**: Package restore failed
**Solution**:
```yaml
- name: Clear NuGet cache
  run: nuget locals all -clear

- name: Restore with verbose output
  run: nuget restore Gcode.sln -Verbosity detailed
```

#### MSBuild Errors
**Error**: MSBuild not found or build failed
**Diagnostics**:
```yaml
- name: Check MSBuild version
  run: msbuild -version

- name: Build with detailed output
  run: msbuild Gcode.sln /p:Configuration=Release /verbosity:diagnostic
```

#### Test Failures
**Error**: Tests fail in CI but pass locally
**Investigation**:
```yaml
- name: Run tests with verbose output
  run: |
    vstest.console.exe "Gcodes.Test\bin\Debug\Gcodes.Test.dll" `
    --logger:console;verbosity=detailed `
    --logger:trx;LogFileName=test-results.trx
```

### Workflow Debugging

#### Enable Debug Logging
Add to workflow environment:
```yaml
env:
  ACTIONS_STEP_DEBUG: true
  ACTIONS_RUNNER_DEBUG: true
```

#### Check Workflow Status
```bash
# Using GitHub CLI
gh run list --workflow=build.yml
gh run view <run-id>
gh run download <run-id>
```

#### Local Workflow Testing
Use `act` to test workflows locally:
```bash
# Install act
choco install act-cli

# Run workflow locally
act push -W .github/workflows/build.yml
```

## Adding New Workflows

### Workflow Template
```yaml
name: New Workflow

on:
  push:
    branches: [master]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
      
    - name: Your custom step
      run: echo "Custom action"
```

### Best Practices
1. **Use Specific Versions**: Pin action versions for stability
2. **Cache Dependencies**: Cache NuGet packages when possible
3. **Fail Fast**: Stop on first error unless debugging
4. **Secure Secrets**: Use GitHub secrets for sensitive data
5. **Descriptive Names**: Use clear step and job names

### Adding Package Publishing
```yaml
- name: Pack NuGet package
  run: nuget pack Gcodes\Gcodes.csproj -Properties Configuration=Release

- name: Publish to NuGet
  run: nuget push *.nupkg -ApiKey ${{ secrets.NUGET_API_KEY }} -Source https://api.nuget.org/v3/index.json
```

### Adding Code Quality Checks
```yaml
- name: Run static analysis
  run: |
    msbuild Gcode.sln /p:RunCodeAnalysis=true /p:TreatWarningsAsErrors=true

- name: Check code formatting
  run: dotnet format --verify-no-changes
```

## Migration from AppVeyor

The project previously used AppVeyor for CI/CD. The migration to GitHub Actions provides:

### Improvements
- ✅ **Better GitHub Integration**: Native PR status checks
- ✅ **Free for Public Repos**: No cost for open source projects
- ✅ **More Flexible**: Easier workflow customization
- ✅ **Better Artifact Management**: Integrated with GitHub releases
- ✅ **Matrix Builds**: Easy to test multiple configurations

### Legacy AppVeyor Configuration
The `appveyor.yml` file is retained for reference:
```yaml
version: 1.0.{build}
image: Visual Studio 2019
configuration: Release
platform: Any CPU

before_build:
  - nuget restore

build:
  project: Gcode.sln
  verbosity: minimal

test_script:
  - vstest.console.exe "Gcodes.Test\bin\Release\Gcodes.Test.dll"

artifacts:
  - path: '**\bin\Release\**\*'
    name: Binaries
```

### Migration Benefits
| Feature | AppVeyor | GitHub Actions |
|---------|----------|----------------|
| Cost | Limited free builds | Unlimited for public repos |
| Integration | External service | Native GitHub |
| Configuration | Single YAML file | Multiple specialized workflows |
| Artifacts | External storage | GitHub-integrated |
| Matrix builds | Limited | Extensive support |

## Monitoring and Notifications

### Build Status Badges
Add to README.md:
```markdown
[![GitHub Actions](https://github.com/J-DRD/Michael-F-Bryan-gcodes/workflows/Build%20and%20Package/badge.svg)](https://github.com/J-DRD/Michael-F-Bryan-gcodes/actions)
```

### Notification Settings
Configure in repository settings:
- **Email**: Notifications for failed builds
- **Slack/Teams**: Integration for team notifications
- **GitHub Mobile**: Push notifications

### Workflow Insights
Access via Insights → Actions:
- **Success Rate**: Build success percentage
- **Duration Trends**: Build time over time
- **Failure Analysis**: Common failure patterns

This CI/CD setup ensures reliable, automated building and testing while providing flexibility for future enhancements and debugging capabilities.