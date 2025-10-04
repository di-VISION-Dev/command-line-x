/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using System.CommandLine;
using System.CommandLine.Invocation;

namespace diVISION.CommandLineX.Binding
{
    public class SyncBindingCommandLineAction<T> : SynchronousCommandLineAction, IBindingCommandLineAction
       where T : ICommandAction
    {
        protected readonly Command _command;
        protected readonly Func<T> _actionResolver;

        public SyncBindingCommandLineAction(Command command, Func<T> actionResolver)
        {
            _command = command;
            _actionResolver = actionResolver;
            command.Action = this;
        }

        public override int Invoke(ParseResult parseResult)
        {
            var binder = CommandActionBinder<T>.Create(_command, _actionResolver);
            return binder.Invoke(parseResult);
        }
    }
}
