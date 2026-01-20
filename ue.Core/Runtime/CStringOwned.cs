// Copyright (c) 2026 Yuieii.

using System;
using System.Runtime.InteropServices;

namespace ue.Core.Runtime
{
    public readonly struct CStringOwned : 
        IDisposable, 
        IEquatable<CStringOwned>,
        ICHandle<CStringOwned>
    {
        public static CStringOwned Null => default;
        
        public nint Handle { get; }

        public CStringOwned(nint handle)
        {
            Handle = handle;
        }

        public CStringRef CreateRef() => new(Handle);
        
        public static CStringOwned FromString(string str) 
            => new(Marshal.StringToHGlobalAnsi(str));

        public void Dispose() 
            => Marshal.FreeHGlobal(Handle);

        public override string ToString()
        {
            if (Handle == 0) return string.Empty;
            return Marshal.PtrToStringAnsi(Handle).OrEmpty();
        }

        public bool Equals(CStringOwned other)
        {
            if (Handle == other.Handle) 
                return true;

            return ToString() == other.ToString();
        }

        public override bool Equals(object? obj) 
            => obj is CStringOwned other && Equals(other);

        public override int GetHashCode() 
            => ToString().GetHashCode();

        public static bool operator ==(CStringOwned left, CStringOwned right) 
            => left.Equals(right);

        public static bool operator !=(CStringOwned left, CStringOwned right) 
            => !left.Equals(right);
    }
}