# Getting Started

This guide will help you get up and running with the Gcodes library in your C# projects.

## Prerequisites

- **.NET Framework 4.7.2** or later
- **Visual Studio 2017** or later (recommended)
- **MSBuild** (for building from command line)

## Installation

### Option 1: Clone the Repository

```bash
git clone https://github.com/J-DRD/Michael-F-Bryan-gcodes.git
cd Michael-F-Bryan-gcodes
```

### Option 2: Download Release

1. Go to the [Releases page](https://github.com/J-DRD/Michael-F-Bryan-gcodes/releases)
2. Download the latest release zip file
3. Extract to your desired location

### Option 3: Add as Git Submodule

```bash
cd your-project
git submodule add https://github.com/J-DRD/Michael-F-Bryan-gcodes.git gcodes
git submodule update --init --recursive
```

## Building the Project

### Using Visual Studio

1. Open `Gcode.sln` in Visual Studio
2. Build → Build Solution (Ctrl+Shift+B)
3. The compiled libraries will be in `bin/Debug/` or `bin/Release/`

### Using MSBuild (Command Line)

```bash
# Restore NuGet packages
nuget restore

# Build in Release mode
msbuild Gcode.sln /p:Configuration=Release

# Build specific project
msbuild Gcodes/Gcodes.csproj /p:Configuration=Release
```

## Adding References

### In Visual Studio

1. Right-click your project → Add → Reference
2. Browse to the compiled `Gcodes.dll`
3. Add reference

### In Project File (.csproj)

```xml
<Reference Include="Gcodes">
  <HintPath>path\to\Gcodes.dll</HintPath>
</Reference>
```

## Basic Usage

### 1. Simple G-code Parsing

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Gcodes;
using Gcodes.Ast;

class Program
{
    static void Main(string[] args)
    {
        // Sample G-code
        string gcode = @"
            N10 G01 X10.5 Y20.0 F1000
            N20 G02 X15.0 Y25.0 I2.5 J2.5
            N30 M03 S1200
        ";

        // Parse the G-code
        var parser = new Parser(gcode);
        var codes = parser.Parse().ToList();

        // Iterate through parsed codes
        foreach (var code in codes)
        {
            Console.WriteLine($"Parsed: {code}");
            
            if (code is Gcode gCode)
            {
                Console.WriteLine($"  G-code: G{gCode.Number}");
                foreach (var arg in gCode.Arguments)
                {
                    Console.WriteLine($"    {arg.Kind}: {arg.Value}");
                }
            }
            else if (code is Mcode mCode)
            {
                Console.WriteLine($"  M-code: M{mCode.Number}");
            }
        }
    }
}
```

### 2. Reading from File

```csharp
using System.IO;
using Gcodes;

static void ParseGcodeFile(string filename)
{
    string src = File.ReadAllText(filename);
    
    var lexer = new Lexer(src);
    var tokens = lexer.Tokenize().ToList();
    
    var parser = new Parser(tokens);
    var gcodes = parser.Parse().ToList();
    
    // Process the parsed G-codes
    foreach (var code in gcodes)
    {
        Console.WriteLine(code);
    }
}
```

### 3. Working with Arguments

```csharp
using Gcodes.Ast;

// Parse a G-code with arguments
var parser = new Parser("G01 X10 Y20 Z5 F1000");
var codes = parser.Parse().ToList();
var gcode = codes[0] as Gcode;

// Get specific argument values
double? xPos = gcode.ValueFor(ArgumentKind.X);        // 10
double? yPos = gcode.ValueFor(ArgumentKind.Y);        // 20
double? zPos = gcode.ValueFor(ArgumentKind.Z);        // 5
double? feedRate = gcode.ValueFor(ArgumentKind.FeedRate); // 1000

// Check if argument exists
if (xPos.HasValue)
{
    Console.WriteLine($"X position: {xPos.Value}");
}

// Get multiple possible arguments (returns first found)
double? position = gcode.ValueFor(ArgumentKind.X, ArgumentKind.Y, ArgumentKind.Z);
```

### 4. Event Handling During Parsing

```csharp
using Gcodes;

var lexer = new Lexer(gcodeText);

// Handle comments detected during lexing
lexer.CommentDetected += (sender, args) =>
{
    Console.WriteLine($"Comment found: {args.Comment}");
};

var tokens = lexer.Tokenize().ToList();
var parser = new Parser(tokens);
var codes = parser.Parse().ToList();
```

## Using the Console Application

The project includes a console application for processing G-code files:

### Build the Console App

```bash
msbuild Gcodes.Console/Gcodes.Console.csproj /p:Configuration=Release
```

### Run the Console App

```bash
# Basic usage
Gcodes.Console.exe myfile.gcode

# Verbose output
Gcodes.Console.exe myfile.gcode --verbose

# Get help
Gcodes.Console.exe --help
```

### Console Output Example

```
[14:30:15 DBG] Reading C:\gcode\test.gcode
[14:30:15 DBG] Comment: "ALPHA V2.0 P80 U 5 FF MV" at L1,C1
[14:30:15 DBG] Executing G78 with Noop { Duration: 00:00:00 }
[14:30:15 DBG] Executing M13 with Noop { Duration: 00:00:00 }
[14:30:15 DBG] State changed to { X: 0, Y: 0, Z: 0, FeedRate: 0 } (t: 0)
```

## Testing Your Integration

Create a simple test to verify everything is working:

```csharp
using Xunit;
using Gcodes;
using Gcodes.Ast;

public class BasicIntegrationTest
{
    [Fact]
    public void CanParseSimpleGcode()
    {
        var parser = new Parser("G01 X10 Y20");
        var codes = parser.Parse().ToList();
        
        Assert.Single(codes);
        Assert.IsType<Gcode>(codes[0]);
        
        var gcode = codes[0] as Gcode;
        Assert.Equal(1, gcode.Number);
        Assert.Equal(10.0, gcode.ValueFor(ArgumentKind.X));
        Assert.Equal(20.0, gcode.ValueFor(ArgumentKind.Y));
    }
}
```

## Next Steps

- Check out the [API Reference](API-Reference) for complete class documentation
- See [Examples and Tutorials](Examples-and-Tutorials) for more advanced usage
- Learn about the [Architecture Overview](Architecture-Overview) to understand the library's design
- Read the [G-code Reference](Gcode-Reference) for supported G-code syntax

## Common Issues

### Build Errors

**Issue**: "Could not find MSBuild"
**Solution**: Install Visual Studio Build Tools or use Developer Command Prompt

**Issue**: "Package restore failed"
**Solution**: Run `nuget restore` before building

**Issue**: "Target framework not supported"
**Solution**: Ensure you have .NET Framework 4.7.2 or later installed

### Runtime Errors

**Issue**: "UnrecognisedCharacterException"
**Solution**: Check your G-code file for invalid characters or syntax

**Issue**: "FileNotFoundException for Gcodes.dll"
**Solution**: Ensure the library is properly referenced and the DLL is accessible

For more troubleshooting help, see the [Troubleshooting Guide](Troubleshooting).