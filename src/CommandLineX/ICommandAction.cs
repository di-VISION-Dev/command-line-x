/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
namespace diVISION.CommandLineX
{
    /// <summary>
    /// Interface for command action model binding.
    /// </summary>
    public interface ICommandAction
    {
        /// <summary>
        /// Called by the framework when a synchronous action binding matches the command line arguments.
        /// </summary>
        /// <seealso cref="CommandActionContext"/>
        /// <param name="context">current execution context</param>
        /// <returns></returns>
        int Invoke(CommandActionContext context);

        /// <summary>
        /// Called by the framework when an asynchronous action binding matches the command line arguments.
        /// </summary>
        /// <seealso cref="CommandActionContext"/>
        /// <param name="context">urrent execution context</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default);
    }
}
