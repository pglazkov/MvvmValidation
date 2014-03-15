using System;
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
	public partial class ValidationHelper
	{
		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty, ...).</param>
		/// <returns>Task that represents the validation operation.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Task<ValidationResult> ValidateAsync([NotNull] Expression<Func<object>> propertyPathExpression)
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
		[NotNull]
		public Task<ValidationResult> ValidateAsync([NotNull] object target)
		{
			Contract.Requires(target != null);
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(target);
		}

		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		/// <returns>Task that represents the validation operation.</returns>
		[NotNull]
		public Task<ValidationResult> ValidateAllAsync()
		{
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(null);
		}

		private Task<ValidationResult> ValidateInternalAsync(object target)
		{
			if (isValidationSuspended)
			{
				return Task.Factory.StartNew(() => ValidationResult.Valid);
			}

			return ExecuteValidationRulesAsync(target);
		}

		private Task<ValidationResult> ExecuteValidationRulesAsync(object target)
		{
			var syncContext = SynchronizationContext.Current;

			return Task.Factory.StartNew(() =>
			{
				lock (syncRoot)
				{
					try
					{
						var rulesToExecute = GetRulesForTarget(target);

						ValidationResult result = ExecuteValidationRules(rulesToExecute, syncContext);

						return result;
					}
					catch (Exception ex)
					{
						throw new ValidationException("An exception occurred during validation. See inner exception for details.", ex);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}
	}
}
