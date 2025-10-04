using diVISION.CommandLineX.Binding;
using diVISION.CommandLineX.Hosting;
using diVISION.CommandLineX.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace diVISION.CommandLineX.Tests;

[TestClass]
public class CommmandLineHostingExtensionTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void UseCommandLine_setting_up_CommandActionRegistry_and_CommandLineInvoker_given_RootCommand()
    {
        var builder = Host.CreateDefaultBuilder([]).UseCommandLine([]);
        builder.Should().NotBeNull();
        builder.Properties.Should().ContainKey(typeof(CommandActionRegistry));
        builder.Properties[typeof(CommandActionRegistry)].Should().BeOfType<CommandActionRegistry>();

        using var host = builder.Build();
        var invoker = host.Services.GetService(typeof(ICommandLineInvoker));
        invoker.Should().NotBeNull().And.BeOfType(typeof(CommandLineInvoker));
    }

    [TestMethod]
    public void UseCommandLine_setting_up_CommandActionRegistry_and_CommandLineInvoker_and_CommandLineOptions_given_RootCommand_and_confugure_Action()
    {
        var builder = Host.CreateDefaultBuilder([]).UseCommandLine([], options =>
        {
            options.InvocationConfiguration = new() { EnableDefaultExceptionHandler = false };
            options.ParserConfiguration = new() { EnablePosixBundling = false };
        });
        builder.Should().NotBeNull();
        builder.Properties.Should().ContainKey(typeof(CommandActionRegistry));
        builder.Properties[typeof(CommandActionRegistry)].Should().BeOfType<CommandActionRegistry>();

        using var host = builder.Build();

        var invoker = host.Services.GetService<ICommandLineInvoker>();
        invoker.Should().NotBeNull().And.BeOfType(typeof(CommandLineInvoker));

        var invocationConfiguration = host.Services.GetService<InvocationConfiguration>();
        invocationConfiguration.Should().NotBeNull()
            .And.Satisfy<InvocationConfiguration>(config => config.EnableDefaultExceptionHandler.Should().BeFalse());
        var parserConfiguration = host.Services.GetService<ParserConfiguration>();
        parserConfiguration.Should().NotBeNull()
            .And.Satisfy<ParserConfiguration>(config => config.EnablePosixBundling.Should().BeFalse());
    }

    [TestMethod]
    public void UseHostedCommandInvocation_setting_up_CommandLineInvocationContext_and_CommandLineHostedService()
    {
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine([])
            .UseHostedCommandInvocation(["1", "2"]);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        var invocationContext = host.Services.GetService<CommandLineInvocationContext>();
        invocationContext.Should()
            .NotBeNull()
            .And.BeOfType(typeof(CommandLineInvocationContext))
            .And.Satisfy<CommandLineInvocationContext>(x => x.Args.Should().Equal(["1", "2"]));
        var hostedService = host.Services.GetService<IHostedService>();
        hostedService.Should().NotBeNull().And.BeOfType<CommandLineHostedService>();
    }


    [TestMethod]
    public void UseCommandWithAction_throwing_Exception_given_missing_CommandActionRegistry()
    {
        var builder = Host.CreateDefaultBuilder([]);
        builder.Should().NotBeNull();
        builder.Invoking(x => x.UseCommandWithAction<NoArgsCommandAction>(new RootCommand(), new("nonce"))).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void UseCommandWithAction_throwing_Exception_given_same_ICommandAction_used_twice()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseCommandWithAction<NoArgsCommandAction>(rootCommand, new("noarg1"));
        builder.Should().NotBeNull();
        builder.Invoking(x => x.UseCommandWithAction<NoArgsCommandAction>(rootCommand, new("noarg2"))).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    [DataRow(true, DisplayName = "AsyncBinding")]
    [DataRow(false, DisplayName = "SyncBinding")]
    public void UseCommandWithAction_setting_up_OneIntArgCommandAction_given_RootCommand_with_matching_Command(bool runAsync)
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, runAsync);
        builder.Should().NotBeNull();
        builder.Properties.Should().ContainKey(typeof(CommandActionRegistry));
        var registry = (CommandActionRegistry)builder.Properties[typeof(CommandActionRegistry)];

        using var host = builder.Build();
        host.Should().NotBeNull();
        registry.Resolve(host.Services);

        var action = host.Services.GetService<OneIntArgCommandAction>();
        action.Should().NotBeNull();

        var command = rootCommand.Subcommands.First();
        command.Should()
            .NotBeNull()
            .And.Satisfy<Command>(
                x => x.Action.Should()
                    .NotBeNull()
                    .And.BeOfType(runAsync ? typeof(AsyncBindingCommandLineAction<OneIntArgCommandAction>) : typeof(SyncBindingCommandLineAction<OneIntArgCommandAction>))
            );
    }

    [TestMethod]
    public void RunCommandLine_throwing_Exception_given_hosted_invocation_setup()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseHostedCommandInvocation(["onearg", "66"])
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        host.Invoking(x => x.RunCommandLine(["onearg", "66"])).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void RunCommandLine_returning_result_of_ComplexArgAndOptionCommandAction_given_RootCommand_with_matching_Command_and_arguments()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false)
            .UseCommandWithAction<ComplexArgAndOptionCommandAction>(rootCommand, new("argandopt")
            {
                new Argument<IEnumerable<Guid>>("guid-args"),
                new Option<FileInfo>("-f", ["--file-option"])
            }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        var runResult = host.RunCommandLine(["argandopt", "E7AB96D2-C535-416B-959D-6DFC4F2F50AB", "-f", "testfile"]);
        runResult.Should().Be(2);
    }

    [TestMethod]
    public async Task RunCommandLineAsync_throwing_Exception_given_hosted_invocation_setup()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseHostedCommandInvocation(["onearg", "66"])
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        await host.Invoking(async x => await x.RunCommandLineAsync(["onearg", "66"], TestContext.CancellationToken)).Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task RunCommandLineAsync_returning_result_of_ComplexArgAndOptionCommandAction_given_RootCommand_with_matching_Command_and_arguments()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false)
            .UseCommandWithAction<ComplexArgAndOptionCommandAction>(rootCommand, new("argandopt")
            {
                new Argument<IEnumerable<Guid>>("guid-args"),
                new Option<FileInfo>("-f", ["--file-option"])
            }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        var runResult = await host.RunCommandLineAsync(["argandopt", "E7AB96D2-C535-416B-959D-6DFC4F2F50AB", "-f", "testfile"], TestContext.CancellationToken);
        runResult.Should().Be(2);
    }

    [TestMethod]
    public void RunCommandLineHosted_throwing_Exception_given_missing_CommandLineInvocationContext()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        host.Invoking(x => x.RunCommandLineHosted()).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void RunCommandLineHosted_returning_result_of_ComplexArgAndOptionCommandAction_given_RootCommand_with_matching_Command_and_arguments()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseHostedCommandInvocation(["argandopt", "E7AB96D2-C535-416B-959D-6DFC4F2F50AB", "-f", "testfile"])
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false)
            .UseCommandWithAction<ComplexArgAndOptionCommandAction>(rootCommand, new("argandopt")
            {
                new Argument<IEnumerable<Guid>>("guid-args"),
                new Option<FileInfo>("-f", ["--file-option"])
            }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        var runResult = host.RunCommandLineHosted();
        runResult.Should().Be(2);
    }

    [TestMethod]
    public async Task RunCommandLineHostedAsync_throwing_Exception_given_missing_CommandLineInvocationContext()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        await host.Invoking(async x => await x.RunCommandLineHostedAsync(TestContext.CancellationToken)).Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task RunCommandLineHostedAsync_returning_result_of_ComplexArgAndOptionCommandAction_given_RootCommand_with_matching_Command_and_arguments()
    {
        var rootCommand = new RootCommand();
        var builder = Host.CreateDefaultBuilder([])
            .UseCommandLine(rootCommand)
            .UseHostedCommandInvocation(["argandopt", "E7AB96D2-C535-416B-959D-6DFC4F2F50AB", "-f", "testfile"])
            .UseCommandWithAction<OneIntArgCommandAction>(rootCommand, new("onearg") { new Argument<int>("answer") }, false)
            .UseCommandWithAction<ComplexArgAndOptionCommandAction>(rootCommand, new("argandopt")
            {
                new Argument<IEnumerable<Guid>>("guid-args"),
                new Option<FileInfo>("-f", ["--file-option"])
            }, false);
        builder.Should().NotBeNull();

        using var host = builder.Build();
        host.Should().NotBeNull();
        var runResult = await host.RunCommandLineHostedAsync(TestContext.CancellationToken);
        runResult.Should().Be(2);
    }
}
