using FluentAssertions;

namespace diVISION.CommandLineX.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class AssertionEngineInitializer
    {
        public static void AcknowledgeSoftWarning()
        {
            License.Accepted = true;
        }
    }
}
