using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace MvvmValidation
{
	/// <summary>
	/// Contains extensions methods for <see cref="ValidationHelper"/>.
	/// </summary>
	public static class ValidationHelperExtensions
	{
		/// <summary>
		/// Adds a rule that checks that the property represented by <paramref name="propertyExpression"/> is not
		/// null or empty string. 
		/// </summary>
		/// <param name="validator">An instance of <see cref="ValidationHelper"/> that is used for validation.</param>
		/// <param name="propertyExpression">Expression that specifies the property to validate. Example: Validate(() => MyProperty).</param>
		/// <param name="errorMessage">Error message in case if the property is null or empty.</param>
		public static void AddRequiredRule(this ValidationHelper validator, Expression<Func<object>> propertyExpression, string errorMessage)
		{
			Contract.Requires(validator != null);
			Contract.Requires(propertyExpression != null);
			Contract.Requires(!string.IsNullOrEmpty(errorMessage));

			validator.AddRule(propertyExpression, () =>
			{
				var propertyGetter = propertyExpression.Compile();

				var propertyValue = propertyGetter();

				var stringPropertyValue = propertyValue as string;

				if (propertyValue == null || (stringPropertyValue != null && string.IsNullOrEmpty(stringPropertyValue)))
				{
					return RuleResult.Invalid(errorMessage);
				}

				return RuleResult.Valid();
			});
		}
	}
}