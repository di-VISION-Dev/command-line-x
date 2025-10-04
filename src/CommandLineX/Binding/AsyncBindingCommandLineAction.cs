/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using System.CommandLine;
using System.CommandLine.Invocation;

namespace diVISION.CommandLineX.Binding
{
    public class AsyncBindingCommandLineAction<T> : AsynchronousCommandLineAction, IBindingCommandLineAction
       where T : ICommandAction
    {
        protected readonly Command _command;
        protected readonly Func<T> _actionResolver;

        public AsyncBindingCommandLineAction(Command command, Func<T> actionResolver)
        {
            _command = command;
            _actionResolver = actionResolver;
            _command.Action = this;
        }

        public async override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            var binder = CommandActionBinder<T>.Create(_command, _actionResolver);
            return await binder.InvokeAsync(parseResult, cancellationToken);
        }
    }
}
