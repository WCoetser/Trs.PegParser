﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class SequenceTest
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private TokensMatch<TokenNames> matchedTokenRangeAssert = null;
        private List<string> subActionResults = null;

        public SequenceTest()
        {
            var semanticActions = peg.DefaultSemanticActions;
            var extractValueTerminal = semanticActions.SemanticAction((matchedTokenRange, _) => matchedTokenRange.GetMatchedString());
            semanticActions.SetTerminalAction(TokenNames.A, extractValueTerminal);
            semanticActions.SetTerminalAction(TokenNames.B, extractValueTerminal);
            semanticActions.SequenceAction = (matchedTokenRange, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults.ToList();
                return matchedTokenRange.GetMatchedString();
            };
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            var inputString = "aaabbb";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var op = peg.Operators;
            var parser = peg.Parser(ParsingRuleNames.ConcatenationTest, new[] {
                peg.Rule(ParsingRuleNames.ConcatenationTest, op.Sequence(op.Terminal(TokenNames.A), op.Terminal(TokenNames.B)))
            });

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens);

            // Assert
            Assert.Equal(new MatchRange(0, 2), matchedTokenRangeAssert.MatchedIndices);
            Assert.Equal(2, subActionResults.Count);
            Assert.Equal("aaa", subActionResults[0]);
            Assert.Equal("bbb", subActionResults[1]);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(2, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldReturnNonTerminalNamesForValidationChecks()
        {
            // Arrange
            var op = peg.Operators;
            var seq = op.Sequence(op.NonTerminal(ParsingRuleNames.Head), op.NonTerminal(ParsingRuleNames.Tail));

            // Act
            var nonTerminals = seq.GetNonTerminalNames();

            // Assert
            var testSet = new HashSet<ParsingRuleNames>(new[] { ParsingRuleNames.Head, ParsingRuleNames.Tail });
            Assert.True(testSet.SetEquals(nonTerminals));
        }
    }
}
