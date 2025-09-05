# Examples and Tutorials

This page provides comprehensive examples and step-by-step tutorials for using the Gcodes library in various scenarios.

## Table of Contents

1. [Basic Parsing Examples](#basic-parsing-examples)
2. [Advanced Parser Usage](#advanced-parser-usage)
3. [Simulation and Emulation](#simulation-and-emulation)
4. [Custom Operations](#custom-operations)
5. [Error Handling](#error-handling)
6. [Real-World Scenarios](#real-world-scenarios)
7. [Performance Optimization](#performance-optimization)

## Basic Parsing Examples

### Example 1: Simple G-code Parsing

```csharp
using System;
using System.Linq;
using Gcodes;
using Gcodes.Ast;

class BasicParsingExample
{
    static void Main()
    {
        string gcode = @"
            N10 G90 G21
            N20 G00 X0 Y0 Z5
            N30 G01 Z-2 F300
            N40 G01 X10 Y10 F1000
            N50 G00 Z5
            N60 M30
        ";

        var parser = new Parser(gcode);
        var codes = parser.Parse().ToList();

        Console.WriteLine($"Parsed {codes.Count} commands:");
        
        foreach (var code in codes)
        {
            Console.WriteLine($"  {code}");
        }
    }
}
```

**Output:**
```
Parsed 6 commands:
  G90
  G21
  G0 X0 Y0 Z5
  G1 Z-2 F300
  G1 X10 Y10 F1000
  G0 Z5
  M30
```

### Example 2: Extracting Coordinate Information

```csharp
using System;
using System.Linq;
using Gcodes;
using Gcodes.Ast;

class CoordinateExtraction
{
    static void Main()
    {
        string gcode = "G01 X10.5 Y-20.0 Z5.75 F1000";
        
        var parser = new Parser(gcode);
        var codes = parser.Parse().ToList();
        var moveCommand = codes[0] as Gcode;

        Console.WriteLine($"G-code: G{moveCommand.Number}");
        Console.WriteLine("Coordinates:");
        
        // Extract specific coordinates
        var x = moveCommand.ValueFor(ArgumentKind.X);
        var y = moveCommand.ValueFor(ArgumentKind.Y);
        var z = moveCommand.ValueFor(ArgumentKind.Z);
        var feedRate = moveCommand.ValueFor(ArgumentKind.FeedRate);

        if (x.HasValue) Console.WriteLine($"  X: {x.Value}");
        if (y.HasValue) Console.WriteLine($"  Y: {y.Value}");
        if (z.HasValue) Console.WriteLine($"  Z: {z.Value}");
        if (feedRate.HasValue) Console.WriteLine($"  Feed Rate: {feedRate.Value}");
        
        // Alternative: Iterate through all arguments
        Console.WriteLine("\nAll arguments:");
        foreach (var arg in moveCommand.Arguments)
        {
            Console.WriteLine($"  {arg.Kind}: {arg.Value}");
        }
    }
}
```

### Example 3: Working with Different Code Types

```csharp
using System;
using System.IO;
using System.Linq;
using Gcodes;
using Gcodes.Ast;

class CodeTypeExample
{
    static void Main()
    {
        string gcode = @"
            O1234
            N10 T01 M06
            N20 M03 S1200
            N30 G00 X10 Y20
            N40 G01 Z-5 F500
            N50 M05
            N60 M30
        ";

        var parser = new Parser(gcode);
        var codes = parser.Parse().ToList();

        foreach (var code in codes)
        {
            switch (code)
            {
                case Ocode ocode:
                    Console.WriteLine($"Program Number: O{ocode.Number}");
                    break;
                    
                case Tcode tcode:
                    Console.WriteLine($"Tool Selection: T{tcode.Number:00}");
                    break;
                    
                case Gcode gcode:
                    Console.WriteLine($"G-code: G{gcode.Number:00}");
                    if (gcode.Arguments.Any())
                    {
                        var args = string.Join(", ", 
                            gcode.Arguments.Select(a => $"{a.Kind}{a.Value}"));
                        Console.WriteLine($"  Arguments: {args}");
                    }
                    break;
                    
                case Mcode mcode:
                    Console.WriteLine($"M-code: M{mcode.Number:00}");
                    break;
                    
                case LineNumber lineNum:
                    Console.WriteLine($"Line Number: N{lineNum.Number}");
                    break;
            }
        }
    }
}
```

## Advanced Parser Usage

### Example 4: Comment Detection and Processing

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Gcodes;
using Gcodes.Ast;

class CommentHandlingExample
{
    static void Main()
    {
        string gcode = @"
            ; This is a program header
            N10 G90 G21 (Absolute mode, metric)
            N20 G00 X0 Y0 ; Rapid to origin
            N30 M03 S1200 (Start spindle)
            ; End of program
        ";

        var comments = new List<string>();
        
        var lexer = new Lexer(gcode);
        lexer.CommentDetected += (sender, args) =>
        {
            comments.Add(args.Comment);
            Console.WriteLine($"Comment found: '{args.Comment}'");
        };

        var tokens = lexer.Tokenize().ToList();
        var parser = new Parser(tokens);
        var codes = parser.Parse().ToList();

        Console.WriteLine($"\nFound {comments.Count} comments");
        Console.WriteLine($"Parsed {codes.Count} commands");
    }
}
```

### Example 5: Error Location Tracking

```csharp
using System;
using Gcodes;

class ErrorLocationExample
{
    static void Main()
    {
        string gcode = @"
            N10 G90 G21
            N20 G01 X10$ Y20  ; Invalid character
            N30 M30
        ";

        try
        {
            var fileMap = new FileMap(gcode);
            var parser = new Parser(gcode);
            var codes = parser.Parse().ToList();
        }
        catch (UnrecognisedCharacterException ex)
        {
            Console.WriteLine($"Parse Error:");
            Console.WriteLine($"  Character: '{ex.Character}'");
            Console.WriteLine($"  Line: {ex.Line}");
            Console.WriteLine($"  Column: {ex.Column}");
            
            // Show context around the error
            var lines = gcode.Split('\n');
            if (ex.Line - 1 < lines.Length)
            {
                Console.WriteLine($"  Context: {lines[ex.Line - 1].Trim()}");
                Console.WriteLine($"           {new string(' ', ex.Column - 1)}^");
            }
        }
    }
}
```

## Simulation and Emulation

### Example 6: Basic Machine Simulation

```csharp
using System;
using Gcodes.Runtime;

class BasicSimulationExample
{
    static void Main()
    {
        string gcode = @"
            G90 G21
            G00 X0 Y0 Z5
            G01 Z0 F300
            G01 X10 Y0 F1000
            G01 X10 Y10
            G01 X0 Y10
            G01 X0 Y0
            G00 Z5
        ";

        // Create emulator with initial state
        var emulator = new Emulator();
        emulator.InitialState = new MachineState
        {
            X = 0, Y = 0, Z = 10,
            FeedRate = 100,
            SpindleSpeed = 0
        };

        // Subscribe to state changes
        emulator.StateChanged += (sender, args) =>
        {
            var state = args.NewState;
            Console.WriteLine($"[{args.Time:F2}s] Position: X{state.X:F1} Y{state.Y:F1} Z{state.Z:F1} F{state.FeedRate}");
        };

        // Subscribe to operation execution
        emulator.OperationExecuted += (sender, args) =>
        {
            Console.WriteLine($"Executing: {args.Code} (Duration: {args.Operation.Duration})");
        };

        // Run the simulation
        Console.WriteLine("Starting simulation...");
        emulator.Run(gcode);
        Console.WriteLine("Simulation complete.");
    }
}
```

### Example 7: Advanced Emulator with Logging

```csharp
using System;
using Gcodes.Console;
using Gcodes.Runtime;
using Serilog;

class AdvancedEmulatorExample
{
    static void Main()
    {
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        string gcode = @"
            T01 M06
            M03 S1200
            G00 X10 Y10
            G01 Z-5 F300
            G02 X20 Y10 I5 J0 F500
            G00 Z5
            M05
        ";

        // Use the logging emulator from the console project
        var emulator = new LoggingEmulator();
        emulator.InitialState = new MachineState();
        
        // Configure operations factory to ignore certain G-codes
        emulator.Operations.IgnoreGcode(78); // Ignore G78 if present
        
        emulator.Run(gcode);
    }
}
```

## Custom Operations

### Example 8: Creating Custom Operations

```csharp
using System;
using Gcodes.Runtime;
using Gcodes.Ast;

// Custom operation for drilling
public class DrillOperation : IOperation
{
    public double Depth { get; }
    public double FeedRate { get; }
    
    public DrillOperation(double depth, double feedRate)
    {
        Depth = depth;
        FeedRate = feedRate;
    }
    
    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Abs(Depth) / FeedRate * 60);
    
    public MachineState Execute(MachineState initialState)
    {
        var newState = new MachineState(initialState);
        newState.Z = initialState.Z - Depth;
        return newState;
    }
    
    public override string ToString() => $"Drill(depth: {Depth}, feed: {FeedRate})";
}

// Custom operation factory
public class CustomOperationFactory : OperationFactory
{
    public override IOperation CreateOperation(Code code, MachineState state)
    {
        if (code is Gcode gcode)
        {
            // Custom drilling cycle (G81)
            if (gcode.Number == 81)
            {
                var depth = gcode.ValueFor(ArgumentKind.Z) ?? 0;
                var feedRate = gcode.ValueFor(ArgumentKind.FeedRate) ?? state.FeedRate;
                return new DrillOperation(Math.Abs(depth), feedRate);
            }
        }
        
        return base.CreateOperation(code, state);
    }
}

class CustomOperationExample
{
    static void Main()
    {
        string gcode = @"
            G00 X10 Y10 Z5
            G81 Z-5 F200  ; Custom drilling operation
            G00 Z5
        ";

        var emulator = new Emulator();
        emulator.Operations = new CustomOperationFactory();
        emulator.InitialState = new MachineState();
        
        emulator.OperationExecuted += (sender, args) =>
        {
            Console.WriteLine($"Operation: {args.Operation}");
        };
        
        emulator.Run(gcode);
    }
}
```

### Example 9: Custom Interpreter for Validation

```csharp
using System;
using System.Collections.Generic;
using Gcodes;
using Gcodes.Ast;

public class ValidationInterpreter : Interpreter
{
    private readonly List<string> errors = new List<string>();
    private double currentX = 0, currentY = 0, currentZ = 0;
    private double maxX = 100, maxY = 100, maxZ = 50;  // Machine limits
    
    public IReadOnlyList<string> Errors => errors;
    
    public override void Visit(Gcode code)
    {
        switch (code.Number)
        {
            case 0:  // G00 - Rapid
            case 1:  // G01 - Linear
                ValidateMovement(code);
                break;
            case 2:  // G02 - Clockwise arc
            case 3:  // G03 - Counter-clockwise arc
                ValidateArc(code);
                break;
        }
        
        // Update current position
        currentX = code.ValueFor(ArgumentKind.X) ?? currentX;
        currentY = code.ValueFor(ArgumentKind.Y) ?? currentY;
        currentZ = code.ValueFor(ArgumentKind.Z) ?? currentZ;
    }
    
    private void ValidateMovement(Gcode code)
    {
        var x = code.ValueFor(ArgumentKind.X) ?? currentX;
        var y = code.ValueFor(ArgumentKind.Y) ?? currentY;
        var z = code.ValueFor(ArgumentKind.Z) ?? currentZ;
        
        if (x < 0 || x > maxX) errors.Add($"X coordinate {x} out of range [0, {maxX}]");
        if (y < 0 || y > maxY) errors.Add($"Y coordinate {y} out of range [0, {maxY}]");
        if (z < -maxZ || z > maxZ) errors.Add($"Z coordinate {z} out of range [-{maxZ}, {maxZ}]");
        
        if (code.Number == 1) // G01 requires feed rate
        {
            var feedRate = code.ValueFor(ArgumentKind.FeedRate);
            if (!feedRate.HasValue) errors.Add("G01 command missing feed rate");
        }
    }
    
    private void ValidateArc(Gcode code)
    {
        var i = code.ValueFor(ArgumentKind.I);
        var j = code.ValueFor(ArgumentKind.J);
        
        if (!i.HasValue && !j.HasValue)
        {
            errors.Add($"G{code.Number:00} arc command missing I and J parameters");
        }
    }
}

class ValidationExample
{
    static void Main()
    {
        string gcode = @"
            G00 X150 Y50    ; Out of X range
            G01 X10 Y10     ; Missing feed rate
            G02 X20 Y20     ; Missing arc parameters
        ";

        var validator = new ValidationInterpreter();
        validator.Run(gcode);
        
        Console.WriteLine($"Validation completed with {validator.Errors.Count} errors:");
        foreach (var error in validator.Errors)
        {
            Console.WriteLine($"  Error: {error}");
        }
    }
}
```

## Error Handling

### Example 10: Robust Error Handling

```csharp
using System;
using Gcodes;

class RobustErrorHandling
{
    static void ProcessGcodeFile(string filename)
    {
        try
        {
            string content = File.ReadAllText(filename);
            var fileMap = new FileMap(content);
            
            var lexer = new Lexer(content);
            var parser = new Parser(lexer.Tokenize().ToList());
            var codes = parser.Parse().ToList();
            
            Console.WriteLine($"Successfully parsed {codes.Count} commands from {filename}");
        }
        catch (UnrecognisedCharacterException ex)
        {
            Console.WriteLine($"Lexer Error in {filename}:");
            Console.WriteLine($"  Invalid character '{ex.Character}' at line {ex.Line}, column {ex.Column}");
        }
        catch (UnexpectedTokenException ex)
        {
            Console.WriteLine($"Parser Error in {filename}:");
            Console.WriteLine($"  Expected {ex.Expected} but found {ex.Got}");
            
            if (ex.Location != null)
            {
                // Use FileMap to get better error location
                Console.WriteLine($"  At position {ex.Location.Start}-{ex.Location.End}");
            }
        }
        catch (MissingNumberException ex)
        {
            Console.WriteLine($"Parser Error in {filename}:");
            Console.WriteLine($"  Missing number in command");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File not found: {filename}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error processing {filename}: {ex.Message}");
        }
    }
    
    static void Main()
    {
        string[] testFiles = { "test1.gcode", "test2.gcode", "nonexistent.gcode" };
        
        foreach (var file in testFiles)
        {
            ProcessGcodeFile(file);
        }
    }
}
```

## Real-World Scenarios

### Example 11: G-code File Analyzer

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gcodes;
using Gcodes.Ast;

public class GcodeAnalyzer
{
    public class AnalysisReport
    {
        public int TotalCommands { get; set; }
        public int GCodes { get; set; }
        public int MCodes { get; set; }
        public Dictionary<int, int> GCodeCounts { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> MCodeCounts { get; set; } = new Dictionary<int, int>();
        public double MinX { get; set; } = double.MaxValue;
        public double MaxX { get; set; } = double.MinValue;
        public double MinY { get; set; } = double.MaxValue;
        public double MaxY { get; set; } = double.MinValue;
        public double MinZ { get; set; } = double.MaxValue;
        public double MaxZ { get; set; } = double.MinValue;
        public List<int> ToolsUsed { get; set; } = new List<int>();
        public TimeSpan EstimatedTime { get; set; }
    }
    
    public static AnalysisReport AnalyzeFile(string filename)
    {
        var report = new AnalysisReport();
        string content = File.ReadAllText(filename);
        
        var parser = new Parser(content);
        var codes = parser.Parse().ToList();
        
        report.TotalCommands = codes.Count;
        
        foreach (var code in codes)
        {
            switch (code)
            {
                case Gcode gcode:
                    report.GCodes++;
                    report.GCodeCounts[gcode.Number] = report.GCodeCounts.GetValueOrDefault(gcode.Number) + 1;
                    
                    // Track coordinate ranges
                    UpdateCoordinateRange(gcode, report);
                    break;
                    
                case Mcode mcode:
                    report.MCodes++;
                    report.MCodeCounts[mcode.Number] = report.MCodeCounts.GetValueOrDefault(mcode.Number) + 1;
                    break;
                    
                case Tcode tcode:
                    if (!report.ToolsUsed.Contains(tcode.Number))
                        report.ToolsUsed.Add(tcode.Number);
                    break;
            }
        }
        
        return report;
    }
    
    private static void UpdateCoordinateRange(Gcode gcode, AnalysisReport report)
    {
        var x = gcode.ValueFor(ArgumentKind.X);
        var y = gcode.ValueFor(ArgumentKind.Y);
        var z = gcode.ValueFor(ArgumentKind.Z);
        
        if (x.HasValue)
        {
            report.MinX = Math.Min(report.MinX, x.Value);
            report.MaxX = Math.Max(report.MaxX, x.Value);
        }
        if (y.HasValue)
        {
            report.MinY = Math.Min(report.MinY, y.Value);
            report.MaxY = Math.Max(report.MaxY, y.Value);
        }
        if (z.HasValue)
        {
            report.MinZ = Math.Min(report.MinZ, z.Value);
            report.MaxZ = Math.Max(report.MaxZ, z.Value);
        }
    }
    
    public static void PrintReport(AnalysisReport report)
    {
        Console.WriteLine("G-code Analysis Report");
        Console.WriteLine("======================");
        Console.WriteLine($"Total Commands: {report.TotalCommands}");
        Console.WriteLine($"G-codes: {report.GCodes}");
        Console.WriteLine($"M-codes: {report.MCodes}");
        
        Console.WriteLine("\nCoordinate Range:");
        Console.WriteLine($"  X: {report.MinX:F2} to {report.MaxX:F2} (range: {report.MaxX - report.MinX:F2})");
        Console.WriteLine($"  Y: {report.MinY:F2} to {report.MaxY:F2} (range: {report.MaxY - report.MinY:F2})");
        Console.WriteLine($"  Z: {report.MinZ:F2} to {report.MaxZ:F2} (range: {report.MaxZ - report.MinZ:F2})");
        
        if (report.ToolsUsed.Any())
        {
            Console.WriteLine($"\nTools Used: {string.Join(", ", report.ToolsUsed.Select(t => $"T{t:00}"))}");
        }
        
        Console.WriteLine("\nMost Common G-codes:");
        foreach (var kvp in report.GCodeCounts.OrderByDescending(x => x.Value).Take(5))
        {
            Console.WriteLine($"  G{kvp.Key:00}: {kvp.Value} times");
        }
    }
}

class AnalyzerExample
{
    static void Main()
    {
        string testFile = "sample.gcode";
        
        try
        {
            var report = GcodeAnalyzer.AnalyzeFile(testFile);
            GcodeAnalyzer.PrintReport(report);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing file: {ex.Message}");
        }
    }
}
```

### Example 12: G-code Converter

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gcodes;
using Gcodes.Ast;

public class GcodeConverter : Interpreter
{
    private readonly StringBuilder output = new StringBuilder();
    private readonly string targetFormat;
    
    public GcodeConverter(string targetFormat)
    {
        this.targetFormat = targetFormat.ToLower();
    }
    
    public string Convert(string inputGcode)
    {
        output.Clear();
        
        // Add header comment
        output.AppendLine($"; Converted to {targetFormat.ToUpper()} format");
        output.AppendLine($"; Conversion date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine();
        
        Run(inputGcode);
        return output.ToString();
    }
    
    public override void Visit(Gcode code)
    {
        switch (targetFormat)
        {
            case "grbl":
                ConvertToGrbl(code);
                break;
            case "marlin":
                ConvertToMarlin(code);
                break;
            default:
                output.AppendLine(code.ToString());
                break;
        }
    }
    
    public override void Visit(Mcode code)
    {
        switch (targetFormat)
        {
            case "grbl":
                ConvertMcodeToGrbl(code);
                break;
            default:
                output.AppendLine($"M{code.Number}");
                break;
        }
    }
    
    private void ConvertToGrbl(Gcode code)
    {
        // GRBL-specific conversions
        switch (code.Number)
        {
            case 0:  // G00 - Rapid positioning
            case 1:  // G01 - Linear interpolation
                var sb = new StringBuilder($"G{code.Number:00}");
                foreach (var arg in code.Arguments)
                {
                    sb.Append($" {arg.Kind}{arg.Value:F3}");
                }
                output.AppendLine(sb.ToString());
                break;
                
            case 28: // G28 - Return to home
                output.AppendLine("$H"); // GRBL homing command
                break;
                
            default:
                output.AppendLine(code.ToString());
                break;
        }
    }
    
    private void ConvertToMarlin(Gcode code)
    {
        // Marlin-specific conversions (3D printer)
        switch (code.Number)
        {
            case 0:  // G00 -> G01 for 3D printers
                var sb = new StringBuilder("G01");
                foreach (var arg in code.Arguments)
                {
                    if (arg.Kind != ArgumentKind.FeedRate) // Skip F for G00->G01 conversion
                        sb.Append($" {arg.Kind}{arg.Value:F3}");
                }
                sb.Append(" F3000"); // Default rapid feed rate
                output.AppendLine(sb.ToString());
                break;
                
            default:
                output.AppendLine(code.ToString());
                break;
        }
    }
    
    private void ConvertMcodeToGrbl(Mcode code)
    {
        switch (code.Number)
        {
            case 3:  // M03 - Spindle on CW
                output.AppendLine("M3"); // GRBL uses M3 (no leading zero)
                break;
            case 5:  // M05 - Spindle off
                output.AppendLine("M5");
                break;
            default:
                output.AppendLine($"M{code.Number}");
                break;
        }
    }
}

class ConverterExample
{
    static void Main()
    {
        string inputGcode = @"
            G90 G21
            G28 X Y Z
            M03 S1200
            G00 X10 Y10
            G01 Z-5 F300
            G01 X20 F1000
            M05
            M30
        ";

        var grblConverter = new GcodeConverter("grbl");
        var marlinConverter = new GcodeConverter("marlin");
        
        Console.WriteLine("GRBL Output:");
        Console.WriteLine(grblConverter.Convert(inputGcode));
        
        Console.WriteLine("\nMarlin Output:");
        Console.WriteLine(marlinConverter.Convert(inputGcode));
    }
}
```

## Performance Optimization

### Example 13: Optimizing Large File Processing

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gcodes;
using Gcodes.Ast;

class PerformanceOptimizedProcessor
{
    public static void ProcessLargeFile(string filename)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Read file efficiently
        string content = File.ReadAllText(filename);
        Console.WriteLine($"File read: {stopwatch.ElapsedMilliseconds}ms");
        
        // Tokenize
        stopwatch.Restart();
        var lexer = new Lexer(content);
        var tokens = lexer.Tokenize().ToList(); // Materialize tokens once
        Console.WriteLine($"Tokenization: {stopwatch.ElapsedMilliseconds}ms, {tokens.Count} tokens");
        
        // Parse
        stopwatch.Restart();
        var parser = new Parser(tokens);
        var codes = parser.Parse().ToList(); // Materialize codes once
        Console.WriteLine($"Parsing: {stopwatch.ElapsedMilliseconds}ms, {codes.Count} codes");
        
        // Process efficiently
        stopwatch.Restart();
        ProcessCodes(codes);
        Console.WriteLine($"Processing: {stopwatch.ElapsedMilliseconds}ms");
    }
    
    private static void ProcessCodes(List<Code> codes)
    {
        // Use LINQ for efficient processing
        var gcodes = codes.OfType<Gcode>().ToList();
        var mcodes = codes.OfType<Mcode>().ToList();
        
        // Parallel processing for large datasets
        var movementCommands = gcodes.AsParallel()
            .Where(g => g.Number == 0 || g.Number == 1)
            .Select(g => new
            {
                Code = g,
                X = g.ValueFor(ArgumentKind.X),
                Y = g.ValueFor(ArgumentKind.Y),
                Z = g.ValueFor(ArgumentKind.Z)
            })
            .ToList();
        
        Console.WriteLine($"Found {movementCommands.Count} movement commands");
        
        // Efficient memory usage - process in chunks
        const int chunkSize = 1000;
        for (int i = 0; i < codes.Count; i += chunkSize)
        {
            var chunk = codes.Skip(i).Take(chunkSize);
            ProcessChunk(chunk);
        }
    }
    
    private static void ProcessChunk(IEnumerable<Code> chunk)
    {
        // Process chunk without storing intermediate results
        foreach (var code in chunk)
        {
            // Minimal processing per code
        }
    }
}
```

These examples demonstrate the flexibility and power of the Gcodes library. You can combine these patterns to create sophisticated G-code processing applications tailored to your specific needs.

For more advanced topics, see:
- [API Reference](API-Reference) for complete method documentation
- [Architecture Overview](Architecture-Overview) for understanding the library's design
- [Contributing Guide](Contributing-Guide) for extending the library