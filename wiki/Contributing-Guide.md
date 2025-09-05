# Contributing Guide

Thank you for your interest in contributing to the Gcodes project! This guide will help you get started with contributing code, documentation, and other improvements.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Development Environment Setup](#development-environment-setup)
3. [Code Style and Standards](#code-style-and-standards)
4. [Testing Guidelines](#testing-guidelines)
5. [Submitting Changes](#submitting-changes)
6. [Issue Guidelines](#issue-guidelines)
7. [Pull Request Process](#pull-request-process)
8. [Documentation Contributions](#documentation-contributions)
9. [Performance Considerations](#performance-considerations)
10. [Release Process](#release-process)

## Getting Started

### Prerequisites
- **.NET Framework 4.7.2** or later
- **Visual Studio 2017** or later (recommended) or **Visual Studio Code**
- **Git** for version control
- **Windows OS** (required for .NET Framework builds)

### Fork and Clone
1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/Michael-F-Bryan-gcodes.git
   cd Michael-F-Bryan-gcodes
   ```
3. Add the upstream remote:
   ```bash
   git remote add upstream https://github.com/J-DRD/Michael-F-Bryan-gcodes.git
   ```

## Development Environment Setup

### Building the Project
1. Open `Gcode.sln` in Visual Studio
2. Restore NuGet packages:
   ```bash
   nuget restore
   ```
3. Build the solution:
   ```bash
   msbuild Gcode.sln /p:Configuration=Debug
   ```

### Running Tests
```bash
# Run all tests
dotnet test Gcodes.Test/Gcodes.Test.csproj

# Or use Visual Studio Test Explorer
# Test → Run All Tests
```

### Project Structure
```
Gcodes/                    # Main library
├── Ast/                   # Abstract Syntax Tree classes
├── Tokens/               # Tokenization classes
├── Runtime/              # Execution engine
├── Lexer.cs             # Tokenizer
├── Parser.cs            # Parser
└── Interpreter.cs       # Base interpreter

Gcodes.Console/           # Command-line tool
├── Program.cs           # Main entry point
└── LoggingSimulator.cs  # Logging implementation

Gcodes.Test/             # Unit tests
├── LexerTest.cs        # Lexer tests
├── ParserTest.cs       # Parser tests
├── Runtime/            # Simulation tests
└── Fixtures/           # Test G-code files
```

## Code Style and Standards

### C# Coding Standards
We follow standard C# conventions with some specific guidelines:

#### Naming Conventions
```csharp
// Classes: PascalCase
public class GcodeParser { }

// Methods: PascalCase
public void ParseGcodeFile() { }

// Properties: PascalCase
public int LineNumber { get; set; }

// Private fields: camelCase
private List<Token> tokens;

// Constants: PascalCase
public const int MaxLineLength = 256;

// Enums: PascalCase
public enum TokenKind { G, M, X, Y }
```

#### Documentation
All public APIs must have XML documentation:

```csharp
/// <summary>
/// Parses a G-code string and returns a list of parsed commands.
/// </summary>
/// <param name="gcode">The G-code string to parse</param>
/// <returns>A list of parsed Code objects</returns>
/// <exception cref="UnrecognisedCharacterException">
/// Thrown when invalid characters are encountered
/// </exception>
public List<Code> Parse(string gcode)
{
    // Implementation
}
```

#### Error Handling
- Use specific exception types for different error conditions
- Include position information in parsing exceptions
- Provide meaningful error messages

```csharp
// Good
throw new UnrecognisedCharacterException(line, column, character);

// Bad
throw new Exception("Parse error");
```

#### LINQ Usage
Prefer LINQ for collection operations but be mindful of performance:

```csharp
// Good for readability
var gcodes = codes.OfType<Gcode>()
    .Where(g => g.Number == 1)
    .ToList();

// Consider performance for large collections
var gcodes = new List<Gcode>();
foreach (var code in codes)
{
    if (code is Gcode g && g.Number == 1)
        gcodes.Add(g);
}
```

### Design Patterns
The project uses several design patterns consistently:

#### Visitor Pattern
For AST traversal:
```csharp
public class MyInterpreter : Interpreter
{
    public override void Visit(Gcode code)
    {
        // Handle G-codes
    }
    
    public override void Visit(Mcode code)
    {
        // Handle M-codes
    }
}
```

#### Factory Pattern
For operation creation:
```csharp
public class CustomOperationFactory : OperationFactory
{
    public override IOperation CreateOperation(Code code, MachineState state)
    {
        // Custom logic
        return base.CreateOperation(code, state);
    }
}
```

## Testing Guidelines

### Test Organization
- One test class per production class
- Use descriptive test method names
- Group related tests with nested classes

```csharp
public class ParserTest
{
    public class GcodeTests
    {
        [Fact]
        public void ParsesBasicGcode()
        {
            // Test implementation
        }
    }
    
    public class McodeTests
    {
        [Fact]
        public void ParsesSpindleCommands()
        {
            // Test implementation
        }
    }
}
```

### Test Naming
Use descriptive names that explain the scenario:

```csharp
// Good
[Fact]
public void ParseGcode_WithXYCoordinates_ReturnsCorrectArguments()

// Bad
[Fact]
public void TestGcode()
```

### Test Data
Use inline data for simple cases:
```csharp
[Theory]
[InlineData("G01", 1)]
[InlineData("G00", 0)]
[InlineData("G28", 28)]
public void ParseGcode_VariousNumbers_ReturnsCorrectNumber(string input, int expected)
{
    var parser = new Parser(input);
    var gcode = parser.ParseGCode();
    Assert.Equal(expected, gcode.Number);
}
```

Use fixtures for complex test data:
```csharp
public class ComplexGcodeTest : IClassFixture<GcodeFixture>
{
    private readonly GcodeFixture fixture;
    
    public ComplexGcodeTest(GcodeFixture fixture)
    {
        this.fixture = fixture;
    }
}
```

### Testing Best Practices
1. **Arrange, Act, Assert**: Structure tests clearly
2. **One assertion per test**: Focus on specific behaviors
3. **Test edge cases**: Empty inputs, null values, boundary conditions
4. **Use meaningful assertions**: Prefer specific assertions over general ones

```csharp
[Fact]
public void Lexer_WithInvalidCharacter_ThrowsSpecificException()
{
    // Arrange
    var lexer = new Lexer("G01 X10$");
    
    // Act & Assert
    var ex = Assert.Throws<UnrecognisedCharacterException>(() => 
        lexer.Tokenize().ToList());
    
    Assert.Equal('$', ex.Character);
    Assert.Equal(1, ex.Line);
    Assert.Equal(8, ex.Column);
}
```

## Submitting Changes

### Branch Naming
Use descriptive branch names:
- `feature/add-arc-interpolation`
- `bugfix/fix-parser-null-reference`
- `docs/update-api-reference`
- `refactor/improve-lexer-performance`

### Commit Messages
Follow conventional commit format:
```
type(scope): brief description

Longer description if needed.

- Bullet points for multiple changes
- Reference issues: Fixes #123
```

Examples:
```
feat(parser): add support for G02/G03 arc commands

- Implement circular interpolation parsing
- Add I, J, K parameter support
- Update tests for arc functionality

Fixes #45
```

```
fix(lexer): handle negative numbers correctly

The lexer was incorrectly tokenizing negative numbers
as separate minus tokens.

Fixes #67
```

### Keeping Your Fork Updated
```bash
# Fetch upstream changes
git fetch upstream

# Update your main branch
git checkout master
git merge upstream/master

# Rebase your feature branch
git checkout feature/your-feature
git rebase master
```

## Issue Guidelines

### Reporting Bugs
Include the following information:
1. **Clear description** of the problem
2. **Steps to reproduce** the issue
3. **Expected behavior** vs **actual behavior**
4. **Code sample** that demonstrates the issue
5. **Environment details** (OS, .NET version, etc.)

**Bug Report Template:**
```markdown
## Bug Description
Brief description of the issue.

## Steps to Reproduce
1. Create a Parser with input "G01 X10"
2. Call Parse()
3. Exception is thrown

## Expected Behavior
Should parse successfully and return a Gcode object.

## Actual Behavior
UnexpectedTokenException is thrown.

## Code Sample
```csharp
var parser = new Parser("G01 X10");
var codes = parser.Parse().ToList(); // Throws here
```

## Environment
- OS: Windows 10
- .NET Framework: 4.7.2
- Visual Studio: 2019
```

### Feature Requests
For new features, please:
1. **Describe the use case** and motivation
2. **Provide examples** of how it would be used
3. **Consider alternatives** and explain why this approach is best
4. **Discuss implementation** if you have ideas

### Enhancement Proposals
For significant changes:
1. **Open an issue first** to discuss the approach
2. **Wait for maintainer feedback** before implementing
3. **Consider backward compatibility**
4. **Plan for documentation updates**

## Pull Request Process

### Before Submitting
1. **Ensure all tests pass**:
   ```bash
   msbuild Gcode.sln /p:Configuration=Release
   dotnet test Gcodes.Test/
   ```

2. **Add tests** for new functionality
3. **Update documentation** if needed
4. **Follow code style guidelines**
5. **Rebase on latest master**

### Pull Request Template
```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Added unit tests for new functionality
- [ ] All existing tests pass
- [ ] Manual testing completed

## Checklist
- [ ] My code follows the project's style guidelines
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
```

### Review Process
1. **Automated checks** must pass (CI/CD)
2. **Code review** by maintainers
3. **Address feedback** promptly
4. **Squash commits** if requested
5. **Maintainer merge** after approval

## Documentation Contributions

### Wiki Updates
The project wiki includes:
- **Home**: Project overview and navigation
- **Getting Started**: Installation and basic usage
- **API Reference**: Complete API documentation
- **Architecture Overview**: Design and implementation details
- **Examples and Tutorials**: Code examples and walkthroughs
- **G-code Reference**: Supported G-code syntax
- **Contributing Guide**: This document
- **Troubleshooting**: Common issues and solutions

### Documentation Style
- Use **clear, concise language**
- Include **code examples** for concepts
- Provide **step-by-step instructions**
- Keep **up to date** with code changes
- Use **consistent formatting**

### API Documentation
All public APIs require XML documentation:
```csharp
/// <summary>
/// Gets the value for a particular <see cref="ArgumentKind"/>, if the
/// argument was specified in this gcode.
/// </summary>
/// <param name="kind">The argument type to search for</param>
/// <returns>The argument value, or null if not found</returns>
public double? ValueFor(ArgumentKind kind)
```

## Performance Considerations

### Parser Performance
- **Minimize allocations** in hot paths
- **Use StringBuilder** for string building
- **Cache compiled regexes**
- **Consider memory usage** for large files

### Testing Performance
Add performance tests for critical paths:
```csharp
[Fact]
public void Parser_LargeFile_CompletesInReasonableTime()
{
    var largeGcode = GenerateLargeGcodeFile(10000); // 10k lines
    var stopwatch = Stopwatch.StartNew();
    
    var parser = new Parser(largeGcode);
    var codes = parser.Parse().ToList();
    
    stopwatch.Stop();
    Assert.True(stopwatch.ElapsedMilliseconds < 1000); // < 1 second
}
```

### Memory Efficiency
- **Use IEnumerable** for streaming when possible
- **Dispose resources** properly
- **Avoid large object heap** allocations

## Release Process

### Version Numbers
We use Semantic Versioning (SemVer):
- **MAJOR**: Breaking API changes
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, backward compatible

### Release Checklist
1. **Update version numbers** in AssemblyInfo.cs files
2. **Update CHANGELOG.md** with changes
3. **Create release notes**
4. **Tag the release**: `git tag v1.2.3`
5. **Build release artifacts**
6. **Update documentation** if needed

### Backward Compatibility
- **Maintain API compatibility** in minor releases
- **Deprecate features** before removing them
- **Document breaking changes** clearly
- **Provide migration guides** for major releases

## Getting Help

### Communication Channels
- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and community discussions
- **Pull Request Reviews**: Code review and feedback

### Maintainer Guidelines
- **Be respectful** and constructive in feedback
- **Provide clear guidance** for improvements
- **Help contributors** succeed with their contributions
- **Acknowledge contributions** in release notes

## Code of Conduct

We are committed to providing a friendly, safe, and welcoming environment for all contributors. Please:

- **Be respectful** of differing viewpoints and experiences
- **Give and receive constructive feedback** gracefully
- **Focus on what is best** for the community and project
- **Show empathy** towards other community members

## Recognition

Contributors will be recognized in:
- **Release notes** for significant contributions
- **README.md** contributors section
- **Git commit history** as the permanent record

Thank you for contributing to the Gcodes project! Your contributions help make G-code processing more accessible and powerful for everyone.