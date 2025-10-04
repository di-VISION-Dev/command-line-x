using diVISION.CommandLineX.Binding;
using diVISION.CommandLineX.Tests.Mocks;
using FluentAssertions;
using System.CommandLine;

namespace diVISION.CommandLineX.Tests;

[TestClass]
public class AsyncBindingCommandLineActionTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task InvokeAsync_NoArgsCommandAction_returning_default_given_empty_Command()
    {
        var command = new Command("simple");
        var bindingAction = new AsyncBindingCommandLineAction<NoArgsCommandAction>(command, () => new());
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse(string.Empty), TestContext.CancellationTokenSource.Token);
        actionResult.Should().Be(42);
    }

    [TestMethod]
    public async Task InvokeAsync_NoArgsCommandAction_returning_default_given_Command_with_any_arguments()
    {
        var command = new Command("simple")
        {
            new Argument<int>("nonce")
        };
        var bindingAction = new AsyncBindingCommandLineAction<NoArgsCommandAction>(command, () => new());
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("666"), TestContext.CancellationTokenSource.Token);
        actionResult.Should().Be(42);
    }

    [TestMethod]
    public async Task InvokeAsync_OneIntArgCommandAction_returning_Argument_given_Command_with_int_argument()
    {
        var command = new Command("onearg")
        {
            new Argument<int>("answer")
        };
        var action = new OneIntArgCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<OneIntArgCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("42"), TestContext.CancellationTokenSource.Token);
        actionResult.Should().Be(action.Answer).And.Be(42);
    }

    [TestMethod]
    public async Task InvokeAsync_OneIntArgCommandAction_returning_default_given_Command_with_string_argument()
    {
        var command = new Command("onearg")
        {
            new Argument<string>("answer")
        };
        var action = new OneIntArgCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<OneIntArgCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("42"), TestContext.CancellationTokenSource.Token);
        actionResult.Should().Be(action.Answer).And.Be(0);
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
        var action = new OneIntArgCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<OneIntArgCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();

        await bindingAction.Invoking(async (x) => await bindingAction.InvokeAsync(command.Parse("43"), tokenSource.Token)).Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task InvokeAsync_TwoPrimitiveArgsCommandAction_returning_combination_of_Arguments_given_Command_with_2_arguments()
    {
        var command = new Command("twoargs")
        {
            new Argument<string>("the-question"),
            new Argument<int>("the-answer")
        };
        var action = new TwoPrimitiveArgsCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<TwoPrimitiveArgsCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var args = new string[] { "what's the question?", "42" };
        var actionResult = await bindingAction.InvokeAsync(command.Parse(args), TestContext.CancellationTokenSource.Token);
        action.TheQuestion.Should().Be(args[0]);
        action.TheAnswer.Should().Be(42);
        actionResult.Should().Be(args[0].Length + 42);
    }

    [TestMethod]
    public async Task InvokeAsync_OneStringOptionCommandAction_returning_Option_length_given_Command_with_string_option()
    {
        var command = new Command("oneopt")
        {
            new Option<string>("-o", ["--the-option"])
        };
        var action = new OneStringOptionCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<OneStringOptionCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("-o whatever"), TestContext.CancellationTokenSource.Token);
        action.TheOption.Should().Be("whatever");
        actionResult.Should().Be(action.TheOption.Length).And.Be("whatever".Length);
    }

    [TestMethod]
    public async Task InvokeAsync_OneStringOptionCommandAction_returning_default_given_Command__with_unaliased_string_option()
    {
        var command = new Command("oneopt")
        {
            new Option<string>("-o")
        };
        var action = new OneStringOptionCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<OneStringOptionCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("-o whatever"), TestContext.CancellationTokenSource.Token);
        action.TheOption.Should().BeEmpty();
        actionResult.Should().Be(0);
    }

    [TestMethod]
    public async Task InvokeAsync_ComplexArgAndOptionCommandAction_returning_Symbol_count_given_Command_with_Guid_argument_and_FileInfo_option()
    {
        var command = new Command("argandopt")
        {
            new Argument<IEnumerable<Guid>>("guid-args"),
            new Option<FileInfo>("-f", ["--file-option"])
        };
        var action = new ComplexArgAndOptionCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<ComplexArgAndOptionCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("E7AB96D2-C535-416B-959D-6DFC4F2F50AB -f testfile"), TestContext.CancellationTokenSource.Token);
        var file = new FileInfo("testfile");
        action.GuidArgs.Should().HaveCount(1).And.Contain(Guid.Parse("E7AB96D2-C535-416B-959D-6DFC4F2F50AB"));
        action.FileOption.Should().NotBeNull().And.Satisfy<FileInfo>(x => x.FullName.Should().Be(file.FullName));
        actionResult.Should().Be(2);
    }

    [TestMethod]
    public async Task InvokeAsync_ComplexArgAndOptionCommandAction_returning_Symbol_count_given_Command_with_2_Guid_argument_and_FileInfo_option()
    {
        var command = new Command("argandopt")
        {
            new Argument<IEnumerable<Guid>>("guid-args"),
            new Option<FileInfo>("-f", ["--file-option"])
        };
        var action = new ComplexArgAndOptionCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<ComplexArgAndOptionCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("E7AB96D2-C535-416B-959D-6DFC4F2F50AB 43B95992-25E0-40BC-AC59-D8B3E4CB7BFD -f testfile"), TestContext.CancellationTokenSource.Token);
        var testfile = new FileInfo("testfile");
        action.GuidArgs.Should().HaveCount(2).And.Contain([Guid.Parse("E7AB96D2-C535-416B-959D-6DFC4F2F50AB"), Guid.Parse("43B95992-25E0-40BC-AC59-D8B3E4CB7BFD")]);
        action.FileOption.Should().NotBeNull().And.Satisfy<FileInfo>(x => x.FullName.Should().Be(testfile.FullName));
        actionResult.Should().Be(3);
    }

    [TestMethod]
    public async Task InvokeAsync_ComplexArgAndOptionCommandAction_returning_Symbol_count_given_Command_with_2_Guid_arguments_only()
    {
        var command = new Command("argandopt")
        {
            new Argument<IEnumerable<Guid>>("guid-args")
        };
        var action = new ComplexArgAndOptionCommandAction();
        var bindingAction = new AsyncBindingCommandLineAction<ComplexArgAndOptionCommandAction>(command, () => action);
        bindingAction.Should().NotBeNull();
        var actionResult = await bindingAction.InvokeAsync(command.Parse("E7AB96D2-C535-416B-959D-6DFC4F2F50AB 43B95992-25E0-40BC-AC59-D8B3E4CB7BFD"), TestContext.CancellationTokenSource.Token);
        var file = new FileInfo("testfile");
        action.GuidArgs.Should().HaveCount(2).And.Contain([Guid.Parse("E7AB96D2-C535-416B-959D-6DFC4F2F50AB"), Guid.Parse("43B95992-25E0-40BC-AC59-D8B3E4CB7BFD")]);
        action.FileOption.Should().BeNull();
        actionResult.Should().Be(2);
    }
}
