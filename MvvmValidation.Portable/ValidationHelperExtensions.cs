using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IValidationRule AddRequiredRule([NotNull] this ValidationHelper validator,
			[NotNull] Expression<Func<object>> propertyExpression, [NotNull] string errorMessage)
		{
			Contract.Requires(validator != null);
			Contract.Requires(propertyExpression != null);
			Contract.Requires(!string.IsNullOrEmpty(errorMessage));
			Contract.Ensures(Contract.Result<IValidationRule>() != null);

			Func<object> propertyGetter = propertyExpression.Compile();

			return validator.AddRule(propertyExpression, () =>
			{
				object propertyValue = propertyGetter();

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
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public static IAsyncValidationRule AddChildValidatable([NotNull] this ValidationHelper validator,
			[NotNull] Expression<Func<IValidatable>> childValidatableGetter)
		{
			Contract.Requires(validator != null);
			Contract.Requires(childValidatableGetter != null);
			Contract.Ensures(Contract.Result<IAsyncValidationRule>() != null);

			Func<IValidatable> getter = childValidatableGetter.Compile();

			return validator.AddAsyncRule(PropertyName.For(childValidatableGetter), () =>
			{
				IValidatable validatable = getter();

				if (validatable != null)
				{
#if SILVERLIGHT_4
					validatable.Validate(result =>
					{
#else
                    return validatable.Validate().ContinueWith(r =>
                    {
                        ValidationResult result = r.Result;
#endif
						var ruleResult = new RuleResult();

						foreach (ValidationError error in result.ErrorList)
						{
							ruleResult.AddError(error.ErrorText);
						}

						return ruleResult;
					});
				}

				return Task.Factory.StartNew(() => RuleResult.Valid());
			});
		}

		/// <summary>
		/// Creates a validation rule that validates all the <see cref="IValidatable"/> items in the collection specified in <paramref name="validatableCollectionGetter"/>
		/// and adds error to this object from all the validatable items in invalid.
		/// </summary>
		/// <param name="validator">An instance of <see cref="ValidationHelper"/> that is used for validation.</param>
		/// <param name="validatableCollectionGetter">Expression for getting the collection of <see cref="IValidatable"/> objects to add as child items.</param>
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IAsyncValidationRule AddChildValidatableCollection([NotNull] this ValidationHelper validator,
			[NotNull] Expression<Func<IEnumerable<IValidatable>>> validatableCollectionGetter)
		{
			Contract.Requires(validator != null);
			Contract.Requires(validatableCollectionGetter != null);
			Contract.Ensures(Contract.Result<IAsyncValidationRule>() != null);

			Func<IEnumerable<IValidatable>> getter = validatableCollectionGetter.Compile();

			return validator.AddAsyncRule(PropertyName.For(validatableCollectionGetter), () =>
			{
				IEnumerable<IValidatable> items = getter();

				if (items == null)
				{
					return Task.Factory.StartNew(() => RuleResult.Valid());
				}

				return Task.Factory.StartNew(() =>
				{
					var result = new RuleResult();

					// Execute validation on all items at the same time, wait for all
					// to fininish and combine the results.

					var results = new List<ValidationResult>();

					var syncEvent = new ManualResetEvent(false);

					var itemsArray = items as IValidatable[] ?? items.ToArray();
					int[] numerOfThreadsNotYetCompleted = { itemsArray.Length };

					foreach (var item in itemsArray)
					{
#if SILVERLIGHT_4
						item.Validate(r =>
						{
							Exception ex = null;
#else
						item.Validate().ContinueWith(tr =>
						{
							ValidationResult r = tr.Result;
							AggregateException ex = tr.Exception;
#endif
							lock (results)
							{
								// ReSharper disable ConditionIsAlwaysTrueOrFalse
								if (ex == null && r != null)
								// ReSharper restore ConditionIsAlwaysTrueOrFalse
								{
									results.Add(r);
								}

								if (Interlocked.Decrement(ref numerOfThreadsNotYetCompleted[0]) == 0)
								{
									syncEvent.Set();
								}
							}
						});
					}

					if (numerOfThreadsNotYetCompleted[0] > 0)
					{
						// Wait for all items to finish validation
						syncEvent.WaitOne();

						// Add errors from all validation results
						foreach (ValidationResult itemResult in results)
						{
							foreach (ValidationError error in itemResult.ErrorList)
							{
								result.AddError(error.ErrorText);
							}
						}
					}

					return result;
				});
				
			});
		}
	}
}