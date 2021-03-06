﻿using System;
using System.Linq;
using Trl.PegParser.Grammer;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class OrderedChoiceTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private TokensMatch<TokenNames> matchedTokens;

        public OrderedChoiceTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.OrderedChoiceAction = (matchedTokens, subResults, matchedPeg) => {
                this.matchedTokens = matchedTokens;
                return subResults.First();
            };
        }

        [Fact]
        public void ShouldParseWithSecondExpression()
        {
            // Arrange
            string input = "bbb";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            peg.DefaultSemanticActions.SetTerminalAction(TokenNames.B, (m, _, matchedPeg) => m.GetMatchedString());
            var rules = peg.ParserGenerator.GetParsingRules("Start => [A] | [B]");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokens = tokenizer.Tokenize(input);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(input, parseResult.SemanticActionResult);
            Assert.Equal(new MatchRange(0, 1), matchedTokens.MatchedIndices);
            Assert.Equal(1, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldParseWithFirstExpression()
        {
            // Arrange
            string input = "aaa";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            peg.DefaultSemanticActions.SetTerminalAction(TokenNames.A, (m, _, matchedPeg) => m.GetMatchedString());
            var rules = peg.ParserGenerator.GetParsingRules("Start => [A] | [B]");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokens = tokenizer.Tokenize(input);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(input, parseResult.SemanticActionResult);
            Assert.Equal(new MatchRange(0, 1), matchedTokens.MatchedIndices);
            Assert.Equal(1, parseResult.NextParseStartIndex);
        }
    }
}
