using diVISION.CommandLineX.Binding;
using diVISION.CommandLineX.Hosting;
using diVISION.CommandLineX.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace diVISION.CommandLineX.Tests;

[TestClass]
public class CommandLineHostedServiceTest
{
    internal class IntModifierService : INumberModifierService<int>
    {
        public int Modify(int num)
        {
            return num * 2;
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class ServiceProviderMock : IServiceProvider
    {
        public object? GetService(Type serviceType) => serviceType switch
        {
            var type when type == typeof(InvocationConfiguration) => new InvocationConfiguration() { EnableDefaultExceptionHandler = false },
            var type when type == typeof(INumberModifierService<int>) => new IntModifierService(),
            _ => TryGeneric(serviceType)
        };

        private static object? TryGeneric(Type serviceType)
        {
            if (serviceType.IsGenericType && serviceType.IsAssignableTo(typeof(ILogger)))
            {
                var loggerType = typeof(LoggerMock<>).MakeGenericType(serviceType.GetGenericArguments());
                return Activator.CreateInstance(loggerType);
            }
            return null;
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class HostApplicationLifetimeMock : IHostApplicationLifetime
    {
        private readonly TaskCompletionSource<bool> _source = new();

        public CancellationToken ApplicationStarted => throw new NotImplementedException();

        public CancellationToken ApplicationStopping => throw new NotImplementedException();

        public CancellationToken ApplicationStopped => throw new NotImplementedException();

        public void StopApplication()
        {
            _source.TrySetResult(true);
        }

        public Task<bool> WaitForStopAsync(CancellationToken cancellationToken = default)
        {
            Task.Run(async () =>
            {
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
                _source.TrySetCanceled(cancellationToken);
            }, cancellationToken);
            return _source.Task;
        }
    }

    private readonly ServiceProviderMock _serviceProvider = new();
    private readonly CommandActionRegistry _registry = new();
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task StartAsync_setting_ExitCode_to_result_of_NoArgsCommandAction_modified_by_IntModifierService_given_any_Command()
    {
        var command = new Command("noarg");
        var invoker = SetupInvoker<NoArgsCommandAction>(command, _serviceProvider.GetRequiredService<INumberModifierService<int>>());
        var lifetime = new HostApplicationLifetimeMock();
        var service = new CommandLineHostedService(invoker, new CommandLineInvocationContext(["noarg"]), lifetime
            , _serviceProvider.GetRequiredService<ILogger<CommandLineHostedService>>());

        await service.StartAsync(TestContext.CancellationToken);
        var finished = await lifetime.WaitForStopAsync(TestContext.CancellationToken);
        finished.Should().BeTrue();
        service.Result.Should().Be(84);
    }

    [TestMethod]
    public async Task StartAsync_setting_ExitCode_to_result_of_OneIntArgCommandAction_given_Command_with_int_argument()
    {
        var command = new Command("onearg")
        {
            new Argument<int>("answer")
        };
        var invoker = SetupInvoker<OneIntArgCommandAction>(command);
        var lifetime = new HostApplicationLifetimeMock();
        var service = new CommandLineHostedService(invoker, new CommandLineInvocationContext(["onearg", "42"]), lifetime
            , _serviceProvider.GetRequiredService<ILogger<CommandLineHostedService>>());

        await service.StartAsync(TestContext.CancellationToken);
        var finished = await lifetime.WaitForStopAsync(TestContext.CancellationToken);
        finished.Should().BeTrue();
        service.Result.Should().Be(42);
    }

    [TestMethod]
    public async Task StartAsync_logging_Exception_thrown_by_OneStringOptionCommandAction_given_Command_with_invalid_option()
    {
        var logger = new LoggerMock<CommandLineHostedService>();
        var command = new Command("oneopt")
        {
            new Option<string>("-o", ["--the-option"])
        };
        var invoker = SetupInvoker<OneStringOptionCommandAction>(command);
        var lifetime = new HostApplicationLifetimeMock();
        var service = new CommandLineHostedService(invoker, new CommandLineInvocationContext(["oneopt", "-o", "error"]), lifetime, logger);

        await service.StartAsync(TestContext.CancellationToken);
        var finished = await lifetime.WaitForStopAsync(TestContext.CancellationToken);
        finished.Should().BeTrue();
        service.Result.Should().Be(-2);

        logger.Messages.Should().HaveCount(1)
            .And.AllSatisfy(entry =>
            {
                entry.Key.Should().Be(LogLevel.Error);
                entry.Value.Should().StartWith("Failed to invoke command line 'oneopt -o error' due to error");
            });
    }

    private CommandLineInvoker SetupInvoker<TAction>(Command command, params object?[]? acrionArgs)
        where TAction : ICommandAction
    {
        _ = new AsyncBindingCommandLineAction<TAction>(command, () => (TAction)Activator.CreateInstance(typeof(TAction), acrionArgs)!);
        return new CommandLineInvoker([command], _registry, _serviceProvider);
    }
}
