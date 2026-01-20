// Copyright (c) 2026 Yuieii.

using System.Runtime.CompilerServices;

namespace ue.Core.Runtime
{
    public static class CHandleExtensions
    {
        extension<T>(T handle) where T : struct, ICHandle
        {
            public static T Null => default;
            
            public bool IsNull => handle.Handle == CHandle.Null;
        }
    }
    
    public interface ICHandle
    {
        CHandle Handle { get; }
        
        bool IsNull => Handle == CHandle.Null;
    }

    public interface ICHandle<T> : ICHandle
        where T : struct, ICHandle<T>
    {
#if NET7_0_OR_GREATER
        static virtual T Null => default;
#endif
        
        CHandle ICHandle.Handle
        {
            get
            {
                var self = (T) this;
                return Unsafe.As<T, CHandle>(ref self);
            }
        }
    }
}