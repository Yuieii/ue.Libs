// Copyright (c) 2026 Yuieii.

using System;
using System.Runtime.InteropServices;

namespace ue.Core.Runtime
{
    public readonly struct CStringRef : ICHandle<CStringRef>, IEquatable<CStringRef>
    {
        public static CStringRef Null => default;
        
        public nint Handle { get; }

        public CStringRef(nint handle)
        {
            Handle = handle;
        }

        public override string ToString()
        {
            if (Handle == 0) return string.Empty;
            return Marshal.PtrToStringAnsi(Handle).OrEmpty();
        }

        public bool Equals(CStringRef other)
        {
            if (Handle == other.Handle) 
                return true;

            return ToString() == other.ToString();
        }

        public override bool Equals(object? obj) 
            => obj is CStringRef other && Equals(other);

        public override int GetHashCode() 
            => ToString().GetHashCode();

        public static bool operator ==(CStringRef left, CStringRef right) 
            => left.Equals(right);

        public static bool operator !=(CStringRef left, CStringRef right) 
            => !left.Equals(right);
    }
}