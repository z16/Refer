using System;

namespace Reference
{
    /// <summary>
    /// Represents a <see cref="TProp"/> reference.
    /// </summary>
    /// <typeparam name="TProp">The type of the referenced value.</typeparam>
    public interface IReference<TProp>
    {
        /// <summary>
        /// Gets whether or not the <see cref="Value"/> can be evaluated.
        /// </summary>
        Boolean Valid { get; }

        /// <summary>
        /// Gets or sets the value. Throws an <see cref="InvalidReferenceException"/> if it cannot be evaluated.
        /// </summary>
        TProp Value { get; set; }

        /// <summary>
        /// Gets the value. Returns the default <see cref="TProp"/> if it cannot be evaluated.
        /// </summary>
        TProp ValueOrDefault { get; }
    }

    /// <summary>
    /// Represents a reference.
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// Gets whether or not the <see cref="Value"/> can be evaluated.
        /// </summary>
        Boolean Valid { get; }

        /// <summary>
        /// Gets or sets the value. Throws an <see cref="InvalidReferenceException"/> if it cannot be evaluated.
        /// </summary>
        Object Value { get; set; }

        /// <summary>
        /// Gets the value. Returns null if it cannot be evaluated.
        /// </summary>
        Object ValueOrDefault { get; }
    }

    /// <summary>
    /// Represents a <see cref="TProp"/> reference based on a <see cref="TBase"/> object.
    /// </summary>
    /// <typeparam name="TBase">The type of the base object.</typeparam>
    /// <typeparam name="TProp">The type of the referenced value.</typeparam>
    public interface IModelReference<TBase, TProp> : IReference<TProp>
    {
        TBase Model { get; set; }
    }

    /// <summary>
    /// Represents a reference based on a <see cref="TBase"/> object.
    /// </summary>
    /// <typeparam name="TBase">The type of the base object.</typeparam>
    public interface IModelReference<TBase> : IReference
    {
        TBase Model { get; set; }
    }

    /// <summary>
    /// Represents a reference based on a object.
    /// </summary>
    public interface IModelReference : IReference
    {
        Object Model { get; set; }
    }
}
