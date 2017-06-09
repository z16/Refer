using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Reference
{
    public static class Reference
    {
        public static Reference<TProp, TBase> Create<TProp, TBase>(Expression<Func<TBase, TProp>> expression, TBase model = default(TBase))
        {
            return new Reference<TProp, TBase>(expression, model);
        }

        public static Reference<TProp, TBase> Bind<TProp, TBase>(this TBase model, Expression<Func<TBase, TProp>> expression)
        {
            return Create(expression, model);
        }
    }

    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class Reference<TProp, TBase> : IReference<TProp, TBase>, IReference<TProp>, IReference
    {
        private Expression<Func<TBase, TProp>> GetterExpression { get; }
        private Func<TBase, TProp>  GetterFunction { get; set; }
        private Action<TBase, TProp>  SetterFunction { get; set; }
        public TBase Model { get; set; }

        public Reference(Expression<Func<TBase, TProp>> getterExpression, TBase model = default(TBase))
        {
            GetterExpression = getterExpression;
            Model = model;
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
            get { return Value; }
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
        {
            return reference.Value;
        }

        public override String ToString()
        {
            return $"{typeof(TBase)} {GetterExpression}";
        }

        private TProp Getter(TBase model)
        {
            if (GetterFunction == null)
            {
                GetterFunction = GetterExpression.Compile();
            }
            return GetterFunction(model);
        }

        private void Setter(TBase model, TProp value)
        {
            if (SetterFunction == null)
            {
                SetterFunction = CreateSetter();
            }
            SetterFunction(model, value);
        }

        private Action<TBase, TProp> CreateSetter()
        {
            var target = GetterExpression.Parameters.Single();
            var value = Expression.Parameter(typeof(TProp), "value");
            var assignment = Expression.Assign(CreateSetterAssignmentTarget(GetterExpression.Body), value);
            return Expression.Lambda<Action<TBase, TProp>>(assignment, target, value).Compile();
        }

        private static Expression CreateSetterAssignmentTarget(Expression body)
        {
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
}
