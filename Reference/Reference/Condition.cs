using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Reference
{
    /// <summary>
    /// Static methods.
    /// </summary>
    public static class Condition
    {
        /// <summary>
        /// Creates a condition, based on the provided <see cref="model"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of the base object.</typeparam>
        /// <param name="expression">An expression that determines a boolean <see cref="Condition{TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to evalute the condition from.</param>
        /// <returns>The condition object.</returns>
        public static Condition<TBase> Create<TBase>(Expression<Func<TBase, Boolean>> expression, TBase model = default(TBase))
            => new Condition<TBase>(expression, model);

        /// <summary>
        /// Creates a condition, based on the provided <see cref="model"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of the base object.</typeparam>
        /// <param name="expression">An expression that determines a boolean <see cref="Condition{TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to evalute the condition from.</param>
        /// <returns>The condition object.</returns>
        public static Condition<TBase> BindCondition<TBase>(this TBase model, Expression<Func<TBase, Boolean>> expression)
            => Create(expression, model);
    }

    /// <summary>
    /// A wrapper to calculate a boolean condition based on the provided <see cref="Condition{TBase}.Model"/>.
    /// </summary>
    /// <typeparam name="TBase">The type of the base object.</typeparam>
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class Condition<TBase> : Reference<TBase, Boolean>
    {
        /// <summary>
        /// Creates a condition, based on the provided <see cref="model"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of the base object.</typeparam>
        /// <param name="expression">An expression that determines a boolean <see cref="Condition{TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to evalute the condition from.</param>
        /// <returns>The condition object.</returns>
        public Condition(Expression<Func<TBase, Boolean>> expression, TBase model = default(TBase))
            : base(expression, model)
        {
        }

        /// <summary>
        /// The condition's evaluted value. Throws an <see cref="InvalidReferenceException"/> if it cannot be evaluated.
        /// </summary>
        public new bool Value
            => base.Value;
    }
}
