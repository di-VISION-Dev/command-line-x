/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
namespace diVISION.CommandLineX.Hosting
{
    public interface ICommandLineInvoker
    {
        Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken = default);
        int Invoke(string[] args);
    }
}
