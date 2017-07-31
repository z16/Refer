using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Refer
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
        public static Reference<TBase, TProp> Create<TBase, TProp>(Expression<Func<TBase, TProp>> expression, TBase model = default(TBase))
            => new Reference<TBase, TProp>(expression, model);

        /// <summary>
        /// Creates a reference to a <see cref="TProp"/> field or property, based on <see cref="model"/> it is called on.
        /// </summary>
        /// <typeparam name="TProp">The type of the resulting value.</typeparam>
        /// <typeparam name="TBase">The type of the base object.</typeparam>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to retrieve the property from.</param>
        /// <returns>The reference object.</returns>
        public static Reference<TBase, TProp> Bind<TBase, TProp>(this TBase model, Expression<Func<TBase, TProp>> expression)
            => Create<TBase, TProp>(expression, model);

        public static Reference<Object, TProp> ToReference<TProp>(this TProp property)
            => Create<Object, TProp>((_) => property);
    }

    /// <summary>
    /// Static methods.
    /// </summary>
    public static class Reference<TBase>
    {
        /// <summary>
        /// Creates a reference to a <see cref="TProp"/> field or property, based on the provided <see cref="model"/>.
        /// </summary>
        /// <typeparam name="TProp">The type of the resulting value.</typeparam>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to retrieve the property from.</param>
        /// <returns>The reference object.</returns>
        public static Reference<TBase, TProp> Create<TProp>(Expression<Func<TBase, TProp>> expression, TBase model = default(TBase))
            => new Reference<TBase, TProp>(expression, model);
    }

    /// <summary>
    /// An <see cref="IReference{TProp}"/> implementation which takes a getter expression to extract a <see cref="TProp"/> property or field from the provided <see cref="Reference{TProp,TBase}.Model"/>.
    /// </summary>
    /// <typeparam name="TBase">The type of the base object.</typeparam>
    /// <typeparam name="TProp">The type of the resulting value.</typeparam>
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class Reference<TBase, TProp> : IModelReference<TBase, TProp>, IModelReference<TBase>, IModelReference
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
                catch (InvalidReferenceException)
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
            get => Getter(Model);
            set => Setter(Model, value);
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

        /// <summary>
        /// <inheritdoc cref="IModelReference.Model"/>
        /// </summary>
        Object IModelReference.Model
        {
            get { return Model; }
            set { Model = (TBase)value; }
        }

        public static implicit operator TProp(Reference<TBase, TProp> reference)
            => reference.Value;

        private TProp Getter(TBase model)
        {
            try
            {
                return GetterFunction.Value(model);
            }
            catch (Exception ex)
            {
                throw new InvalidReferenceException("Reference could not be retrieved.", ex);
            }
        }

        private void Setter(TBase model, TProp value)
        {
            try
            {
                SetterFunction.Value(model, value);
            }
            catch (Exception ex)
            {
                throw new InvalidReferenceException("Reference could not be set.", ex);
            }
        }
    }
}