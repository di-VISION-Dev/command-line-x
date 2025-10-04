/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using System.CommandLine;

namespace diVISION.CommandLineX
{
    public class CommandLineOptions
    {
        public ParserConfiguration? ParserConfiguration { get; set; }
        public InvocationConfiguration? InvocationConfiguration { get; set; }
    }
}
