/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace diVISION.CommandLineX.Hosting
{
    public class CommandLineInvoker(RootCommand rootCommand, CommandActionRegistry registry, IServiceProvider serviceProvider) : ICommandLineInvoker
    {
        protected readonly RootCommand _rootCommand = rootCommand;
        protected readonly CommandActionRegistry _registry = registry;
        protected readonly IServiceProvider _serviceProvider = serviceProvider;

        public int Invoke(string[] args)
        {
            _registry.Resolve(_serviceProvider);
            return _rootCommand
                .Parse(args, _serviceProvider.GetService<ParserConfiguration>())
                .Invoke(_serviceProvider.GetService<InvocationConfiguration>());
        }

        public async Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken = default)
        {
            _registry.Resolve(_serviceProvider);
            return await _rootCommand
                .Parse(args, _serviceProvider.GetService<ParserConfiguration>())
                .InvokeAsync(_serviceProvider.GetService<InvocationConfiguration>(), cancellationToken);
        }
    }
}
