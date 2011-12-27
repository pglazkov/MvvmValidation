using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	public partial class ValidationHelper
	{
		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty, ...).</param>
		/// <returns>Task that represents the validation operation.</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Task<ValidationResult> ValidateAsync(Expression<Func<object>> propertyPathExpression)
		{
			Contract.Requires(propertyPathExpression != null);
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(PropertyName.For(propertyPathExpression));
		}

		/// <summary>
		/// Executes validation for the given target asynchronously. 
		/// Executes all (normal and async) validation rules for the target object specified in the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		/// <returns>Task that represents the validation operation.</returns>
		public Task<ValidationResult> ValidateAsync(object target)
		{
			Contract.Requires(target != null);
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(target);
		}

		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		/// <returns>Task that represents the validation operation.</returns>
		public Task<ValidationResult> ValidateAllAsync()
		{
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(null);
		}

		private Task<ValidationResult> ValidateInternalAsync(object target)
		{
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			if (isValidationSuspanded)
			{
				return Task.Factory.StartNew(() => ValidationResult.Valid);
			}

			return ExecuteValidationRulesAsync(target);
		}

		private Task<ValidationResult> ExecuteValidationRulesAsync(object target)
		{
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return Task.Factory.StartNew(() =>
			{
				try
				{
					var rulesToExecute = GetRulesForTarget(target);

					ValidationResult result = ExecuteValidationRules(rulesToExecute);

					return result;
				}
				catch(Exception ex)
				{
					throw new ValidationException("An exception occurred during validation. See inner exception for details.", ex);
				}
			});
		}
	}
}
