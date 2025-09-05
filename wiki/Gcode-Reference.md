# G-code Reference

This page documents the G-code syntax and commands supported by the Gcodes library.

## Overview

G-code (also known as RS-274) is a numerical control programming language used to control automated machine tools, such as CNC machines, 3D printers, and lathes. The Gcodes library can parse and interpret standard G-code commands.

## Basic Syntax

### Command Structure
G-code commands follow this general structure:
```
[Line Number] [G/M/O/T-code] [Arguments] [Comment]
```

**Example:**
```gcode
N10 G01 X10.5 Y20.0 Z5.0 F1000 ; Linear interpolation
```

### Components
- **Line Number (N-code)**: Optional sequence number (e.g., `N10`, `N20`)
- **Command**: The actual G-code, M-code, O-code, or T-code
- **Arguments**: Parameters for the command (X, Y, Z coordinates, feed rates, etc.)
- **Comments**: Explanatory text (`;comment` or `(comment)`)

## Supported Code Types

### G-codes (Preparatory Commands)
G-codes prepare the machine for operation or define how movements should be performed.

#### Motion Commands
| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| G00 | Rapid positioning (rapid traverse) | X, Y, Z, A, B, C | `G00 X10 Y20` |
| G01 | Linear interpolation (feed) | X, Y, Z, A, B, C, F | `G01 X10 Y20 F1000` |
| G02 | Clockwise circular interpolation | X, Y, Z, I, J, K, F | `G02 X20 Y20 I10 J0 F500` |
| G03 | Counter-clockwise circular interpolation | X, Y, Z, I, J, K, F | `G03 X20 Y20 I10 J0 F500` |

#### Positioning Commands
| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| G20 | Programming in inches | - | `G20` |
| G21 | Programming in millimeters | - | `G21` |
| G28 | Return to home position | X, Y, Z | `G28 X0 Y0 Z0` |
| G90 | Absolute programming | - | `G90` |
| G91 | Incremental programming | - | `G91` |
| G92 | Set position register | X, Y, Z | `G92 X0 Y0 Z0` |

#### Feed Rate and Dwell Commands
| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| G04 | Dwell (pause) | P (seconds) or H | `G04 P2.5` |
| G94 | Feed per minute mode | - | `G94` |
| G95 | Feed per revolution mode | - | `G95` |

### M-codes (Miscellaneous Functions)
M-codes control auxiliary functions of the machine.

#### Spindle Control
| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| M03 | Spindle on clockwise | S (speed) | `M03 S1200` |
| M04 | Spindle on counter-clockwise | S (speed) | `M04 S1000` |
| M05 | Spindle stop | - | `M05` |

#### Coolant Control
| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| M07 | Mist coolant on | - | `M07` |
| M08 | Flood coolant on | - | `M08` |
| M09 | Coolant off | - | `M09` |

#### Program Control
| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| M00 | Program stop | - | `M00` |
| M01 | Optional stop | - | `M01` |
| M02 | Program end | - | `M02` |
| M30 | Program end and rewind | - | `M30` |

### O-codes (Program Numbers)
O-codes identify program numbers or subroutines.

| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| O | Program number | Number | `O1234` |

### T-codes (Tool Selection)
T-codes select tools for machining operations.

| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| T | Tool selection | Tool number | `T01`, `T12` |

### N-codes (Line Numbers)
N-codes provide sequence numbers for program lines.

| Code | Description | Arguments | Example |
|------|-------------|-----------|---------|
| N | Line number | Sequence number | `N10`, `N100` |

## Arguments and Parameters

### Coordinate Arguments
| Argument | Description | Type | Example |
|----------|-------------|------|---------|
| X | X-axis coordinate | double | `X10.5` |
| Y | Y-axis coordinate | double | `Y-20.0` |
| Z | Z-axis coordinate | double | `Z5.75` |
| A | A-axis rotation | double | `A90.0` |
| B | B-axis rotation | double | `B45.0` |
| C | C-axis rotation | double | `C180.0` |

### Arc Parameters
| Argument | Description | Type | Example |
|----------|-------------|------|---------|
| I | X-axis arc center offset | double | `I5.0` |
| J | Y-axis arc center offset | double | `J2.5` |
| K | Z-axis arc center offset | double | `K1.0` |

### Feed and Speed Parameters
| Argument | Description | Type | Example |
|----------|-------------|------|---------|
| F | Feed rate | double | `F1000` |
| S | Spindle speed | double | `S2400` |

### Miscellaneous Parameters
| Argument | Description | Type | Example |
|----------|-------------|------|---------|
| P | Dwell time or parameter | double | `P2.5` |
| H | Tool length offset | double | `H01` |

## Comments

The library supports two comment formats:

### Semicolon Comments
```gcode
G01 X10 Y20 ; This is a comment
```

### Parentheses Comments
```gcode
G01 X10 Y20 (This is also a comment)
```

## Number Formats

