/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using System.CommandLine;

namespace diVISION.CommandLineX
{
    /// <summary>
    /// Container with configuration options for <c cref="System.CommandLine">System.CommandLine</c>.
    /// An instance of this class initialized with the defaults is passed to the configure action by
    /// <c cref="diVISION.CommandLineX.Hosting.CommmandLineHostingExtension.UseCommandLine(Microsoft.Extensions.Hosting.IHostBuilder, RootCommand, Action{CommandLineOptions}?)">CommmandLineHostingExtension.UseCommandLine</c>,
    /// the action then can modify the configuration as required.
    /// </summary>
    public class CommandLineOptions
    {
        public ParserConfiguration? ParserConfiguration { get; set; }
        public InvocationConfiguration? InvocationConfiguration { get; set; }
    }
}
