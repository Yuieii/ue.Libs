// Copyright (c) 2025 Yuieii.

using System;

namespace ue.Core
{
    public static class Validation
    {
        public static void EnsureNotNull(object? value)
        {
            if (value != null) return;
            throw new InvalidOperationException("The provided value cannot be null.");
        }
        
        public static void EnsureNotNull(object? value, string paramName)
        {
            if (value != null) return;
            throw new ArgumentNullException(paramName, "The provided value cannot be null");
        }

        public static void EnsureInclusiveBetween<T>(T start, T end, T value, Func<string> message)
            where T : IComparable<T>
        {
            if (value.CompareTo(start) >= 0 && value.CompareTo(end) <= 0)
                return;
            
            throw new ArgumentException(message());
        }

        public static void EnsureInclusiveBetween<T>(T start, T end, T value)
            where T : IComparable<T>
            => EnsureInclusiveBetween(start, end, value, () => $"The provided value must be between {start} and {end}");
    }
}