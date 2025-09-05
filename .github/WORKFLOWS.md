# GitHub Workflows Documentation

This repository contains three GitHub Actions workflows for building and testing the Gcodes project.

## Workflows Overview

### 1. Build and Package (`build.yml`)

**Triggers:**
- Push to `master` branch
- Release published

**Purpose:**
- Builds the solution in Release configuration
- Creates zip archives of each project's `bin/` directory:
  - `gcodes-lib-bin.zip` - Contains Gcodes library binaries
  - `gcodes-console-bin.zip` - Contains Gcodes.Console application binaries
  - `gcodes-test-bin.zip` - Contains Gcodes.Test binaries
- Uploads archives as GitHub Actions artifacts

**Artifacts:**
The workflow produces downloadable artifacts for each project's build output, making it easy to distribute compiled binaries.

### 2. PR Validation (`pr-validation.yml`)

**Triggers:**
- Pull requests to `master` branch

**Purpose:**
- Builds the solution in Debug configuration
- Runs all unit tests using VSTest
- Publishes test results with detailed reporting
- Fails the PR if tests fail

**Testing:**
Uses xUnit framework for testing with comprehensive test result reporting.

### 3. Manual Test Build (`manual-test.yml`)

**Triggers:**
- Manual dispatch only (can be triggered from GitHub Actions UI)

**Purpose:**
- Development and debugging workflow
- Builds solution in both Debug and Release configurations
- Lists all build outputs for inspection
- Runs tests with console logging
- Continues on test errors for debugging

## Technical Details

### Platform Requirements
All workflows run on `windows-latest` runners because:
- The project targets .NET Framework 4.7.2
- .NET Framework requires Windows environment for building
- Uses MSBuild and VSTest which are Windows-specific tools

### Dependencies
- **MSBuild**: For building .NET Framework projects
- **NuGet**: For package restoration
- **VSTest**: For running xUnit tests
- **PowerShell**: For archive creation and file operations

### Build Process
1. Checkout source code
2. Setup MSBuild and NuGet
3. Restore NuGet packages from `packages.config` files
4. Build solution using MSBuild
5. Create zip archives of `bin/` directories
6. Upload artifacts (build workflow only)
7. Run tests and publish results (validation workflows)

## Usage

### Running Workflows

**Build Workflow:**
- Automatically runs on push to master
- Manually triggered on releases

**PR Validation:**
- Automatically runs on all pull requests to master
- Must pass for PR to be mergeable

**Manual Test:**
- Go to Actions tab in GitHub
- Select "Manual Test Build" workflow
- Click "Run workflow" button

### Downloading Build Artifacts

1. Go to the Actions tab
2. Click on a completed "Build and Package" workflow run
3. Scroll down to the "Artifacts" section
4. Download the desired zip file(s)

## Migration from AppVeyor

This project previously used AppVeyor for CI/CD. The GitHub Actions workflows provide equivalent functionality with the following improvements:

- Better integration with GitHub pull requests
- More detailed test reporting
- Artifact management through GitHub
- Free for public repositories
- More flexible workflow configuration

The `appveyor.yml` file is retained for reference but is no longer the primary CI/CD system.