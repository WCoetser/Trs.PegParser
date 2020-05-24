﻿using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class OptionalTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private TokensMatch<TokenNames> matchedTokenRangeAssert = null;
        private IEnumerable<string> subActionResults = null;

        public OptionalTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.SetTerminalAction(TokenNames.A, (tokens, _) => tokens.GetMatchedString());
            semanticActions.OptionalAction = (matchedTokenRange, subResults) =>
            {
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults;
                return matchedTokenRange.GetMatchedString();
            };
        }

        [Fact]
        public void ShouldParseEmptyCase()
        {
            // Arrange
            string testInput = string.Empty;
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var parseRules = peg.ParserGenerator.GetParsingRules("Start => [A]?");
            var parser = peg.Parser(ParsingRuleNames.Start, parseRules);

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Empty(subActionResults);            
            Assert.Equal(new MatchRange(0, 0), matchedTokenRangeAssert.MatchedIndices);
            Assert.Equal(testInput, parseResult.SemanticActionResult);
            Assert.Equal(0, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldParseNonEmptyCase()
        {
            // Arrange
            string testInput = "aaaa";
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var parseRules = peg.ParserGenerator.GetParsingRules("Start => [A]?");
            var parser = peg.Parser(ParsingRuleNames.Start, parseRules);

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Single(subActionResults);
            Assert.Equal(testInput, subActionResults.Single());
            Assert.Equal(new MatchRange(0, 1), matchedTokenRangeAssert.MatchedIndices);
            Assert.Equal(testInput, parseResult.SemanticActionResult);
            Assert.Equal(1, parseResult.NextParseStartIndex);
        }
    }
}
