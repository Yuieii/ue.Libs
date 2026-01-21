// Copyright (c) 2025 Yuieii.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ue.Core.Runtime;

namespace ue.Core
{
    public abstract class ScopeGuard : IDisposable
    {
        protected virtual bool ShouldEndScope => true;

        protected abstract void EndScope();

        public void Dispose()
        {
            if (!ShouldEndScope) return;
            EndScope();
        }
    }

    public class DelegateScopeGuard(Action endScope) : ScopeGuard
    {
        protected override void EndScope() => endScope();
    }

    public class OwnedUnmanagedResourceScopeGuard : ScopeGuard
    {
        private readonly CHandle _handle;
        
        public CHandle Handle => _handle;
        
        public OwnedUnmanagedResourceScopeGuard(CHandle handle)
        {
            if (handle.IsNull)
                throw new ArgumentException("The owned handle cannot be null.", nameof(handle));
            
            _handle = handle;
        }

        public OwnedUnmanagedResourceScopeGuard(Option<CHandle> handle)
        {
            _handle = handle.OrElse(CHandle.Null);
        }

        protected override bool ShouldEndScope
            => !_handle.IsNull;

        protected override void EndScope() 
            => Marshal.FreeHGlobal(_handle);
    }

    public class AllocHGlobalScopeGuard : OwnedUnmanagedResourceScopeGuard
    {
        public AllocHGlobalScopeGuard(int cb) 
            : base(new CHandle(Marshal.AllocHGlobal(cb)).ToOption())
        {
        }

        public AllocHGlobalScopeGuard(nint cb) 
            : base(new CHandle(Marshal.AllocHGlobal(cb)).ToOption())
        {
        }
    }

    public class SemaphoreSlimGuard : ScopeGuard
    {
        private readonly SemaphoreSlim _semaphore;

        private SemaphoreSlimGuard(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public static SemaphoreSlimGuard Create(SemaphoreSlim semaphore)
        {
            semaphore.Wait();
            return new SemaphoreSlimGuard(semaphore);
        }

        public static async Task<SemaphoreSlimGuard> CreateAsync(SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            return new SemaphoreSlimGuard(semaphore);
        }

        protected override void EndScope() => _semaphore.Release();
    }
}