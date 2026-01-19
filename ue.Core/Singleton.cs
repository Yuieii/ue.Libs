// Copyright (c) 2026 Yuieii.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ue.Core.Attributes;

namespace ue.Core
{
#if NET7_0_OR_GREATER
    public interface ISingleton<out T>
    {
        /// <inheritdoc cref="Singleton{T}.Instance" />
        public static abstract T Instance { get; }
    }
#endif
    
    [RegisterLogger]
#if NET7_0_OR_GREATER
    public partial class Singleton<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        T> : ISingleton<T>
#else
    public partial class Singleton<T>
#endif
        where T : class
    {
        private static readonly Lazy<T> _instance = new(CreateInstance);
        
        /// <summary>
        /// Retrieves the instance of this singleton.
        /// </summary>
        public static T Instance => _instance.Value;

        private static T CreateInstance()
        {
#if NET7_0_OR_GREATER
            if (typeof(T).GetInterfaces().Contains(typeof(ISingleton<T>)))
            {
                var properties = typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var property = properties.FirstOrDefault(t => t.Name.EndsWith(".Instance"))
                    .ToOption()
                    .OrGet(() => properties.FirstOrDefault(t => t.Name == "Instance").ToOption());

                if (property.IsSome)
                {
                    return (T) property.Unwrap().GetValue(null)!;
                }
            }
#endif

            return (T) Activator.CreateInstance(typeof(T))!;
        }
    }
}