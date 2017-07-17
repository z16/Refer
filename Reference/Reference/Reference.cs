using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Reference
{
    public static class Reference
    {
        public static Reference<TProp, TBase> Create<TProp, TBase>(Expression<Func<TBase, TProp>> expression, TBase model = default(TBase))
            => new Reference<TProp, TBase>(expression, model);

        public static Reference<TProp, TBase> Bind<TProp, TBase>(this TBase model, Expression<Func<TBase, TProp>> expression)
            => Create(expression, model);
    }

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class Reference<TProp, TBase> : IReference<TProp, TBase>, IReference<TProp>, IReference
    {
        private readonly Lazy<Func<TBase, TProp>>  GetterFunction;
        private readonly Lazy<Action<TBase, TProp>> SetterFunction;
        public TBase Model { get; set; }

        public Reference(Expression<Func<TBase, TProp>> getterExpression, TBase model = default(TBase))
        {
            Model = model;

            GetterFunction = new Lazy<Func<TBase, TProp>>(getterExpression.Compile);
            SetterFunction = new Lazy<Action<TBase, TProp>>(CreateSetter);

            Action<TBase, TProp> CreateSetter()
            {
                var target = getterExpression.Parameters.Single();
                var value = Expression.Parameter(typeof(TProp), "value");
                var assignment = Expression.Assign(CreateSetterAssignmentTarget(), value);
                return Expression.Lambda<Action<TBase, TProp>>(assignment, target, value).Compile();
            }

            Expression CreateSetterAssignmentTarget()
            {
                var body = getterExpression.Body;

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

        Object IReference.ValueOrDefault => ValueOrDefault;

        public static implicit operator TProp(Reference<TProp, TBase> reference)
            => reference.Value;

        private TProp Getter(TBase model)
            => GetterFunction.Value(model);

        private void Setter(TBase model, TProp value)
            => SetterFunction.Value(model, value);
    }
}
