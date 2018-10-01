using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Refer
{
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
        public virtual TBase Model { get; set; }

        /// <summary>
        /// Creates a new reference based on the provided <see cref="expression"/>.
        /// </summary>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        public Reference(Expression<Func<TBase, TProp>> expression)
        {
            GetterFunction = new Lazy<Func<TBase, TProp>>(expression.Compile);
            SetterFunction = new Lazy<Action<TBase, TProp>>(CreateSetter);

            Action<TBase, TProp> CreateSetter()
            {
                var target = expression.Parameters.Single();
                var value = Expression.Parameter(typeof(TProp), "value");
                var assignment = Expression.Assign(CreateSetterAssignmentTarget(), value);
                return Expression.Lambda<Action<TBase, TProp>>(assignment, target, value).Compile();

                Expression CreateSetterAssignmentTarget()
                {
                    var body = expression.Body;

                    var binaryExpression = body as BinaryExpression;
                    if (binaryExpression != null && binaryExpression.NodeType == ExpressionType.ArrayIndex)
                    {
                        return Expression.ArrayAccess(binaryExpression.Left, binaryExpression.Right);
                    }

                    var methodCallExpression = body as MethodCallExpression;
                    if (methodCallExpression?.Object != null && methodCallExpression.Method.Name == "get_Item")
                    {
                        return Expression.Property(methodCallExpression.Object, "Item", methodCallExpression.Arguments.Single());
                    }

                    return body;
                }
            }
        }

        /// <summary>
        /// Creates a new reference based on the provided <see cref="expression"/> and <see cref="model"/>.
        /// </summary>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to retrieve the property from.</param>
        public Reference(Expression<Func<TBase, TProp>> expression, TBase model)
            : this(expression)
        {
            Model = model;
        }

        /// <inheritdoc cref="IReference{TProp}.Valid"/>
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

        /// <inheritdoc cref="IReference{TProp}.Value"/>
        public TProp Value
        {
            get => GetValue(Model);
            set => SetValue(Model, value);
        }

        /// <inheritdoc cref="IReference.Value"/>
        Object IReference.Value
        {
            get => Value;
            set => Value = (TProp) value;
        }

        /// <inheritdoc cref="IReference{TProp}.ValueOrDefault"/>
        public TProp ValueOrDefault
            => GetValueOrDefault(Model);

        /// <inheritdoc cref="IReference.ValueOrDefault"/>
        Object IReference.ValueOrDefault
            => ValueOrDefault;

        /// <inheritdoc cref="IModelReference.Model"/>
        Object IModelReference.Model
        {
            get { return Model; }
            set { Model = (TBase) value; }
        }

        public TProp GetValue(TBase model)
            => Getter(model);

        public Object GetValue(Object model)
            => GetValue((TBase) model);

        object IModelReference<TBase>.GetValue(TBase model)
            => GetValue(model);

        public TProp GetValueOrDefault(TBase model)
        {
            try
            {
                return Getter(model);
            }
            catch (InvalidReferenceException)
            {
                return default(TProp);
            }
        }

        public Object GetValueOrDefault(Object model)
            => GetValueOrDefault((TBase) model);

        Object IModelReference<TBase>.GetValueOrDefault(TBase model)
            => GetValueOrDefault(model);

        public void SetValue(TBase model, TProp value)
            => Setter(model, value);

        public void SetValue(TBase model, Object value)
            => SetValue(model, (TProp) value);

        public void SetValue(Object model, Object value)
            => SetValue((TBase) model, value);

        public static implicit operator TProp(Reference<TBase, TProp> reference)
            => reference.Value;

        /// <inheritdoc cref="IReference{TProp}.ValueType"/>
        public Type ValueType
            => typeof(TProp);

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

        /// <summary>
        /// Creates a reference to a <see cref="TProp"/> field or property, based on the provided <see cref="model"/>.
        /// </summary>
        /// <typeparam name="TProp">The type of the resulting value.</typeparam>
        /// <param name="expression">A getter that retrieves the <see cref="Reference{TProp, TBase}.Value"/> from the <see cref="Reference{TProp,TBase}.Model"/>.</param>
        /// <param name="model">The base object to retrieve the property from.</param>
        /// <returns>The reference object.</returns>
        public static Reference<TBase, TProp> Create(Expression<Func<TBase, TProp>> expression, TBase model = default(TBase))
            => new Reference<TBase, TProp>(expression, model);
    }

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
            => Create(expression, model);
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
}
