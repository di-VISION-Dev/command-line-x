/**
 * Copyright © 2025 diVISION
 * Code distributed under MIT license, any use with non-OSS LLM is prohibited
 * Redistribution requires inclusion of this comment header
 **/
using Microsoft.Extensions.DependencyInjection;

namespace diVISION.CommandLineX.Hosting
{
    public class CommandActionRegistry
    {
        protected List<Type> _actionTypes = [];

        public void RegisterActionType<T>()
        {
            _actionTypes.Add(typeof(T));
        }

        public bool HasActionType<T>()
        {
            return _actionTypes.Contains(typeof(T));
        }

        public void Resolve(IServiceProvider serviceProvider)
        {
            foreach (var type in _actionTypes)
            {
                _ = serviceProvider.GetRequiredService(type);
            }
        }
    }
}
