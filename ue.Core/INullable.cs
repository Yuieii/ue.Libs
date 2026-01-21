// Copyright (c) 2026 Yuieii.

namespace ue.Core
{
    /// <summary>
    /// Provides a common interface for anything that can be considered as <c>null</c>.
    /// </summary>
    public interface INullable
    {
        bool HasValue { get; }
    }

    /// <inheritdoc cref="INullable" />
    /// <typeparam name="T">The type of the non-null value.</typeparam>
    public interface INullable<out T> : INullable
    {
        T Value { get; }
    }
}