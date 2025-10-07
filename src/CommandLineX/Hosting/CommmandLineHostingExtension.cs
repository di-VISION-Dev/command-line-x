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
    /// <summary>
    /// <see cref="https://github.com/dotnet/command-line-api"><c>System.CommandLine</c></see> integration with <c>IHostBuilder</c> and <c>IHost</c>.
    /// </summary>
    public static class CommmandLineHostingExtension
    {
        /// <summary>
        /// Registers basic services neccessary to invoke commands from within a host.
        /// If <paramref name="configure"/> action is provided, it is called with <c>CommandLineOptions</c> instance
        /// where configuration for <c>System.CommandLine</c> can be set up. Call this method before any other extension is used.
        /// </summary>
        /// <seealso cref="CommandLineOptions"/>
        /// <param name="builder">host builder instance being extended</param>
        /// <param name="rootCommand" cref="RootCommand">root command to be invoked by host</param>
        /// <param name="configure">optional configure action</param>
        /// <returns><paramref name="builder"/></returns>
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

        /// <summary>
        /// Registers <c cref="CommandLineInvocationContext">CommandLineInvocationContext</c> and <c cref="CommandLineHostedService">CommandLineHostedService</c>
        /// so that commands specified by <paramref name="args"/> will be parsed and invoked asynchronously at the host startup.
        /// </summary>
        /// <param name="builder">host builder instance being extended</param>
        /// <param name="args">command line arguments to parse</param>
        /// <returns><paramref name="builder"/></returns>
        public static IHostBuilder UseHostedCommandInvocation(this IHostBuilder builder, string[] args)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(new CommandLineInvocationContext(args));
                services.AddHostedService<CommandLineHostedService>();
            });
            return builder;
        }

        /// <summary>
        /// Adds specified <paramref name="command"/> to <paramref name="parent"/> command (ususally <c>RootCommand</c>) and binds <typeparamref name="TAction"/> model to the former.
        /// Requires comand line services to be set up by <c cref="UseCommandLine(IHostBuilder, RootCommand, Action{CommandLineOptions}?)">UseCommandLine</c> prior to this call.
        /// </summary>
        /// <seealso cref="UseCommandLine(IHostBuilder, RootCommand, Action{CommandLineOptions}?)"/>
        /// <typeparam name="TAction">action to bind</typeparam>
        /// <param name="builder">host builder instance being extended</param>
        /// <param name="parent">parent <c cref="Command">Command</c> to which <paramref name="command"/> will be added</param>
        /// <param name="command"><c cref="Command">Command</c> used for binding</param>
        /// <param name="runAsync">indicates whether <typeparamref name="TAction"/>.<c cref="ICommandAction.Invoke(CommandActionContext)">Invoke</c>
        /// or <typeparamref name="TAction"/>.<c cref="ICommandAction.InvokeAsync(CommandActionContext, CancellationToken)">InvokeAsync</c> is used to execute the action</param>
        /// <returns><paramref name="builder"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Starts the <paramref name="host"/>, parses command line <paramref name="args"/> and executes matching <c cref="ICommandAction.Invoke(CommandActionContext)">ICommandAction.Invoke</c>
        /// previously bound by <c cref="UseCommandWithAction{TAction}(IHostBuilder, Command, Command, bool)">UseCommandWithAction</c>. If no match exists an error message is displayed and an error code is returned.
        /// </summary>
        /// <seealso cref="UseCommandWithAction{TAction}(IHostBuilder, Command, Command, bool)"/>
        /// <param name="host">host instance being extended</param>
        /// <param name="args">command line arguments to parse</param>
        /// <returns>either result of <c cref="ICommandAction.Invoke(CommandActionContext)">ICommandAction.Invoke</c> or an error code</returns>
        public static int RunCommandLine(this IHost host, string[] args)
        {
            host.ThrowIfHostedCommandLine();
            host.Start();
            var invoker = host.Services.GetRequiredService<ICommandLineInvoker>();
            return invoker.Invoke(args);
        }

        /// <summary>
        /// Starts the <paramref name="host"/>, parses command line <paramref name="args"/> and executes matching <c cref="ICommandAction.InvokeAsync(CommandActionContext, CancellationToken)">ICommandAction.InvokeAsync</c>
        /// previously bound by <c cref="UseCommandWithAction{TAction}(IHostBuilder, Command, Command, bool)">UseCommandWithAction</c>. If no match exists an error message is displayed and an error code is returned.
        /// </summary>
        /// <seealso cref="UseCommandWithAction{TAction}(IHostBuilder, Command, Command, bool)"/>
        /// <param name="host">host instance being extended</param>
        /// <param name="args">command line arguments to parse</param>
        /// <param name="cancellationToken">cancellation token passed to <c cref="ICommandAction.InvokeAsync(CommandActionContext, CancellationToken)">ICommandAction.InvokeAsync</c></param>
        /// <returns>either result of <c cref="ICommandAction.InvokeAsync(CommandActionContext, CancellationToken)">ICommandAction.InvokeAsync</c> or an error code</returns>
        public static async Task<int> RunCommandLineAsync(this IHost host, string[] args, CancellationToken cancellationToken = default)
        {
            host.ThrowIfHostedCommandLine();
            await host.StartAsync(cancellationToken);
            var invoker = host.Services.GetRequiredService<ICommandLineInvoker>();
            return await invoker.InvokeAsync(args, cancellationToken);
        }

        /// <summary>
        /// Starts the <paramref name="host"/>, during the startup the <c cref="CommandLineHostedService">CommandLineHostedService</c>
        /// (registered by <c cref="UseHostedCommandInvocation(IHostBuilder, string[])">UseHostedCommandInvocation</c>)
        /// parses the command line and executes matching <c cref="ICommandAction.Invoke(CommandActionContext)">ICommandAction.Invoke</c>
        /// previously bound by <c cref="UseCommandWithAction{TAction}(IHostBuilder, Command, Command, bool)">UseCommandWithAction</c>. If no match exists an error message is displayed and an error code is returned.
        /// </summary>
        /// <seealso cref="UseHostedCommandInvocation(IHostBuilder, string[])"/>
        /// <param name="host">host instance being extended</param>
        /// <returns>either result of <c cref="ICommandAction.Invoke(CommandActionContext)">ICommandAction.Invoke</c> or an error code</returns>
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

        /// <summary>
        /// Starts the <paramref name="host"/>, during the startup the <c cref="CommandLineHostedService">CommandLineHostedService</c>
        /// (registered by <c cref="UseHostedCommandInvocation(IHostBuilder, string[])">UseHostedCommandInvocation</c>)
        /// parses the command line and executes matching <c cref="ICommandAction.InvokeAsync(CommandActionContext, CancellationToken)">ICommandAction.InvokeAsync</c>
        /// previously bound by <c cref="UseCommandWithAction{TAction}(IHostBuilder, Command, Command, bool)">UseCommandWithAction</c>. If no match exists an error message is displayed and an error code is returned.
        /// </summary>
        /// <seealso cref="UseHostedCommandInvocation(IHostBuilder, string[])"/>
        /// <param name="host">host instance being extended</param>
        /// <param name="cancellationToken">cancellation token passed to the <paramref name="host"/></param>
        /// <returns>either result of <c cref="ICommandAction.InvokeAsync(CommandActionContext, CancellationToken)">ICommandAction.InvokeAsync</c> or an error code</returns>
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

        /// <summary>
        /// For internal use.
        /// </summary>
        /// <param name="host">host instance being extended</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void CheckInvocationContext(this IHost host)
        {
            if (null == host.Services.GetService<CommandLineInvocationContext>())
            {
                throw new InvalidOperationException("Invocation context is missing, did you call IHostBuilder.UseHostedCommandInvocation?");
            }
        }

        /// <summary>
        /// For internal use.
        /// </summary>
        /// <param name="host">host instance being extended</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ThrowIfHostedCommandLine(this IHost host)
        {
            if (host.Services.GetService<IHostedService>() is CommandLineHostedService)
            {
                throw new InvalidOperationException("This host is configured to run commands in the hosted service, use RunCommandsHosted* methods");
            }

        }
    }
}