### Supported Number Formats
- **Integer**: `10`, `100`, `1234`
- **Decimal**: `10.5`, `0.25`, `123.456`
- **Signed**: `+10.5`, `-5.0`
- **Leading Zero**: `G01`, `G00`, `M03`

### Precision
The library uses double-precision floating-point numbers for all numeric values.

## Example Programs

### Basic Linear Movement
```gcode
; Simple 2D square
N10 G90 G21        ; Absolute mode, metric units
N20 G00 X0 Y0      ; Rapid to origin
N30 G01 Z-5 F300   ; Plunge to depth
N40 G01 X10 F1000  ; Move to X10
N50 G01 Y10        ; Move to Y10
N60 G01 X0         ; Move back to X0
N70 G01 Y0         ; Move back to Y0
N80 G00 Z5         ; Retract
N90 M30            ; Program end
```

### Circular Interpolation
```gcode
; Circle with radius 5mm
N10 G90 G21        ; Absolute mode, metric
N20 G00 X5 Y0      ; Position to start
N30 G01 Z-2 F300   ; Plunge
N40 G02 X5 Y0 I-5 J0 F500  ; Full circle
N50 G00 Z5         ; Retract
N60 M30            ; End
```

### Tool Change Example
```gcode
; Tool change sequence
N10 T01 M06        ; Select tool 1
N20 M03 S1200      ; Start spindle
N30 G00 X10 Y10    ; Position
N40 G01 Z-5 F300   ; Machine operation
N50 G00 Z10        ; Retract
N60 M05            ; Stop spindle
N70 T02 M06        ; Change to tool 2
N80 M30            ; End program
```

## Error Handling

### Common Parsing Errors

#### Invalid Characters
```gcode
G01 X10$ Y20  ; Error: '$' is not a valid character
```
**Exception**: `UnrecognisedCharacterException`

#### Missing Numbers
```gcode
G X10 Y20     ; Error: G-code number missing
```
**Exception**: `MissingNumberException`

#### Unexpected Tokens
```gcode
G01 G02 X10   ; Error: Unexpected second G-code
```
**Exception**: `UnexpectedTokenException`

### Best Practices for Error Handling

```csharp
try
{
    var parser = new Parser(gcodeText);
    var codes = parser.Parse().ToList();
}
catch (UnrecognisedCharacterException ex)
{
    Console.WriteLine($"Invalid character '{ex.Character}' at line {ex.Line}, column {ex.Column}");
}
catch (MissingNumberException ex)
{
    var spanInfo = fileMap.GetSpanInfo(ex.Location);
    Console.WriteLine($"Missing number at line {spanInfo?.Start.Line}");
}
```

## Library-Specific Features

### Argument Retrieval
```csharp
var gcode = parser.ParseGCode();

// Get specific argument
double? xPos = gcode.ValueFor(ArgumentKind.X);

// Get first available from multiple options
double? position = gcode.ValueFor(ArgumentKind.X, ArgumentKind.Y, ArgumentKind.Z);

// Check all arguments
foreach (var arg in gcode.Arguments)
{
    Console.WriteLine($"{arg.Kind}: {arg.Value}");
}
```

### Position Information
```csharp
// Get source location for error reporting
var span = gcode.Span;
var spanInfo = fileMap.GetSpanInfo(span);
Console.WriteLine($"G-code at line {spanInfo?.Start.Line}, column {spanInfo?.Start.Column}");
```

### Event Handling
```csharp
var lexer = new Lexer(gcodeText);
lexer.CommentDetected += (sender, args) =>
{
    Console.WriteLine($"Comment: {args.Comment}");
};
```

## Limitations

### Unsupported Features
- **Expressions**: Mathematical expressions in arguments (e.g., `X[10+5]`)
- **Variables**: Variable definitions and usage (e.g., `#1=10`)
- **Subroutines**: Subroutine calls and definitions
- **Conditional Execution**: IF/THEN/ELSE statements
- **Loops**: WHILE and FOR loops

### Parser Limitations
- **Single-line parsing**: Each line is parsed independently
- **No semantic validation**: Parser doesn't validate G-code semantics
- **No machine-specific validation**: Doesn't check machine limits or capabilities

## Extending G-code Support

### Adding Custom Operations
```csharp
public class CustomOperationFactory : OperationFactory
{
    public override IOperation CreateOperation(Code code, MachineState state)
    {
        if (code is Gcode gcode && gcode.Number == 99)
        {
            return new CustomOperation();
        }
        return base.CreateOperation(code, state);
    }
}
```

### Custom Interpreters
```csharp
public class ValidationInterpreter : Interpreter
{
    public override void Visit(Gcode code)
    {
        // Custom validation logic
        if (code.Number > 99)
        {
            throw new InvalidOperationException($"Unsupported G-code: G{code.Number}");
        }
        base.Visit(code);
    }
}
```

This reference covers the G-code syntax supported by the Gcodes library. For implementation details and advanced usage, see the [API Reference](API-Reference) and [Examples and Tutorials](Examples-and-Tutorials).