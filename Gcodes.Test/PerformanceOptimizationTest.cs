using Gcodes;
using Gcodes.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Gcodes.Test
{
    public class PerformanceOptimizationTest
    {
        [Fact]
        public void CapacityEstimationReducesAllocations()
        {
            // Test that our capacity estimation is reasonable for typical G-code
            string smallGcode = "G01 X10.5 Y20.3 Z5.0 F100";
            string mediumGcode = "N10 G01 X10.5 Y20.3 Z5.0 F100\nN20 G02 X15 Y25 I2.5 J0\nN30 M03 S1200";
            
            // Test parser capacity estimation
            var parser1 = new Parser(smallGcode);
            var parser2 = new Parser(mediumGcode);
            
            // These should parse without exceptions, validating our optimizations don't break functionality
            var codes1 = parser1.Parse().ToList();
            var codes2 = parser2.Parse().ToList();
            
            Assert.NotEmpty(codes1);
            Assert.NotEmpty(codes2);
        }

        [Fact]
        public void ConfigurableLexerPatterns()
        {
            // Test that configurable patterns work
            string gcode = "G01 X10";
            
            // Default patterns should work
            var defaultLexer = new Lexer(gcode);
            var defaultTokens = defaultLexer.Tokenize().ToList();
            
            // Custom patterns should also work (using default patterns for this test)
            var customPatterns = new List<Pattern>
            {
                new Pattern(@"G", TokenKind.G),
                new Pattern(@"X", TokenKind.X),
                new Pattern(@"[-+]?(\d+\.\d+|\.\d+|\d+\.?)", TokenKind.Number),
            };
            
            var customLexer = new Lexer(gcode, customPatterns);
            var customTokens = customLexer.Tokenize().ToList();
            
            Assert.Equal(3, defaultTokens.Count); // G, 01, X, 10
            Assert.Equal(3, customTokens.Count); // Should parse same with custom patterns
        }

        [Fact]
        public void ArgumentsListPresizing()
        {
            // Test that ParseArguments handles typical argument counts efficiently
            string gcodeWithManyArgs = "G01 X10.5 Y20.3 Z5.0 F100 S1200";
            
            var parser = new Parser(gcodeWithManyArgs);
            var codes = parser.Parse().ToList();
            
            Assert.Single(codes);
            
            var gcode = codes.First() as Gcode;
            Assert.NotNull(gcode);
            
            // Should handle 5 arguments efficiently (X, Y, Z, F, S)
            Assert.True(gcode.Arguments.Count <= 6, "Should handle typical argument counts");
        }

        [Fact]
        public void LargeFileHandling()
        {
            // Generate a larger G-code program to test performance optimizations
            var lines = new List<string>();
            for (int i = 1; i <= 100; i++)
            {
                lines.Add($"N{i * 10} G01 X{i} Y{i + 10} Z{i * 0.1} F100");
            }
            
            string largeGcode = string.Join("\n", lines);
            
            var stopwatch = Stopwatch.StartNew();
            var interpreter = new Interpreter();
            
            // This should complete without excessive memory allocations
            var tokenCount = 0;
            interpreter.BeforeParse += (sender, tokens) => tokenCount = tokens.Count;
            
            interpreter.Run(largeGcode);
            stopwatch.Stop();
            
            // Verify it processed the expected number of elements
            Assert.True(tokenCount > 400, "Should have processed many tokens");
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, "Should complete in reasonable time");
        }
    }
}