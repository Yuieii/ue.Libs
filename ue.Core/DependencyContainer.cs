// Copyright (c) 2025 Yuieii.

using System;
using System.Collections.Concurrent;

namespace ue.Core
{
    public class DependencyContainer
    {
        private readonly ConcurrentDictionary<Type, object> _dependencies = [];
        private readonly Option<DependencyContainer> _parent;

        public DependencyContainer(Option<DependencyContainer> parent = default)
        {
            _parent = parent;
        }
        
        public DependencyContainer(DependencyContainer? parent = null)
        {
            _parent = parent.ToOption();
        }

        public void Register<T>(T dependency)
            => Register(typeof(T), dependency!);

        public bool HasService<T>()
            => _dependencies.ContainsKey(typeof(T));
        
        public bool HasService(Type type)
            => _dependencies.ContainsKey(type);
        
        public void Register(Type type, object dependency)
        {
            if (dependency == null)
            {
                throw new ArgumentException("Dependency cannot be null.", nameof(dependency));                
            }
            
            if (_parent
                    .Select(o => o.HasService(type))
                    .OrElse(false) || !_dependencies.TryAdd(type, dependency))
            {
                throw new InvalidOperationException($"The dependency of type {type.FullName} is already registered.");
            }
        }

        public T Get<T>() => (T) Get(typeof(T));

        public object Get(Type type)
        {
            if (_dependencies.TryGetValue(type, out var result))
                return result;
            
            return _parent
                .Select(o => o.Get(type))
                .Expect(new ArgumentException($"The dependency of type {type.FullName} is not registered."));
        }
    }
}