/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using System.CommandLine;

namespace diVISION.CommandLineX
{
    /// <summary>
    /// Context information collected by the framwork during parsing of the command line arguments.
    /// All bound command action models are passed this in the <c cref="ICommandAction.Invoke(CommandActionContext)">Invoke</c>
    /// or resp. <c cref="ICommandAction.InvokeAsync(CommandActionContext, CancellationToken)">InvokeAsync</c> call.
    /// </summary>
    /// <param name="parseResult"></param>
    /// <param name="unboundSymbols"></param>
    public class CommandActionContext(ParseResult parseResult, IEnumerable<Symbol> unboundSymbols)
    {
        /// <summary>
        /// Result of command line parsing.
        /// </summary>
        public ParseResult ParseResult => parseResult;
        /// <summary>
        /// Collection of symbols that have been defined by the bound <c cref="Command">Command</c> but could not be resolved in the model.
        /// </summary>
        public IEnumerable<Symbol> UnboundSymbols => unboundSymbols;
    }
}
