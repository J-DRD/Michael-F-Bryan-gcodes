# API Reference

This page provides comprehensive documentation for all public classes and methods in the Gcodes library.

## Core Namespace: `Gcodes`

### `Lexer` Class

The `Lexer` class converts G-code text into a stream of tokens.

```csharp
public class Lexer
```

#### Constructors

```csharp
public Lexer(string src)
```

Creates a new Lexer instance for the provided source text.

**Parameters:**
- `src` (string): The G-code source text to tokenize

#### Properties

```csharp
internal bool Finished { get; }
```

Gets a value indicating whether the lexer has finished processing all input.

#### Events

```csharp
public event EventHandler<CommentEventArgs> CommentDetected
```

Fired whenever a comment is encountered during tokenization.

#### Methods

```csharp
public IEnumerable<Token> Tokenize()
```

Tokenizes the source text and returns a sequence of tokens.

**Returns:** `IEnumerable<Token>` - A sequence of tokens representing the G-code

**Throws:** 
- `UnrecognisedCharacterException` - When an invalid character is encountered

### `Parser` Class

The `Parser` class converts tokens into structured G-code objects.

```csharp
public class Parser
```

#### Constructors

```csharp
public Parser(List<Token> tokens)
public Parser(string src)
```

Creates a new Parser instance.

**Parameters:**
- `tokens` (List<Token>): Pre-tokenized input
- `src` (string): Source text (will be automatically tokenized)

#### Methods

```csharp
public IEnumerable<Code> Parse()
```

Parses the tokens and returns a sequence of G-code objects.

**Returns:** `IEnumerable<Code>` - Sequence of parsed G-code, M-code, O-code, and T-code objects

```csharp
public bool GetFinished()
```

Checks if the parser has finished processing all tokens.

**Returns:** `bool` - True if parsing is complete

```csharp
public Gcode ParseGCode()
public Mcode ParseMCode()
public Ocode ParseOCode()
public Tcode ParseTCode()
```

Parse specific types of codes from the current position.

**Returns:** The corresponding code object
**Throws:** Various parsing exceptions for malformed input

### `Interpreter` Class

Base class for executing parsed G-code programs.

```csharp
public class Interpreter : IGcodeVisitor
```

#### Methods

```csharp
public void Run(string src)
public void Run(List<Code> codes)
public void Run(List<Token> tokens)
```

Execute a G-code program from various input formats.

**Parameters:**
- `src` (string): G-code source text
- `codes` (List<Code>): Pre-parsed G-code objects
- `tokens` (List<Token>): Pre-tokenized input

#### Events

```csharp
public event EventHandler<List<Token>> BeforeParse
public event EventHandler<List<Code>> BeforeRun
public event EventHandler<CommentEventArgs> CommentDetected
```

Events fired during various stages of interpretation.

#### Virtual Methods

```csharp
public virtual void Visit(Gcode code)
public virtual void Visit(Mcode code)
public virtual void Visit(Ocode code)
public virtual void Visit(Tcode code)
public virtual void Visit(LineNumber lineNumber)
```

Override these methods to handle specific G-code types during execution.

### `FileMap` Class

Provides mapping between character positions and line/column information.

```csharp
public class FileMap
```

#### Constructor

```csharp
public FileMap(string src)
```

#### Methods

```csharp
public SpanInfo? GetSpanInfo(Span span)
```

Gets line/column information for a span.

## AST Namespace: `Gcodes.Ast`

### `Code` Class (Abstract)

Base class for all G-code objects.

```csharp
public abstract class Code
```

#### Properties

```csharp
public Span Span { get; }
public int? Line { get; }
```

#### Methods

```csharp
public abstract void Accept(IGcodeVisitor visitor)
```

### `Gcode` Class

Represents a G-code command (e.g., G01, G02).

```csharp
public class Gcode : Code, IEquatable<Gcode>
```

#### Constructor

```csharp
public Gcode(int number, List<Argument> args, Span span, int? line = null)
```

#### Properties

```csharp
public int Number { get; }
public IReadOnlyList<Argument> Arguments { get; }
```

#### Methods

```csharp
public double? ValueFor(ArgumentKind kind)
public double? ValueFor(params ArgumentKind[] kinds)
```

Gets the value for specific argument types.

**Parameters:**
- `kind` (ArgumentKind): The argument type to search for
- `kinds` (ArgumentKind[]): Multiple argument types (returns first found)

**Returns:** `double?` - The argument value, or null if not found

### `Mcode` Class

Represents an M-code command (e.g., M03, M05).

```csharp
public class Mcode : Code, IEquatable<Mcode>
```

#### Constructor

```csharp
public Mcode(int number, Span span, int? line = null)
```

#### Properties

```csharp
public int Number { get; }
```

### `Ocode` Class

Represents an O-code (program number).

```csharp
public class Ocode : Code, IEquatable<Ocode>
```

#### Constructor

```csharp
public Ocode(int number, Span span, int? line = null)
```

#### Properties

```csharp
public int Number { get; }
```

### `Tcode` Class

Represents a T-code (tool selection).

```csharp
public class Tcode : Code, IEquatable<Tcode>
```

#### Constructor

```csharp
public Tcode(int number, Span span, int? line = null)
```

#### Properties

```csharp
public int Number { get; }
```

### `Argument` Class

Represents a G-code argument (e.g., X10, F1000).

```csharp
public class Argument : IEquatable<Argument>
```

#### Constructor

```csharp
public Argument(ArgumentKind kind, double value, Span span)
```

#### Properties

```csharp
public ArgumentKind Kind { get; }
public double Value { get; }
public Span Span { get; }
```

