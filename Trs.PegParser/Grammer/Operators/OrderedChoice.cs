﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class OrderedChoice<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly IEnumerable<IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> _choiceSubExpressions;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public OrderedChoice(IEnumerable<IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> choiceSubExpressions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction)
        {
            if (choiceSubExpressions.Count() < 2)
            {
                throw new ArgumentException("Must have at least 2 sub-expressions for Ordered Choice.");
            }

            _choiceSubExpressions = choiceSubExpressions;
            _matchAction = matchAction;
        }

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
        => _choiceSubExpressions.SelectMany(cse => cse.GetNonTerminalNames());

        public ParseResult<TTokenTypeName, TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startIndex)
        {
            ParseResult<TTokenTypeName, TActionResult> lastResult = null;
            foreach (var subExpression in _choiceSubExpressions)
            {
                lastResult = subExpression.Parse(inputTokens, startIndex);
                if (lastResult.Succeed)
                {
                    break;
                }
            }
            if (lastResult == null)
            {
                return ParseResult<TTokenTypeName, TActionResult>.Failed(startIndex);
            }
            return ParseResult<TTokenTypeName, TActionResult>.Succeeded(lastResult.NextParseStartIndex, lastResult.MatchedTokens, 
                _matchAction(lastResult.MatchedTokens, new[] { lastResult.SemanticActionResult }));
        }

        void IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>
            .SetNonTerminalParsingRuleBody(IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            foreach (var subExpression in _choiceSubExpressions)
            {
                subExpression.SetNonTerminalParsingRuleBody(ruleBodies);
            }
        }

        bool IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.HasNonTerminalParsingRuleBodies
            => _choiceSubExpressions.Any(e => e.HasNonTerminalParsingRuleBodies);
    }
}
