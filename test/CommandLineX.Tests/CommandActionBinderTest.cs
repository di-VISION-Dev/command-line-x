using diVISION.CommandLineX.Binding;
using System.CommandLine;
using FluentAssertions;
using diVISION.CommandLineX.Tests.Mocks;

namespace diVISION.CommandLineX.Tests
{
    [TestClass]
    public sealed class CommandActionBinderTest
    {
        [TestInitialize]
        public void TestInit()
        {
            // This method is called before each test method.
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // This method is called after each test method.
        }

        [TestMethod]
        public void Create_binding_to_NoArgsCommandAction_without_Symbols_given_arumentless_Command()
        {
            var binder = CommandActionBinder<NoArgsCommandAction>.Create(new("simple"), () => new());
            binder.Should().NotBeNull();
            binder.TypeBindings.GetMappings().Should().BeEmpty();
            binder.TypeBindings.GetUnboundSymbols().Should().BeEmpty();
        }

        [TestMethod]
        public void Create_binding_to_OneIntArgCommandAction_with_int_Symbol_given_Command_with_int_argument()
        {
            var binder = CommandActionBinder<OneIntArgCommandAction>.Create(new("onearg")
                {
                    new Argument<int>("answer")
                }, () => new());
            binder.Should().NotBeNull();
            binder.TypeBindings.GetMappings().Should()
                .NotBeEmpty()
                .And.AllSatisfy(mapping =>
                {
                    mapping.Key.Name.Should().Be("answer");
                    mapping.Value.Name.Should().Be("Answer");
                });
            binder.TypeBindings.GetUnboundSymbols().Should().BeEmpty();
        }

        [TestMethod]
        public void Create_binding_to_OneIntArgCommandAction_with_unbound_Symbol_given_Command_with_string_argument()
        {
            var binder = CommandActionBinder<OneIntArgCommandAction>.Create(new("onearg")
                {
                    new Argument<string>("answer")
                }, () => new());
            binder.Should().NotBeNull();
            binder.TypeBindings.GetMappings().Should().BeEmpty();
            binder.TypeBindings.GetUnboundSymbols().Should()
                .NotBeEmpty()
                .And.AllSatisfy(symbol =>
                {
                    symbol.Name.Should().Be("answer");
                });
        }

        [TestMethod]
        public void Create_binding_to_TwoPrimitiveArgsCommandAction_with_2_Symbols_given_Command_with_2_arguments()
        {
            var binder = CommandActionBinder<TwoPrimitiveArgsCommandAction>.Create(new("twoarg")
                {
                    new Argument<int>("the-answer"),
                    new Argument<string>("the-question")
                }, () => new());
            binder.Should().NotBeNull();
            binder.TypeBindings.GetMappings().Should()
                .NotBeEmpty()
                .And.Satisfy(
                    mapping => "the-answer" == mapping.Key.Name && "TheAnswer" == mapping.Value.Name,
                    mapping => "the-question" == mapping.Key.Name && "TheQuestion" == mapping.Value.Name
                );
            binder.TypeBindings.GetUnboundSymbols().Should().BeEmpty();
        }

        [TestMethod]
        public void Create_binding_to_OneStringOptionCommandAction_with_string_Symbol_given_Command_with_string_option()
        {
            var binder = CommandActionBinder<OneStringOptionCommandAction>.Create(new("oneopt")
                {
                    new Option<string>("--the-option", "-o")
                }, () => new());
            binder.Should().NotBeNull();
            binder.TypeBindings.GetMappings().Should()
                .NotBeEmpty()
                .And.AllSatisfy(mapping =>
                {
                    mapping.Key.Name.Should().Be("--the-option");
                    mapping.Value.Name.Should().Be("TheOption");
                });
            binder.TypeBindings.GetUnboundSymbols().Should().BeEmpty();
        }

        [TestMethod]
        public void Create_binding_to_OneStringOptionCommandAction_with_unbound_Symbol_given_Command_with_unaliased_string_option()
        {
            var binder = CommandActionBinder<OneStringOptionCommandAction>.Create(new("oneopt")
                {
                    new Option<string>("-o")
                }, () => new());
            binder.Should().NotBeNull();
            binder.TypeBindings.GetMappings().Should().BeEmpty();
            binder.TypeBindings.GetUnboundSymbols().Should()
                .NotBeEmpty()
                .And.AllSatisfy(symbol =>
                {
                    symbol.Name.Should().Be("-o");
                });
        }

        [TestMethod]
        public void Create_binding_to_ComplexArgAndOptionCommandAction_with_both_Symbols_given_Command_with_Guid_argument_and_FileInfo_option()
        {
            var binder = new CommandActionBinder<ComplexArgAndOptionCommandAction>(new("argandopt")
                {
                    new Argument<IEnumerable<Guid>>("guid-args"),
                    new Option<FileInfo>("--file-option", "-f")
                }, new());
            binder.Should().NotBeNull();
            binder.TypeBindings.GetMappings().Should()
                .NotBeEmpty()
                .And.Satisfy(
                    mapping => "guid-args" == mapping.Key.Name && "GuidArgs" == mapping.Value.Name,
                    mapping => "--file-option" == mapping.Key.Name && "FileOption" == mapping.Value.Name
                );
            binder.TypeBindings.GetUnboundSymbols().Should().BeEmpty();
        }
    }
}
