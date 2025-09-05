# Architecture Overview

This document provides a deep dive into the design and architecture of the Gcodes library, explaining how the components work together to parse, interpret, and simulate G-code programs.

## High-Level Architecture

The Gcodes library follows a classic compiler architecture with additional simulation capabilities:

```
G-code Text
     ↓
┌─────────────┐     ┌────────────┐     ┌──────────────┐
│   Lexer     │ →   │   Parser   │ →   │ Interpreter  │
│ (Tokenizer) │     │            │     │              │
└─────────────┘     └────────────┘     └──────────────┘
     ↓                     ↓                    ↓
  Tokens               AST Nodes           Execution
                                               ↓
                                      ┌──────────────┐
                                      │  Emulator    │
                                      │ (Simulation) │
                                      └──────────────┘
```

## Core Components

### 1. Lexical Analysis Layer

#### `Lexer` Class
- **Purpose**: Converts raw G-code text into a stream of tokens
- **Input**: Raw string containing G-code
- **Output**: Stream of `Token` objects
- **Responsibilities**:
  - Character-by-character parsing
  - Comment detection and extraction
  - Whitespace handling
  - Error reporting for invalid characters

#### Token System
- **`Token`**: Represents a single lexical unit (G, M, X, Y, numbers, etc.)
- **`TokenKind`**: Enumeration of all possible token types
- **`Span`**: Tracks position information for error reporting

```csharp
// Example tokenization
"G01 X10 Y20" → [G, Number(01), X, Number(10), Y, Number(20)]
```

#### Pattern Matching
The lexer uses regex patterns to identify tokens:

```csharp
private List<Pattern> patterns = new List<Pattern>
{
    new Pattern(@"G", TokenKind.G),
    new Pattern(@"M", TokenKind.M),
    new Pattern(@"[+-]?\d*\.?\d+", TokenKind.Number),
    // ... more patterns
};
```

### 2. Parsing Layer

#### `Parser` Class
- **Purpose**: Converts tokens into structured Abstract Syntax Tree (AST) nodes
- **Input**: Stream of `Token` objects
- **Output**: Stream of `Code` objects (Gcode, Mcode, etc.)
- **Parsing Strategy**: Recursive descent parser

#### AST Design
The AST follows the Visitor pattern for extensible processing:

```csharp
public abstract class Code
{
    public abstract void Accept(IGcodeVisitor visitor);
}

public class Gcode : Code
{
    public int Number { get; }
    public IReadOnlyList<Argument> Arguments { get; }
}
```

#### Parsing Process
1. **Token Consumption**: Parser consumes tokens sequentially
2. **Syntax Validation**: Ensures proper G-code syntax
3. **AST Construction**: Creates appropriate AST nodes
4. **Error Handling**: Throws specific exceptions for malformed input

### 3. Interpretation Layer

#### `Interpreter` Class
- **Purpose**: Base class for executing parsed G-code programs
- **Pattern**: Visitor pattern implementation
- **Extensibility**: Virtual methods for custom behavior

#### Visitor Pattern Implementation
```csharp
public class Interpreter : IGcodeVisitor
{
    public virtual void Visit(Gcode code) { /* Default behavior */ }
    public virtual void Visit(Mcode code) { /* Default behavior */ }
    public virtual void Visit(Ocode code) { /* Default behavior */ }
    // ... more visit methods
}
```

### 4. Simulation Layer (Runtime)

#### `Emulator` Class
- **Purpose**: Simulates CNC machine behavior
- **Inheritance**: Extends `Interpreter`
- **State Management**: Tracks machine state over time
- **Event System**: Provides hooks for monitoring execution

#### Machine State Management
```csharp
public class MachineState
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double FeedRate { get; set; }
    public double SpindleSpeed { get; set; }
    // ... other state properties
}
```

#### Operation System
- **`IOperation`**: Interface for executable operations
- **`OperationFactory`**: Creates operations from G-codes
- **Extensibility**: Custom operations can be registered

## Design Patterns

### 1. Visitor Pattern
Used throughout the AST for type-safe operations:

```csharp
// Each AST node accepts visitors
public override void Accept(IGcodeVisitor visitor)
{
    visitor.Visit(this);
}

// Visitors implement specific behaviors
public class MyCustomInterpreter : Interpreter
{
    public override void Visit(Gcode code)
    {
        // Custom G-code handling
    }
}
```

### 2. Factory Pattern
`OperationFactory` creates appropriate operations for G-codes:

```csharp
public class OperationFactory
{
    private Dictionary<int, Func<Gcode, MachineState, IOperation>> gcodeOperations;
    
    public IOperation CreateOperation(Code code, MachineState state)
    {
        // Factory logic to create appropriate operation
    }
}
```

### 3. Observer Pattern
Event-driven architecture for monitoring:

