# Troubleshooting Guide

This guide helps you resolve common issues when using the Gcodes library. Issues are organized by category with solutions and workarounds.

## Table of Contents

1. [Build and Setup Issues](#build-and-setup-issues)
2. [Parsing Errors](#parsing-errors)
3. [Runtime Exceptions](#runtime-exceptions)
4. [Performance Issues](#performance-issues)
5. [Console Application Issues](#console-application-issues)
6. [Integration Problems](#integration-problems)
7. [Platform-Specific Issues](#platform-specific-issues)
8. [Common Usage Mistakes](#common-usage-mistakes)

## Build and Setup Issues

### MSBuild Not Found

**Problem**: `msbuild: command not found` or `MSBuild not found`

**Solution**:
1. Install Visual Studio Build Tools or Visual Studio
2. Use Developer Command Prompt for Visual Studio
3. Add MSBuild to PATH:
   ```bash
   # Typical paths
   C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin
   C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin
   ```

**Alternative**: Use Visual Studio IDE to build instead of command line.

### NuGet Restore Failed

**Problem**: `Package restore failed` or missing packages

**Solution**:
1. Update NuGet to latest version:
   ```bash
   nuget update -self
   ```
2. Clear NuGet cache:
   ```bash
   nuget locals all -clear
   ```
3. Restore packages manually:
   ```bash
   nuget restore Gcode.sln
   ```
4. Check internet connectivity and proxy settings

### Target Framework Not Supported

**Problem**: `.NET Framework 4.7.2 not found` or unsupported target framework

**Solution**:
1. Install .NET Framework 4.7.2 Developer Pack
2. Verify installation:
   ```bash
   # Check installed versions
   dir "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\"
   ```
3. Update project target if needed (not recommended):
   ```xml
   <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
   ```

### Assembly Load Errors

**Problem**: `Could not load file or assembly 'Gcodes'`

**Solution**:
1. Check reference paths in project file
2. Ensure DLL is in output directory
3. Verify target platform (x86/x64/AnyCPU) matches
4. Check .NET Framework version compatibility

**Example fix**:
```xml
<Reference Include="Gcodes">
  <HintPath>..\Gcodes\bin\Debug\Gcodes.dll</HintPath>
  <Private>True</Private>
</Reference>
```

## Parsing Errors

### UnrecognisedCharacterException

**Problem**: `UnrecognisedCharacterException: Invalid character '$' at line 1, column 8`

**Common Causes**:
- Special characters in G-code file
- Encoding issues (non-ASCII characters)
- Corrupted file data

**Solutions**:
1. **Check file encoding**:
   ```csharp
   // Specify encoding when reading
   string content = File.ReadAllText(filename, Encoding.UTF8);
   ```

2. **Clean the G-code**:
   ```csharp
   // Remove problematic characters
   string cleanGcode = gcode.Replace("$", "").Replace("@", "");
   ```

3. **Identify problematic lines**:
   ```csharp
   try
   {
       var parser = new Parser(gcode);
       var codes = parser.Parse().ToList();
   }
   catch (UnrecognisedCharacterException ex)
   {
       var lines = gcode.Split('\n');
       Console.WriteLine($"Problematic line {ex.Line}: {lines[ex.Line - 1]}");
       Console.WriteLine($"Character '{ex.Character}' at position {ex.Column}");
   }
   ```

### UnexpectedTokenException

**Problem**: `Expected Number but got G`

**Common Causes**:
- Malformed G-code syntax
- Missing numbers in commands
- Invalid command sequences

**Solutions**:
1. **Check G-code format**:
   ```gcode
   ; Bad
   G X10 Y20    ; Missing G-code number
   
   ; Good
   G01 X10 Y20  ; Complete G-code
   ```

2. **Validate syntax**:
   ```csharp
   public bool IsValidGcode(string line)
   {
       try
       {
           var parser = new Parser(line);
           parser.Parse().ToList();
           return true;
       }
       catch (UnexpectedTokenException)
       {
           return false;
       }
   }
   ```

### MissingNumberException

**Problem**: `Missing number in command`

**Solution**:
```csharp
// Add validation before parsing
string[] lines = gcode.Split('\n');
for (int i = 0; i < lines.Length; i++)
{
    string line = lines[i].Trim();
    if (line.StartsWith("G") && !char.IsDigit(line[1]))
    {
        Console.WriteLine($"Line {i + 1}: Missing G-code number");
    }
}
```

## Runtime Exceptions

### NullReferenceException in Parser

**Problem**: `Object reference not set to an instance of an object`

**Common Causes**:
- Null input to Parser constructor
- Empty token list
- Uninitialized collections

**Solution**:
```csharp
// Defensive programming
public void SafeParse(string input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Empty input provided");
        return;
    }
    
    try
    {
        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize()?.ToList();
        
        if (tokens == null || !tokens.Any())
        {
            Console.WriteLine("No tokens generated");
            return;
        }
        
        var parser = new Parser(tokens);
        var codes = parser.Parse().ToList();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Parse error: {ex.Message}");
    }
}
```

### ArgumentOutOfRangeException

**Problem**: Index out of range during parsing

**Solution**:
```csharp
// Check bounds before accessing
if (index < tokens.Count && tokens[index] != null)
{
    var token = tokens[index];
    // Process token
}
```

### InvalidOperationException in Emulator

**Problem**: Emulator state becomes invalid

**Solutions**:
1. **Initialize emulator properly**:
   ```csharp
   var emulator = new Emulator();
   emulator.InitialState = new MachineState
   {
       X = 0, Y = 0, Z = 0,
       FeedRate = 100 // Always set a default feed rate
   };
   ```

2. **Validate operations**:
   ```csharp
   public class SafeOperationFactory : OperationFactory
   {
       public override IOperation CreateOperation(Code code, MachineState state)
       {
           try
           {
               return base.CreateOperation(code, state);
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Failed to create operation for {code}: {ex.Message}");
               return new Noop(); // Return safe default
           }
       }
   }
   ```

## Performance Issues

### Slow Parsing of Large Files

**Problem**: Parsing takes too long for large G-code files

**Solutions**:
1. **Process in chunks**:
   ```csharp
   public void ProcessLargeFile(string filename)
   {
       const int chunkSize = 1000;
       var lines = File.ReadLines(filename);
       
       foreach (var chunk in lines.Chunk(chunkSize))
       {
           var chunkText = string.Join("\n", chunk);
           var parser = new Parser(chunkText);
           var codes = parser.Parse().ToList();
           ProcessCodes(codes);
       }
   }
   ```

2. **Use streaming approach**:
   ```csharp
   public void StreamProcess(string filename)
   {
       using (var reader = new StreamReader(filename))
       {
           string line;
           while ((line = reader.ReadLine()) != null)
           {
               if (!string.IsNullOrWhiteSpace(line))
               {
                   try
                   {
                       var parser = new Parser(line);
                       var codes = parser.Parse();
                       foreach (var code in codes)
                       {
                           ProcessCode(code);
                       }
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"Error parsing line: {line}");
                   }
               }
           }
       }
   }
   ```

### Memory Usage Issues

**Problem**: High memory consumption

**Solutions**:
1. **Dispose resources**:
   ```csharp
   using (var stream = File.OpenRead(filename))
   using (var reader = new StreamReader(stream))
   {
       // Process file
   }
   ```

2. **Avoid storing large collections**:
   ```csharp
   // Instead of
   var allCodes = parser.Parse().ToList(); // Loads everything into memory
   
   // Use
   foreach (var code in parser.Parse()) // Lazy evaluation
   {
       ProcessCode(code);
   }
   ```

## Console Application Issues

### Command Line Arguments Not Working

**Problem**: Console app doesn't recognize arguments

**Solution**:
```bash
# Correct usage
Gcodes.Console.exe "path\to\file.gcode" --verbose

# Check help
Gcodes.Console.exe --help
```

### File Not Found Errors

**Problem**: `FileNotFoundException` when running console app

**Solutions**:
1. **Use absolute paths**:
   ```bash
   Gcodes.Console.exe "C:\full\path\to\file.gcode"
   ```

2. **Check working directory**:
   ```bash
   # Change to file directory first
   cd C:\gcode\files
   Gcodes.Console.exe file.gcode
   ```

3. **Verify file exists**:
   ```csharp
   if (!File.Exists(args[0]))
   {
       Console.WriteLine($"File not found: {args[0]}");
       return 1;
   }
   ```

### Logging Issues

**Problem**: No log output or logging errors

**Solutions**:
1. **Check log configuration**:
   ```csharp
   Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Debug()
       .WriteTo.Console()
       .WriteTo.File("logs/gcodes.log")
       .CreateLogger();
   ```

2. **Verify Serilog installation**:
   ```xml
   <package id="Serilog" version="2.x.x" targetFramework="net472" />
   <package id="Serilog.Sinks.Console" version="3.x.x" targetFramework="net472" />
   ```

## Integration Problems

### Reference Errors in Other Projects

**Problem**: Cannot reference Gcodes library

**Solutions**:
1. **Check target framework compatibility**:
   ```xml
   <!-- Your project should target .NET Framework 4.7.2 or higher -->
   <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
   ```

2. **Add proper references**:
   ```xml
   <ItemGroup>
     <Reference Include="Gcodes">
       <HintPath>..\Gcodes\bin\Release\Gcodes.dll</HintPath>
     </Reference>
   </ItemGroup>
   ```

3. **Copy local if needed**:
   ```xml
   <Reference Include="Gcodes">
     <HintPath>..\Gcodes\bin\Release\Gcodes.dll</HintPath>
     <Private>True</Private>
   </Reference>
   ```

### Namespace Issues

**Problem**: `The type or namespace name 'Gcodes' could not be found`

**Solution**:
```csharp
// Add using statements
using Gcodes;
using Gcodes.Ast;
using Gcodes.Tokens;
using Gcodes.Runtime;
```

### Version Conflicts

**Problem**: Assembly version conflicts

**Solutions**:
1. **Use binding redirects**:
   ```xml
   <configuration>
     <runtime>
       <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
         <dependentAssembly>
           <assemblyIdentity name="Gcodes" culture="neutral" />
           <bindingRedirect oldVersion="0.0.0.0-1.0.0.0" newVersion="1.0.0.0" />
         </dependentAssembly>
       </assemblyBinding>
     </runtime>
   </configuration>
   ```

2. **Check assembly versions**:
   ```csharp
   var assembly = Assembly.LoadFrom("Gcodes.dll");
   Console.WriteLine($"Version: {assembly.GetName().Version}");
   ```

## Platform-Specific Issues

### Linux/Mac Compatibility

**Problem**: Cannot run on non-Windows platforms

**Explanation**: The library targets .NET Framework 4.7.2, which is Windows-only.

**Alternatives**:
1. **Use Windows VM or container**
2. **Request .NET Core/5+ port** (future enhancement)
3. **Run via Wine** (limited support)

### Path Separator Issues

**Problem**: File paths not working across platforms

**Solution**:
```csharp
// Use Path.Combine for cross-platform paths
string filePath = Path.Combine("directory", "file.gcode");

// Or use forward slashes (work on Windows too)
string filePath = "directory/file.gcode";
```

## Common Usage Mistakes

### Not Handling Exceptions

**Mistake**:
```csharp
// Dangerous - exceptions will crash the app
var parser = new Parser(gcode);
var codes = parser.Parse().ToList();
```

**Correct**:
```csharp
try
{
    var parser = new Parser(gcode);
    var codes = parser.Parse().ToList();
}
catch (UnrecognisedCharacterException ex)
{
    Console.WriteLine($"Invalid character: {ex.Character} at {ex.Line}:{ex.Column}");
}
catch (Exception ex)
{
    Console.WriteLine($"Parse error: {ex.Message}");
}
```

### Assuming Arguments Exist

**Mistake**:
```csharp
var gcode = parser.ParseGCode();
double x = gcode.ValueFor(ArgumentKind.X).Value; // NullReferenceException if no X
```

**Correct**:
```csharp
var gcode = parser.ParseGCode();
double? x = gcode.ValueFor(ArgumentKind.X);
if (x.HasValue)
{
    Console.WriteLine($"X position: {x.Value}");
}
else
{
    Console.WriteLine("No X coordinate specified");
}
```

### Not Initializing Emulator State

**Mistake**:
```csharp
var emulator = new Emulator();
emulator.Run(gcode); // State is null
```

**Correct**:
```csharp
var emulator = new Emulator();
emulator.InitialState = new MachineState
{
    X = 0, Y = 0, Z = 0,
    FeedRate = 100
};
emulator.Run(gcode);
```

### Memory Leaks with Large Files

**Mistake**:
```csharp
// Loads entire file into memory
var allCodes = new List<Code>();
foreach (var file in largeFiles)
{
    var parser = new Parser(File.ReadAllText(file));
    allCodes.AddRange(parser.Parse().ToList());
}
```

**Correct**:
```csharp
// Process files individually
foreach (var file in largeFiles)
{
    var parser = new Parser(File.ReadAllText(file));
    foreach (var code in parser.Parse())
    {
        ProcessCode(code);
    }
    // Parser and codes are eligible for GC after each file
}
```

## Getting Help

### Debug Information

When reporting issues, include:

1. **Code sample** that reproduces the problem
2. **Complete error message** and stack trace
3. **Input data** (G-code file or string)
4. **Environment details**:
   ```
   - OS: Windows 10 x64
   - .NET Framework: 4.7.2
   - Visual Studio: 2019
   - Library version: 1.0.0
   ```

### Debug Helpers

Use these helpers to diagnose issues:

```csharp
public static class DebugHelpers
{
    public static void DumpTokens(string gcode)
    {
        var lexer = new Lexer(gcode);
        var tokens = lexer.Tokenize().ToList();
        
        Console.WriteLine($"Generated {tokens.Count} tokens:");
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            Console.WriteLine($"  {i}: {token.Kind} '{token.Value}' at {token.Span.Start}-{token.Span.End}");
        }
    }
    
    public static void DumpCodes(string gcode)
    {
        try
        {
            var parser = new Parser(gcode);
            var codes = parser.Parse().ToList();
            
            Console.WriteLine($"Parsed {codes.Count} codes:");
            foreach (var code in codes)
            {
                Console.WriteLine($"  {code.GetType().Name}: {code}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parse failed: {ex}");
        }
    }
}
```

### Community Support

- **GitHub Issues**: For bugs and feature requests
- **GitHub Discussions**: For questions and help
- **Wiki**: For documentation and examples

When asking for help, provide a minimal, complete, and verifiable example that demonstrates the issue.