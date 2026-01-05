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
    }
}