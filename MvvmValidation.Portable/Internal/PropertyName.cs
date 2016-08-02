using System;
using System.Globalization;
using System.Linq.Expressions;

namespace MvvmValidation.Internal
{
    /// <summary>
    /// Gets property name using lambda expressions.
    /// </summary>
    internal static class PropertyName
    {
        /// <summary>
        /// Returns the property name by given expression.
        /// </summary>
        /// <typeparam name="T">Type of property's return value</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="compound"><c>True</c> if the full expression path should be used to build the string. For example, 
        /// call PropertyName.For(() => MyObj.Property.NestedProperty) will result in string "MyObj.Property.NestedProperty".
        /// If <c>False</c> it will return only the last part, which is "NestedProperty" in the example above.</param>
        public static string For<T>(Expression<Func<T>> expression, bool compound = true)
        {
            Expression body = expression.Body;
            return GetMemberName(body, compound);
        }

        /// <summary>
        /// Returns the property name by given expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="compound"><c>True</c> if the full expression path should be used to build the string. For example, 
        /// call PropertyName.For(() => MyObj.Property.NestedProperty) will result in string "MyObj.Property.NestedProperty".
        /// If <c>False</c> it will return only the last part, which is "NestedProperty" in the example above.</param>
        public static string For(Expression<Func<object>> expression, bool compound = true)
        {
            Expression body = expression.Body;
            return GetMemberName(body, compound);
        }

        /// <summary>
        /// Gets the member name by give expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="compound"><c>True</c> if the full expression path should be used to build the string. For example, 
        /// call GetMemberName(() => MyObj.Property.NestedProperty) will result in string "MyObj.Property.NestedProperty".
        /// If <c>False</c> it will return only the last part, which is "NestedProperty" in the example above.</param>
        private static string GetMemberName(Expression expression, bool compound = true)
        {
            var memberExpression = expression as MemberExpression;

            if (memberExpression != null)
            {
                if (compound && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    return GetMemberName(memberExpression.Expression) + "." + memberExpression.Member.Name;
                }

                return memberExpression.Member.Name;
            }

            var unaryExpression = expression as UnaryExpression;

            if (unaryExpression != null)
            {
                if (unaryExpression.NodeType != ExpressionType.Convert)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot interpret member from {0}",
                        expression));
                }

                return GetMemberName(unaryExpression.Operand);
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Could not determine member from {0}",
                expression));
        }
    }
}