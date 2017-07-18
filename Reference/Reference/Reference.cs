using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Reference
{
    /// <summary>
    /// Static methods.
    /// </summary>
    public static class Reference
    {
        /// <summary>
        /// Creates a reference to a <see cref="TProp"/> field or property, based on the provided <see cref="model"/>.
        /// </summary>
        /// <typeparam name="TProp">The type of the resulting value.</typeparam>
        /// <typeparam name="TBase">The type of the base object.</typeparam>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to retrieve the property from.</param>
        /// <returns>The reference object.</returns>
        public static Reference<TProp, TBase> Create<TProp, TBase>(Expression<Func<TBase, TProp>> expression, TBase model = default(TBase))
            => new Reference<TProp, TBase>(expression, model);

        /// <summary>
        /// Creates a reference to a <see cref="TProp"/> field or property, based on <see cref="model"/> it is called on.
        /// </summary>
        /// <typeparam name="TProp">The type of the resulting value.</typeparam>
        /// <typeparam name="TBase">The type of the base object.</typeparam>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to retrieve the property from.</param>
        /// <returns>The reference object.</returns>
        public static Reference<TProp, TBase> Bind<TProp, TBase>(this TBase model, Expression<Func<TBase, TProp>> expression)
            => Create(expression, model);
    }

    /// <summary>
    /// An <see cref="IReference{TProp}"/> implementation which takes a getter expression to extract a <see cref="TProp"/> property or field from the provided <see cref="Reference{TProp,TBase}.Model"/>.
    /// </summary>
    /// <typeparam name="TProp">The type of the resulting value.</typeparam>
    /// <typeparam name="TBase">The type of the base object.</typeparam>
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class Reference<TProp, TBase> : IReference<TProp>, IReference
    {
        private readonly Lazy<Func<TBase, TProp>>  GetterFunction;
        private readonly Lazy<Action<TBase, TProp>> SetterFunction;

        /// <summary>
        /// Gets or sets the base model to base the reference on.
        /// </summary>
        public TBase Model { get; set; }

        /// <summary>
        /// Creates a new reference based on the provided <see cref="expression"/> and optional <see cref="model"/>.
        /// </summary>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to retrieve the property from.</param>
        public Reference(Expression<Func<TBase, TProp>> expression, TBase model = default(TBase))
        {
            Model = model;

            GetterFunction = new Lazy<Func<TBase, TProp>>(expression.Compile);
            SetterFunction = new Lazy<Action<TBase, TProp>>(CreateSetter);

            Action<TBase, TProp> CreateSetter()
            {
                var target = expression.Parameters.Single();
                var value = Expression.Parameter(typeof(TProp), "value");
                var assignment = Expression.Assign(CreateSetterAssignmentTarget(), value);
                return Expression.Lambda<Action<TBase, TProp>>(assignment, target, value).Compile();
            }

            Expression CreateSetterAssignmentTarget()
            {
                var body = expression.Body;

                var binaryExpression = body as BinaryExpression;
                if (binaryExpression != null && binaryExpression.NodeType == ExpressionType.ArrayIndex)
                {
                    return Expression.ArrayAccess(binaryExpression.Left, binaryExpression.Right);
                }

                var methodCallExpression = body as MethodCallExpression;
                if (methodCallExpression?.Object != null)
                {
                    if (methodCallExpression.Method.Name == "get_Item")
                    {
                        return Expression.Property(methodCallExpression.Object, "Item", methodCallExpression.Arguments.Single());
                    }
                }

                return body;
            }
        }

        /// <summary>
        /// <inheritdoc cref="IReference{TProp}.Valid"/>
        /// </summary>
        public Boolean Valid
        {
            get
            {
                try
                {
                    Getter(Model);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="IReference{TProp}.Value"/>
        /// </summary>
        public TProp Value
        {
            get
            {
                try
                {
                    return Getter(Model);
                }
                catch (Exception ex)
                {
                    throw new InvalidReferenceException("Reference could not be retrieved.", ex);
                }
            }
            set
            {
                try
                {
                    Setter(Model, value);
                }
                catch (Exception ex)
                {
                    throw new InvalidReferenceException("Reference could not be set.", ex);
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="IReference{TProp}.ValueOrDefault"/>
        /// </summary>
        public TProp ValueOrDefault
        {
            get
            {
                try
                {
                    return Getter(Model);
                }
                catch (Exception)
                {
                    return default(TProp);
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="IReference.Value"/>
        /// </summary>
        Object IReference.Value
        {
            get => Value;
            set
            {
                try
                {
                    Value = (TProp) value;
                }
                catch (InvalidCastException ex)
                {
                    throw new InvalidReferenceException("Setter value of incorrect type.", ex);
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="IReference.ValueOrDefault"/>
        /// </summary>
        Object IReference.ValueOrDefault
            => ValueOrDefault;

        public static implicit operator TProp(Reference<TProp, TBase> reference)
            => reference.Value;

        private TProp Getter(TBase model)
            => GetterFunction.Value(model);

        private void Setter(TBase model, TProp value)
            => SetterFunction.Value(model, value);
    }
}
