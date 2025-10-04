using diVISION.CommandLineX.Tests;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(AssertionEngineInitializer),
    nameof(AssertionEngineInitializer.AcknowledgeSoftWarning))]

