using System;

namespace Refer
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

        /// <summary>
        /// Gets the type of the <see cref="Value"/>. Always equal to <see cref="TProp"/>.
        /// </summary>
        Type ValueType { get; }
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

        /// <summary>
        /// Gets the type of the <see cref="Value"/>.
        /// </summary>
        Type ValueType { get; }
    }

    /// <inheritdoc/>
    /// <summary>
    /// Represents a <see cref="TProp"/> reference based on a <see cref="TBase"/> object.
    /// </summary>
    /// <typeparam name="TBase">The type of the base object.</typeparam>
    /// <typeparam name="TProp">The type of the referenced value.</typeparam>
    public interface IModelReference<TBase, TProp> : IReference<TProp>
    {
        /// <summary>
        /// The base object to apply the reference to.
        /// </summary>
        TBase Model { get; set; }

        /// <summary>
        /// Gets the value based on the provided model.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        TProp GetValue(TBase model);

        /// <summary>
        /// Gets the value based on the provided model. Returns the default value for <see cref="TProp"/> on error.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        TProp GetValueOrDefault(TBase model);

        /// <summary>
        /// Sets the value based on the provided model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        void SetValue(TBase model, TProp value);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Represents a reference based on a <see cref="TBase"/> object.
    /// </summary>
    /// <typeparam name="TBase">The type of the base object.</typeparam>
    public interface IModelReference<TBase> : IReference
    {
        /// <inheritdoc cref="IModelReference{TBase, TProp}.Model"/>
        TBase Model { get; set; }

        /// <inheritdoc cref="IModelReference{TBase, TProp}.GetValue"/>
        Object GetValue(TBase model);

        /// <inheritdoc cref="IModelReference{TBase, TProp}.GetValueOrDefault"/>
        Object GetValueOrDefault(TBase model);

        /// <inheritdoc cref="IModelReference{TBase, TProp}.SetValue"/>
        void SetValue(TBase model, Object value);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Represents a reference based on a object.
    /// </summary>
    public interface IModelReference : IReference
    {
        /// <inheritdoc cref="IModelReference{TBase, TProp}.Model"/>
        Object Model { get; set; }

        /// <inheritdoc cref="IModelReference{TBase, TProp}.GetValue"/>
        Object GetValue(Object model);

        /// <inheritdoc cref="IModelReference{TBase, TProp}.GetValueOrDefault"/>
        Object GetValueOrDefault(Object model);

        /// <inheritdoc cref="IModelReference{TBase, TProp}.SetValue"/>
        void SetValue(Object model, Object value);
    }
}
