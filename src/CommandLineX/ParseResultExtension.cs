using System.CommandLine;
using System.Reflection;

namespace diVISION.CommandLineX
{
    public static class ParseResultExtension
    {
        public static object? GetSymbolValue(this ParseResult parseResult, dynamic symbol)
        {
            var getValue = GetValueGetter(parseResult, symbol.GetType().BaseType, symbol.ValueType);
#pragma warning disable IDE0300 // Simplify collection initialization
            return getValue!.Invoke(parseResult, new object[] { symbol });
#pragma warning restore IDE0300 // Simplify collection initialization
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static MethodInfo? GetValueGetter(this ParseResult parseResult, Type baseType, Type valueType)
        {
            var m = parseResult.GetType().GetMethods().Where(m => m.Name == nameof(ParseResult.GetValue))
                .Select(m => new { Method = m, Params = m.GetParameters(), Args = m.GetGenericArguments() })
                .Where(x => 1 == x.Params.Length && 1 == x.Args.Length && x.Params[0].ParameterType.BaseType == baseType)
                .Select(x => x.Method).FirstOrDefault();
            return m?.MakeGenericMethod(valueType);
        }
    }
}
