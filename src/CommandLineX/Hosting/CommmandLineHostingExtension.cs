/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using diVISION.CommandLineX.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

namespace diVISION.CommandLineX.Hosting
{
    public static class CommmandLineHostingExtension
    {
        public static IHostBuilder UseCommandLine(this IHostBuilder builder, RootCommand rootCommand, Action<CommandLineOptions>? configure = null)
        {
            var registry = new CommandActionRegistry();
            builder.Properties[typeof(CommandActionRegistry)] = registry;

            var options = new CommandLineOptions();
            configure?.Invoke(options);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ICommandLineInvoker>(serviceProvider =>
                {
                    return new CommandLineInvoker(rootCommand, registry, serviceProvider);
                });
                if (null != options.ParserConfiguration)
                {
                    services.AddSingleton(options.ParserConfiguration);
                }
                if (null != options.InvocationConfiguration)
                {
                    services.AddSingleton(options.InvocationConfiguration);
                }
            });
            return builder;
        }

        public static IHostBuilder UseHostedCommandInvocation(this IHostBuilder builder, string[] args)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(new CommandLineInvocationContext(args));
                services.AddHostedService<CommandLineHostedService>();
            });
            return builder;
        }

        public static IHostBuilder UseCommandWithAction<TAction>(this IHostBuilder builder, Command parent, Command command, bool runAsync = true)
            where TAction : class, ICommandAction
        {
            if (!builder.Properties.TryGetValue(typeof(CommandActionRegistry), out var propVal) || propVal is not CommandActionRegistry registry)
            {
                throw new InvalidOperationException("Registry is missing, call UseCommandLine first");
            }
            if (null != command.Action || runAsync && registry.HasActionType<AsyncBindingCommandLineAction<TAction>>() || !runAsync && registry.HasActionType<SyncBindingCommandLineAction<TAction>>())
            {
                throw new InvalidOperationException($"Action of type {typeof(TAction)} is already used");
            }

            parent.Add(command);
            if (runAsync)
            {
                registry.RegisterActionType<AsyncBindingCommandLineAction<TAction>>();
            }
            else
            {
                registry.RegisterActionType<SyncBindingCommandLineAction<TAction>>();
            }

            builder.ConfigureServices((ctx, services) =>
            {
                services.AddTransient<TAction>();
                if (runAsync)
                {
                    services.AddSingleton(serviceProvider =>
                    {
                        return new AsyncBindingCommandLineAction<TAction>(command, serviceProvider.GetRequiredService<TAction>);
                    });
                }
                else
                {
                    services.AddSingleton(serviceProvider =>
                    {
                        return new SyncBindingCommandLineAction<TAction>(command, serviceProvider.GetRequiredService<TAction>);
                    });
                }
            });
            return builder;
        }

        public static int RunCommandLine(this IHost host, string[] args)
        {
            host.ThrowIfHostedCommandLine();
            host.Start();
            var invoker = host.Services.GetRequiredService<ICommandLineInvoker>();
            return invoker.Invoke(args);
        }

        public static async Task<int> RunCommandLineAsync(this IHost host, string[] args, CancellationToken cancellationToken = default)
        {
            host.ThrowIfHostedCommandLine();
            await host.StartAsync(cancellationToken);
            var invoker = host.Services.GetRequiredService<ICommandLineInvoker>();
            return await invoker.InvokeAsync(args, cancellationToken);
        }

        public static int RunCommandLineHosted(this IHost host)
        {
            host.CheckInvocationContext();

            var service = host.Services.GetRequiredService<IHostedService>();
            host.Run();
            if (service is CommandLineHostedService hostedService)
            {
                return hostedService.Result;
            }
            return Environment.ExitCode;
        }

        public static async Task<int> RunCommandLineHostedAsync(this IHost host, CancellationToken cancellationToken = default)
        {
            host.CheckInvocationContext();

            var service = host.Services.GetRequiredService<IHostedService>();
            await host.StartAsync(cancellationToken);
            await host.WaitForShutdownAsync(cancellationToken);
            if (service is CommandLineHostedService hostedService)
            {
                return hostedService.Result;
            }
            return Environment.ExitCode;
        }

        public static void CheckInvocationContext(this IHost host)
        {
            if (null == host.Services.GetService<CommandLineInvocationContext>())
            {
                throw new InvalidOperationException("Invocation context is missing, did you call IHostBuilder.UseHostedCommandInvocation?");
            }
        }

        public static void ThrowIfHostedCommandLine(this IHost host)
        {
            if (host.Services.GetService<IHostedService>() is CommandLineHostedService)
            {
                throw new InvalidOperationException("This host is configured to run commands in the hosted service, use RunCommandsHosted* methods");
            }

        }
    }
}