### `ArgumentKind` Enum

Defines the types of G-code arguments.

```csharp
public enum ArgumentKind
{
    X,           // X coordinate
    Y,           // Y coordinate  
    Z,           // Z coordinate
    FeedRate,    // Feed rate (F)
    I,           // I arc parameter
    J,           // J arc parameter
    K,           // K arc parameter
    A,           // A axis rotation
    B,           // B axis rotation
    C,           // C axis rotation
    P,           // P parameter
    S,           // Spindle speed
    H,           // H parameter
}
```

### `LineNumber` Class

Represents a line number (N-code).

```csharp
public class LineNumber : Code, IEquatable<LineNumber>
```

#### Constructor

```csharp
public LineNumber(int number, Span span)
```

#### Properties

```csharp
public int Number { get; }
```

## Tokens Namespace: `Gcodes.Tokens`

### `Token` Class

Represents a single token from the lexer.

```csharp
public class Token : IEquatable<Token>
```

#### Constructor

```csharp
public Token(TokenKind kind, Span span, string value = "")
```

#### Properties

```csharp
public TokenKind Kind { get; }
public Span Span { get; }
public string Value { get; }
```

### `TokenKind` Enum

Defines the types of tokens.

```csharp
public enum TokenKind
{
    G, M, X, Y, Z, F, N, I, J, A, B, C, H, P, K, O, T, S, Number
}
```

#### Extension Methods

```csharp
public static bool HasValue(this TokenKind kind)
public static ArgumentKind AsArgumentKind(this TokenKind kind)
```

### `Span` Struct

Represents a range of characters in the source text.

```csharp
public struct Span : IEquatable<Span>
```

#### Constructor

```csharp
public Span(int start, int end)
```

#### Properties

```csharp
public int Start { get; }
public int End { get; }
public int Length { get; }
```

## Runtime Namespace: `Gcodes.Runtime`

### `Emulator` Class

Provides simulation capabilities for G-code execution.

```csharp
public class Emulator : Interpreter
```

#### Properties

```csharp
public OperationFactory Operations { get; set; }
public MachineState InitialState { get; set; }
public MachineState State { get; set; }
public double Time { get; set; }
public double MinimumTimeStep { get; set; }
```

#### Events

```csharp
public event EventHandler<StateChangeEventArgs> StateChanged
public event EventHandler<OperationExecutedEventArgs> OperationExecuted
```

### `MachineState` Class

Represents the state of a CNC machine.

```csharp
public class MachineState : IEquatable<MachineState>
```

#### Properties

```csharp
public double X { get; set; }
public double Y { get; set; }
public double Z { get; set; }
public double FeedRate { get; set; }
public double SpindleSpeed { get; set; }
// ... other machine state properties
```

### `OperationFactory` Class

Factory for creating operations from G-codes.

```csharp
public class OperationFactory
```

#### Methods

```csharp
public IOperation CreateOperation(Code code, MachineState state)
public void IgnoreGcode(int gcodeNumber)
```

Register custom operations for specific G-codes.

### `IOperation` Interface

Interface for G-code operations.

```csharp
public interface IOperation
{
    TimeSpan Duration { get; }
    MachineState Execute(MachineState initialState);
}
```

## Exception Classes

### `UnrecognisedCharacterException`

Thrown when the lexer encounters an invalid character.

```csharp
public class UnrecognisedCharacterException : Exception
```

#### Properties

```csharp
public int Line { get; }
public int Column { get; }
public char Character { get; }
```

### `UnexpectedTokenException`

Thrown when the parser encounters an unexpected token.

```csharp
public class UnexpectedTokenException : Exception
```

#### Properties

```csharp
public TokenKind Expected { get; }
public TokenKind Got { get; }
public Span Location { get; }
```

### `MissingNumberException`

Thrown when a number is expected but not found.

```csharp
public class MissingNumberException : Exception
```

#### Properties

```csharp
public Span Location { get; }
```

## Usage Examples

### Complete Parsing Workflow

```csharp
try
{
    // Create lexer and parser
    var lexer = new Lexer(gcodeText);
    lexer.CommentDetected += (s, e) => Console.WriteLine($"Comment: {e.Comment}");
    
    var tokens = lexer.Tokenize().ToList();
    var parser = new Parser(tokens);
    var codes = parser.Parse().ToList();
    
    // Process each code
    foreach (var code in codes)
    {
        switch (code)
        {
            case Gcode g:
                Console.WriteLine($"G{g.Number}: {string.Join(", ", g.Arguments.Select(a => $"{a.Kind}{a.Value}"))}");
                break;
            case Mcode m:
                Console.WriteLine($"M{m.Number}");
                break;
            // ... handle other code types
        }
    }
}
catch (UnrecognisedCharacterException ex)
{
    Console.WriteLine($"Invalid character '{ex.Character}' at line {ex.Line}, column {ex.Column}");
}
catch (UnexpectedTokenException ex)
{
    Console.WriteLine($"Expected {ex.Expected} but got {ex.Got}");
}
```

### Using the Emulator

```csharp
var emulator = new Emulator();
emulator.InitialState = new MachineState { X = 0, Y = 0, Z = 0 };

emulator.StateChanged += (s, e) => 
{
    Console.WriteLine($"Machine moved to X:{e.NewState.X}, Y:{e.NewState.Y}, Z:{e.NewState.Z}");
};

emulator.OperationExecuted += (s, e) =>
{
    Console.WriteLine($"Executed {e.Code} (took {e.Operation.Duration})");
};

emulator.Run(gcodeText);
```

This API reference covers the main public interfaces. For implementation details and private members, refer to the source code or generated API documentation.