```csharp
public event EventHandler<StateChangeEventArgs> StateChanged;
public event EventHandler<OperationExecutedEventArgs> OperationExecuted;
public event EventHandler<CommentEventArgs> CommentDetected;
```

## Data Flow

### 1. Parsing Flow
```
Raw Text → Lexer → Tokens → Parser → AST Nodes
```

**Example:**
```
"G01 X10 Y20 F1000"
     ↓ Lexer
[G, Number(01), X, Number(10), Y, Number(20), F, Number(1000)]
     ↓ Parser
Gcode(1, [Argument(X, 10), Argument(Y, 20), Argument(FeedRate, 1000)])
```

### 2. Execution Flow
```
AST Nodes → Interpreter.Visit() → Operation Creation → Machine State Update
```

### 3. Simulation Flow
```
G-code → Operation → State Transition → Events → Logging/Monitoring
```

## Error Handling Architecture

### Exception Hierarchy
```
Exception
├── UnrecognisedCharacterException  // Lexer errors
├── UnexpectedTokenException        // Parser errors
├── MissingNumberException         // Parser errors
└── ... (other specific exceptions)
```

### Error Context
All exceptions include position information:
- Line numbers
- Column positions
- Character spans
- Context information

### Recovery Strategies
- **Lexer**: Continues after skipping invalid characters
- **Parser**: Throws specific exceptions with context
- **Interpreter**: Configurable error handling

## Extension Points

### 1. Custom Operations
Implement `IOperation` for custom G-code behaviors:

```csharp
public class CustomDrillOperation : IOperation
{
    public TimeSpan Duration { get; }
    
    public MachineState Execute(MachineState initialState)
    {
        // Custom drilling logic
        return newState;
    }
}
```

### 2. Custom Interpreters
Extend `Interpreter` for specialized behavior:

```csharp
public class ValidationInterpreter : Interpreter
{
    public override void Visit(Gcode code)
    {
        // Validate G-code without execution
        ValidateGcode(code);
    }
}
```

### 3. Custom Lexer Patterns
Extend the lexer for non-standard G-code dialects:

```csharp
var lexer = new Lexer(source);
// Add custom patterns if needed
```

## Performance Considerations

### Memory Management
- **Immutable AST**: AST nodes are immutable for thread safety
- **Lazy Enumeration**: Parsing uses `IEnumerable<T>` for memory efficiency
- **Span Usage**: `Span` structs minimize allocation overhead

### Parsing Performance
- **Single Pass**: Most parsing is done in a single pass
- **Compiled Regex**: Regular expressions are pre-compiled
- **Token Caching**: Tokens can be cached for repeated parsing

### Simulation Performance
- **State Copying**: Machine state is copied, not mutated
- **Event Filtering**: Events can be selectively enabled
- **Time Management**: Configurable time step granularity

## Thread Safety

### Immutable Components
- All AST nodes are immutable after construction
- Tokens and spans are immutable value types
- Parser creates new objects, doesn't modify input

### Mutable Components
- `Emulator` state is mutable and not thread-safe
- `OperationFactory` registration is not thread-safe
- File mapping is mutable during construction

### Best Practices
- Create separate parser instances per thread
- Don't share `Emulator` instances across threads
- Use separate `OperationFactory` instances if registering custom operations

## Testing Architecture

### Unit Test Structure
```
Gcodes.Test/
├── LexerTest.cs        // Lexer unit tests
├── ParserTest.cs       // Parser unit tests
├── Runtime/            // Simulation tests
│   ├── EmulatorTest.cs
│   └── OperationTest.cs
├── Fixtures/           // Test G-code files
└── Integration/        // End-to-end tests
```

### Test Categories
1. **Unit Tests**: Individual component testing
2. **Integration Tests**: Full pipeline testing
3. **Performance Tests**: Benchmarking
4. **Fixture Tests**: Real-world G-code file testing

## Future Extensibility

### Planned Extension Points
1. **Plugin System**: Dynamic operation loading
2. **Configuration System**: Runtime behavior configuration
3. **Multiple G-code Dialects**: Support for different CNC systems
4. **Optimization Passes**: AST optimization for performance

### API Stability
- **Core Interfaces**: Stable public API
- **Extension Points**: Clearly marked extension interfaces
- **Internal APIs**: Subject to change between versions

## Dependencies

### External Dependencies
- **System Libraries**: Only standard .NET Framework libraries
- **No Third-Party**: Zero external dependencies for core library
- **Testing**: xUnit for unit testing
- **Console App**: CommandLine, Serilog for logging

### Internal Dependencies
```
Gcodes (Core)
├── Gcodes.Console (depends on Core + external libs)
└── Gcodes.Test (depends on Core + xUnit)
```

This architecture provides a clean separation of concerns, extensibility for custom behaviors, and robust error handling while maintaining good performance characteristics.