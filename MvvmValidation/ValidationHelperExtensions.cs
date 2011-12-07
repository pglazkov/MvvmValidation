using System;
using System.Diagnostics.CodeAnalysis;
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
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void AddRequiredRule(this ValidationHelper validator, Expression<Func<object>> propertyExpression, string errorMessage)
		{
			Contract.Requires(validator != null);
			Contract.Requires(propertyExpression != null);
			Contract.Requires(!string.IsNullOrEmpty(errorMessage));

			var propertyGetter = propertyExpression.Compile();

			validator.AddRule(propertyExpression, () =>
			{
				var propertyValue = propertyGetter();

				var stringPropertyValue = propertyValue as string;

				if (propertyValue == null || (stringPropertyValue != null && string.IsNullOrEmpty(stringPropertyValue)))
				{
					return RuleResult.Invalid(errorMessage);
				}

				return RuleResult.Valid();
			});
		}

		/// <summary>
		/// Creates a validation rule that validates the specified child <see cref="IValidatable"/> object and adds errors
		/// to this object if invalid.
		/// </summary>
		/// <param name="validator">An instance of <see cref="ValidationHelper"/> that is used for validation.</param>
		/// <param name="childValidatableGetter">Expression for getting the <see cref="IValidatable"/> object to add as child.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void AddChildValidatable(this ValidationHelper validator, Expression<Func<IValidatable>> childValidatableGetter)
		{
			Contract.Requires(validator != null);
			Contract.Requires(childValidatableGetter != null);

			var getter = childValidatableGetter.Compile();

			validator.AddAsyncRule(childValidatableGetter, (Action<RuleResult> onCompleted) =>
			{
				var validatable = getter();

				if (validatable != null)
				{
					validatable.Validate(result =>
					{
						var ruleResult = new RuleResult();

						foreach (var error in result.ErrorList)
						{
							ruleResult.AddError(error.ErrorText);
						}

						onCompleted(ruleResult);
					});
				}
				else
				{
					onCompleted(RuleResult.Valid());
				}
			});
		}
	}
}