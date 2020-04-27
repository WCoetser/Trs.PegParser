﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    /// <summary>
    /// Represents calls to other parse rules in the parser.
    /// </summary>
    /// <typeparam name="TTokenTypeName">Enum identifying token types fed into the parser.</typeparam>
    /// <typeparam name="TNoneTerminalName">Enum type identifying parser rule heads and non-terminals.</typeparam>
    /// <typeparam name="TActionResult">Result of applying semantic actions when tokens are matched.</typeparam>
    public class NonTerminal<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly TNoneTerminalName _noneTerminalName;
        private IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> _ruleBody;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public NonTerminal(TNoneTerminalName noneTerminalName, SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => (_noneTerminalName, _matchAction) = (noneTerminalName, matchAction);

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
            => new[] { _noneTerminalName };
                
        void IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.SetNonTerminalParsingRuleBody(
                IDictionary<TNoneTerminalName, IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>> ruleBodies)
        {
            _ruleBody = ruleBodies[_noneTerminalName];
        }

        bool IParsingOperatorExecution<TTokenTypeName, TNoneTerminalName, TActionResult>.HasNonTerminalParsingRuleBodies
            => _ruleBody != null;

        public ParseResult<TTokenTypeName, TActionResult> Parse([NotNull] IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, int startPosition)
        {
            var parseResult = _ruleBody.Parse(inputTokens, startPosition);
            if (parseResult.Succeed)
            {
                _matchAction(parseResult.MatchedTokens, new[] { parseResult.SemanticActionResult });
            }            
            return parseResult; // this could be succeed or fail
        }
    }
}