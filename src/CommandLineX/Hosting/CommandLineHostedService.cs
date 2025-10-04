/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace diVISION.CommandLineX.Hosting
{
    public class CommandLineInvocationContext(string[] args)
    {
        public string[] Args => args;
    }

    public class CommandLineHostedService(ICommandLineInvoker invoker, CommandLineInvocationContext invocationContext
        , IHostApplicationLifetime lifetime, ILogger<CommandLineHostedService> logger) : IHostedService
    {
        protected readonly ICommandLineInvoker _invoker = invoker;
        protected readonly CommandLineInvocationContext _invocationContext = invocationContext;
        protected readonly IHostApplicationLifetime _lifetime = lifetime;
        protected readonly ILogger<CommandLineHostedService> _logger = logger;
        private int _result = -1;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Result = _result;
            if (!cancellationToken.IsCancellationRequested)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Result = await _invoker.InvokeAsync(_invocationContext.Args, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.LogError(e, "Failed to invoke command line '{args}' due to error: {message}", string.Join(" ", _invocationContext.Args), e.Message);
                        }
                        Result = -2;
                    }
                    _lifetime.StopApplication();

                }, cancellationToken);
            }
            return Task.CompletedTask;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        internal int Result
        {
            get { return _result; }
            set
            {
                if (-1 == Interlocked.Exchange(ref _result, value))
                {
                    Environment.ExitCode = _result;
                }
            }
        }
    }
}
