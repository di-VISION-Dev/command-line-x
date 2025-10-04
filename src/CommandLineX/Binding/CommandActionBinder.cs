/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using System.CommandLine;
using System.Globalization;
using System.Reflection;

namespace diVISION.CommandLineX.Binding
{
    public class CommandActionTypeBindings
    {
        protected readonly Type _actionType;
        protected readonly Dictionary<Symbol, PropertyInfo> _bindings = [];
        protected readonly HashSet<Symbol> _unbound = [];

        public CommandActionTypeBindings(Command command, Type actionType)
        {
            _actionType = actionType;
            foreach (var arg in command.Arguments)
            {
                BindSymbol(arg);
            }
            foreach (var option in command.Options)
            {
                BindSymbol(option);
            }
        }

        public IEnumerable<KeyValuePair<Symbol, PropertyInfo>> GetMappings()
        {
            return _bindings;
        }

        public IEnumerable<Symbol> GetUnboundSymbols()
        {
            return _unbound;
        }

        protected void BindSymbol(dynamic symbol)
        {
            var name = MakePropertyName(symbol.Name);
            var prop = _actionType.GetProperty(name, symbol.ValueType);
            if (null == prop && symbol is Option)
            {
                foreach (var alias in symbol.Aliases)
                {
                    name = MakePropertyName(alias);
                    prop = _actionType.GetProperty(name, symbol.ValueType);
                    if (null != prop)
                    {
                        break;
                    }
                }
            }
            if (null == prop)
            {
                _unbound.Add(symbol);
            }
            else
            {
                _bindings[symbol] = prop;
            }
        }

        protected static string MakePropertyName(string symbolName)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(symbolName).Replace("-", "");
        }

    }

    public class CommandActionBinder<T>
       where T : ICommandAction
    {


        public static CommandActionBinder<T> Create(Command command, Func<T> actionResolver) => new(command, actionResolver);

        protected readonly ICommandAction _action;
        protected readonly CommandActionTypeBindings _typeBindings;


        protected CommandActionBinder(Command command, Func<T> actionResolver) : this(command, actionResolver())
        {
        }

        internal CommandActionBinder(Command command, T action)
        {
            _action = action;
            _typeBindings = new CommandActionTypeBindings(command, action.GetType());
        }

        internal CommandActionTypeBindings TypeBindings => _typeBindings;

        public int Invoke(ParseResult parseResult)
        {
            ResolveHandlerBindings(parseResult);
            return _action.Invoke(new CommandActionContext(parseResult, _typeBindings!.GetUnboundSymbols()));
        }

        public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            ResolveHandlerBindings(parseResult);
            return await _action.InvokeAsync(new CommandActionContext(parseResult, _typeBindings!.GetUnboundSymbols()), cancellationToken);
        }

        protected void ResolveHandlerBindings(ParseResult parseResult)
        {
            foreach (var entry in _typeBindings!.GetMappings())
            {
                entry.Value.SetValue(_action, GetSymbolValue(parseResult, entry.Key));
            }
        }

        protected static object? GetSymbolValue(ParseResult parseResult, dynamic symbol)
        {
            var m = parseResult.GetType().GetMethods().Where(m => m.Name == nameof(ParseResult.GetValue))
                .Select(m => new { Method = m, Params = m.GetParameters(), Args = m.GetGenericArguments() })
                .Where(x => 1 == x.Params.Length && 1 == x.Args.Length && x.Params[0].ParameterType.BaseType == symbol.GetType().BaseType)
                .Select(x => x.Method).FirstOrDefault();
            var getValue = m?.MakeGenericMethod(symbol.ValueType);
            return getValue?.Invoke(parseResult, new object[] { symbol });
        }

    }
}
