// Copyright (c) 2026 Yuieii.

using System;

namespace ue.Core
{
#if NET7_0_OR_GREATER
    public interface ISingleton<out T>
    {
        /// <inheritdoc cref="Singleton{T}.Instance" />
        public static abstract T Instance { get; }
    }
#endif
    
    public class Singleton<T>
#if NET7_0_OR_GREATER
        : ISingleton<T>
#endif
        where T : class, new()
    {
        private static readonly Lazy<T> _instance = new(() => new T());
        
        /// <summary>
        /// Retrieves the instance of this singleton.
        /// </summary>
        public static T Instance => _instance.Value;
    }
}