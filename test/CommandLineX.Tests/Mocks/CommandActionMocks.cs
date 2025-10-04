using System.Numerics;

namespace diVISION.CommandLineX.Tests.Mocks
{
    internal interface INumberModifierService<T>
        where T : INumber<T>
    {
        public T Modify(T num);
    }

    internal class NoArgsCommandAction(INumberModifierService<int>? service = null) : ICommandAction
    {
        private readonly INumberModifierService<int>? _service = service;

        public int Invoke(CommandActionContext context)
        {
            return GetResult();
        }

        public Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetResult());
        }

        private int GetResult()
        {
            var result = 42;
            return _service?.Modify(result) ?? result;
        }
    }

    internal class OneIntArgCommandAction : ICommandAction
    {
        public int Answer { get; set; } = 0;

        public int Invoke(CommandActionContext context)
        {
            return Answer;
        }

        public Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Answer);
        }
    }

    internal class TwoPrimitiveArgsCommandAction : ICommandAction
    {
        public int TheAnswer { get; set; } = 0;
        public string TheQuestion { get; set; } = string.Empty;

        public int Invoke(CommandActionContext context)
        {
            return TheAnswer + TheQuestion.Length;
        }

        public Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TheAnswer + TheQuestion.Length);
        }
    }

    internal class OneStringOptionCommandAction : ICommandAction
    {
        public string TheOption { get; set; } = string.Empty;

        public int Invoke(CommandActionContext context)
        {
            CheckOption();
            return TheOption.Length;
        }

        public Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default)
        {
            CheckOption();
            return Task.FromResult(TheOption.Length);
        }

        private void CheckOption()
        {
            if ("error" == TheOption)
            {
                throw new ArgumentException($"Invalid option");
            }
        }
    }

    internal class ComplexArgAndOptionCommandAction : ICommandAction
    {
        public IEnumerable<Guid> GuidArgs { get; set; } = [];
        public FileInfo? FileOption { get; set; }

        public int Invoke(CommandActionContext context)
        {
            return GuidArgs.Count() + (null == FileOption ? 0 : 1);
        }

        public Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GuidArgs.Count() + (null == FileOption ? 0 : 1));
        }
    }

}