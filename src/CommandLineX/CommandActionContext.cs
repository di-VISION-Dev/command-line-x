/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using System.CommandLine;

namespace diVISION.CommandLineX
{
    public class CommandActionContext(ParseResult parseResult, IEnumerable<Symbol> unboundSymbols)
    {
        public ParseResult ParseResult => parseResult;
        public IEnumerable<Symbol> UnboundSymbols => unboundSymbols;
    }
}
