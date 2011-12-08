using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MvvmValidation.Internal;

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
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validatable")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "validatable")]
		public static void AddChildValidatable(this ValidationHelper validator, Expression<Func<IValidatable>> childValidatableGetter)
		{
			Contract.Requires(validator != null);
			Contract.Requires(childValidatableGetter != null);

			var getter = childValidatableGetter.Compile();
			
			validator.AddAsyncRule(PropertyName.For(childValidatableGetter), (Action<RuleResult> onCompleted) =>
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

		/// <summary>
		/// Creates a validation rule that validates all the <see cref="IValidatable"/> items in the collection specified in <paramref name="validatableCollectionGetter"/>
		/// and adds error to this object from all the validatable items in invalid.
		/// </summary>
		/// <param name="validator">An instance of <see cref="ValidationHelper"/> that is used for validation.</param>
		/// <param name="validatableCollectionGetter">Expression for getting the collection of <see cref="IValidatable"/> objects to add as child items.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validatable")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "validatable")]
		public static void AddChildValidatableCollection(this ValidationHelper validator, Expression<Func<IEnumerable<IValidatable>>> validatableCollectionGetter)
		{
			Contract.Requires(validator != null);
			Contract.Requires(validatableCollectionGetter != null);

			var getter = validatableCollectionGetter.Compile();

			validator.AddAsyncRule(PropertyName.For(validatableCollectionGetter), (Action<RuleResult> onCompleted) =>
			{
				var items = getter();

				if (items != null)
				{
					var result = new RuleResult();

					// Execute validation on all items at the same time, wait for all
					// to fininish and combine the results.

					var syncEvents = new List<WaitHandle>();
					var results = new List<ValidationResult>();

					foreach (var item in items)
					{
						var syncEvent = new ManualResetEvent(false);
						syncEvents.Add(syncEvent);

						item.Validate(r =>
						{
							results.Add(r);
							syncEvent.Set();
						});
					}

					if (syncEvents.Any())
					{
						// Wait for all items to finish validation
						WaitHandle.WaitAll(syncEvents.ToArray());

						// Add errors from all validation results
						foreach (var itemResult in results)
						{
							foreach (var error in itemResult.ErrorList)
							{
								result.AddError(error.ErrorText);
							}
						}
					}

					onCompleted(result);
				}
				else
				{
					onCompleted(RuleResult.Valid());
				}
			});
		}
	}
}