using System;

namespace Reference
{
    /// <summary>
    /// Represents a reference to a <see cref="TProp"/> object.
    /// </summary>
    /// <typeparam name="TProp">The type of the resulting value.</typeparam>
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
}
