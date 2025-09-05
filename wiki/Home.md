# Gcodes - C# G-code Parser and Interpreter

[![Build status](https://ci.appveyor.com/api/projects/status/b4t1cp42205pdqta?svg=true)](https://ci.appveyor.com/project/Michael-F-Bryan/gcodes)
[![GitHub Actions](https://github.com/J-DRD/Michael-F-Bryan-gcodes/workflows/Build%20and%20Package/badge.svg)](https://github.com/J-DRD/Michael-F-Bryan-gcodes/actions)

Welcome to the **Gcodes** project wiki! This is a comprehensive C# library for parsing, interpreting, and simulating G-code programs used in CNC machining and 3D printing.

## What is Gcodes?

Gcodes is a .NET Framework library that provides:

- **Lexical Analysis**: Convert G-code text into tokens
- **Parsing**: Transform tokens into structured Abstract Syntax Tree (AST) objects
- **Interpretation**: Execute G-code programs with customizable operations
- **Simulation**: Emulate CNC machine states and movements
- **Console Application**: Command-line tool for processing G-code files

## Key Features

- ✅ **Complete G-code Parsing**: Supports G-codes, M-codes, O-codes, T-codes, and line numbers
- ✅ **Extensible Architecture**: Plugin-based operation system for custom machine behaviors
- ✅ **Rich AST**: Full abstract syntax tree with location information for error reporting
- ✅ **Event-Driven**: Hook into parsing and execution events for custom handling
- ✅ **Cross-Platform**: Built on .NET Framework 4.7.2
- ✅ **Well-Tested**: Comprehensive unit test suite with xUnit
- ✅ **CI/CD Ready**: GitHub Actions integration for automated builds and testing

## Quick Example

```csharp
using Gcodes;
using Gcodes.Ast;

// Parse a G-code file
string gcode = "G01 X10 Y20 F1000";
var parser = new Parser(gcode);
var codes = parser.Parse().ToList();

// Access parsed G-code
var firstCode = codes[0] as Gcode;
Console.WriteLine($"G-code: G{firstCode.Number}");
Console.WriteLine($"X position: {firstCode.ValueFor(ArgumentKind.X)}");
Console.WriteLine($"Feed rate: {firstCode.ValueFor(ArgumentKind.FeedRate)}");
```

## Wiki Navigation

| Section | Description |
|---------|-------------|
| [Getting Started](Getting-Started) | Installation, basic usage, and first steps |
| [API Reference](API-Reference) | Complete API documentation and class reference |
| [Architecture Overview](Architecture-Overview) | Deep dive into the library's design and structure |
| [Examples and Tutorials](Examples-and-Tutorials) | Code examples and step-by-step tutorials |
| [G-code Reference](Gcode-Reference) | Supported G-code commands and syntax |
| [Contributing Guide](Contributing-Guide) | How to contribute to the project |
| [Troubleshooting](Troubleshooting) | Common issues and solutions |
| [CI/CD Documentation](CICD-Documentation) | GitHub Actions workflows and build process |

## Project Structure

```
Gcodes/                    # Main library
├── Ast/                   # Abstract Syntax Tree classes
├── Tokens/               # Tokenization and lexical analysis
├── Runtime/              # Execution and simulation engine
├── Properties/           # Assembly information
└── *.cs                  # Core parsing and interpretation classes

Gcodes.Console/           # Command-line application
├── Program.cs            # Main entry point
├── LoggingSimulator.cs   # Logging emulator implementation
└── Properties/           # Assembly information

Gcodes.Test/              # Unit tests
├── Fixtures/             # Test G-code files
├── Runtime/              # Tests for simulation engine
└── *.cs                  # Test classes

.github/                  # GitHub Actions workflows
docs/                     # Documentation (if any)
```

## License

This project is licensed under the MIT License. See the [LICENSE.md](https://github.com/J-DRD/Michael-F-Bryan-gcodes/blob/master/LICENSE.md) file for details.

## Support and Community

- 🐛 **Issues**: [GitHub Issues](https://github.com/J-DRD/Michael-F-Bryan-gcodes/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/J-DRD/Michael-F-Bryan-gcodes/discussions)
- 📝 **Contributing**: See our [Contributing Guide](Contributing-Guide)

---

*This wiki is maintained alongside the project. Last updated: December 2024*