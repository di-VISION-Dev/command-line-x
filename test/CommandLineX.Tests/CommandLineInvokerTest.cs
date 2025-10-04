using diVISION.CommandLineX.Binding;
using diVISION.CommandLineX.Hosting;
using diVISION.CommandLineX.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace diVISION.CommandLineX.Tests;

[TestClass]
public class CommandLineInvokerTest
{
    internal class ServiceProviderMock : IServiceProvider
    {
        public object? GetService(Type serviceType) => serviceType switch
        {
            var type when type == typeof(OneIntArgCommandAction) => new OneIntArgCommandAction(),
            var type when type == typeof(InvocationConfiguration) => new InvocationConfiguration() { EnableDefaultExceptionHandler = false },
            _ => null
        };
    }

    private readonly ServiceProviderMock _serviceProvider = new();
    private readonly CommandActionRegistry _registry = new();
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Invoke_OneIntArgCommandAction_returning_Argument_given_Command_with_int_argument()
    {
        var command = new Command("onearg")
        {
            new Argument<int>("answer")
        };
        SetupServices<OneIntArgCommandAction>(command, false);

        var invoker = new CommandLineInvoker([command], _registry, _serviceProvider);
        var commandResult = invoker.Invoke(["onearg", "42"]);
        commandResult.Should().Be(42);
    }

    [TestMethod]
    public async Task InvokeAsync_OneIntArgCommandAction_returning_Argument_given_Command_with_int_argument()
    {
        var command = new Command("onearg")
        {
            new Argument<int>("answer")
        };
        SetupServices<OneIntArgCommandAction>(command, true);

        var invoker = new CommandLineInvoker([command], _registry, _serviceProvider);
        var commandResult = await invoker.InvokeAsync(["onearg", "42"], TestContext.CancellationTokenSource.Token);
        commandResult.Should().Be(42);
    }

    [TestMethod]
    public async Task InvokeAsync_OneIntArgCommandAction_throwing_Exception_given_CancellationToken_cancelled()
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();
        var command = new Command("onearg")
        {
            new Argument<int>("answer")
        };
        SetupServices<OneIntArgCommandAction>(command, true);

        var invoker = new CommandLineInvoker([command], _registry, _serviceProvider);
        await invoker.Invoking(async (x) => await x.InvokeAsync(["onearg", "43"], tokenSource.Token)).Should().ThrowAsync<OperationCanceledException>();
    }

    private void SetupServices<TAction>(Command command, bool asyncAction)
        where TAction : ICommandAction
    {
        IBindingCommandLineAction _ = asyncAction
            ? new AsyncBindingCommandLineAction<TAction>(command, _serviceProvider.GetRequiredService<TAction>)
            : new SyncBindingCommandLineAction<TAction>(command, _serviceProvider.GetRequiredService<TAction>);

    }
}
