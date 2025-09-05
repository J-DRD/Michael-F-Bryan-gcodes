# G-Code Parser Performance Optimizations

This document describes the performance optimizations implemented in the G-Code parser library.

## Summary of Optimizations

### 1. **Collection Pre-sizing** (Medium Impact)
**Problem**: Lists were created without initial capacity, causing frequent reallocations during parsing of large files.

**Solution**: Added capacity estimation based on input characteristics:
- **Parser.cs**: Estimates token capacity as `Math.Max(10, src.Length / 4)`
- **ParseArguments()**: Pre-sizes argument list with capacity 4 (typical for G-code commands)  
- **Interpreter.Run()**: Estimates capacity for both token and code collections

**Impact**: Reduces memory allocations and GC pressure for large files.

### 2. **Lexer Configurability** (General Improvement)
**Problem**: Token patterns were hard-coded, making the parser inflexible for different G-code dialects.

**Solution**: Added configurable constructors:
```csharp
// Custom patterns
var lexer = new Lexer(src, customPatterns);
// Custom patterns and skips  
var lexer = new Lexer(src, customPatterns, customSkips);
```

**Impact**: Enables support for custom G-code dialects without modifying library source.

### 3. **Regex Compilation** (Already Implemented)
**Finding**: The high-impact regex optimization was already implemented.
- `Pattern.cs` uses `RegexOptions.Compiled` (line 17)
- All skip regexes also use `RegexOptions.Compiled`

**Impact**: Pre-compiled regexes provide significant performance benefits for tokenization.

## Performance Characteristics

### Before Optimizations
- List reallocations: O(log n) for each collection
- Memory overhead: Exponential growth with frequent allocations
- Inflexible pattern matching

### After Optimizations  
- List reallocations: Mostly eliminated with good capacity estimation
- Memory overhead: Linear growth with pre-sized collections
- Configurable pattern matching for different G-code dialects

## Usage Examples

### Basic Usage (No Changes Required)
```csharp
var parser = new Parser(gcodeString);
var codes = parser.Parse().ToList();
```

### Custom Patterns
```csharp
var customPatterns = new List<Pattern>
{
    new Pattern(@"G", TokenKind.G),
    new Pattern(@"CUSTOM", TokenKind.Custom),
    // ... more patterns
};

var lexer = new Lexer(gcode, customPatterns);
var tokens = lexer.Tokenize().ToList();
```

### Performance Testing
Use the included `PerformanceOptimizationTest` class to validate optimizations:
- Tests capacity estimation effectiveness
- Validates configurable patterns
- Benchmarks large file processing

## Future Optimizations

For maximum performance gains, consider:
1. **Mega-Regex Approach**: Combine all patterns into single regex with named capture groups
2. **Token Struct Conversion**: Convert Token from class to readonly struct (requires extensive changes)
3. **Streaming Parser**: Use `yield return` for very large files to reduce memory usage

## Compatibility

All optimizations maintain backward compatibility:
- Existing code continues to work unchanged
- Default behavior remains identical
- Performance improvements are automatic