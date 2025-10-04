/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
namespace diVISION.CommandLineX
{
    public interface ICommandAction
    {
        int Invoke(CommandActionContext context);
        Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default);
    }
}
