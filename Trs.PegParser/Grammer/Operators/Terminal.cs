﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer.Operators
{
    public class Terminal<TTokenTypeName, TNoneTerminalName, TActionResult>
        : IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        private readonly TTokenTypeName _expectedToken;
        private readonly SemanticAction<TActionResult, TTokenTypeName> _matchAction;

        public Terminal(TTokenTypeName expectedToken, SemanticAction<TActionResult, TTokenTypeName> matchAction)
            => (_expectedToken, _matchAction) = (expectedToken, matchAction);

        public IEnumerable<TNoneTerminalName> GetNonTerminalNames()
            => Enumerable.Empty<TNoneTerminalName>();
        
        public ParseResult<TActionResult> Parse(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens,
            int startPosition)
        {
            if (startPosition >= inputTokens.Count 
                || !(inputTokens[startPosition].TokenName.Equals(_expectedToken)))
            {
                return new ParseResult<TActionResult> { Succeed = false };
            }
            else
            {
                TActionResult actionResult = default;
                if (_matchAction != null)
                {
                    actionResult = _matchAction(new MatchRange(startPosition, 1), inputTokens, null);
                }
                return new ParseResult<TActionResult> { 
                    Succeed = true, 
                    NextParsePosition = startPosition + 1,
                    SemanticActionResult = actionResult
                };
            }
        }
    }
}