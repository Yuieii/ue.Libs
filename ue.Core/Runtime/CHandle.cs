// Copyright (c) 2026 Yuieii.

using System;

namespace ue.Core.Runtime
{
    public readonly struct CHandle : ICHandle<CHandle>, IEquatable<CHandle>
    {
        public nint Handle { get; }

        public static CHandle Null => default;
        
        public bool IsNull => this == Null;
        
        public CHandle(nint handle)
        {
            Handle = handle;
        }
        
        public static implicit operator CHandle(nint handle) => new(handle);
        public static implicit operator nint(CHandle handle) => handle.Handle;

        public bool Equals(CHandle other)
        {
            return Handle == other.Handle;
        }

        public override bool Equals(object? obj)
        {
            return obj is CHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public static bool operator ==(CHandle left, CHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CHandle left, CHandle right)
        {
            return !left.Equals(right);
        }
    }
}