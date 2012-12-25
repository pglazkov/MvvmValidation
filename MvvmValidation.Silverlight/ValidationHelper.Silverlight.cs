using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	public partial class ValidationHelper
	{
		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		public void ValidateAllAsync()
		{
			ValidateAllAsync(null);
		}

		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty).</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void ValidateAsync(Expression<Func<object>> propertyPathExpression)
		{
			ValidateAsync(propertyPathExpression, null);
		}

		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty, ...).</param>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void ValidateAsync(Expression<Func<object>> propertyPathExpression, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(propertyPathExpression != null);

			ValidateAsync(PropertyName.For(propertyPathExpression), onCompleted);
		}

		/// <summary>
		/// Executes validation for the given target asynchronously. 
		/// Executes all (normal and async) validation rules for the target object specified in the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		public void ValidateAsync(object target)
		{
			ValidateAsync(target, null);
		}

		/// <summary>
		/// Executes validation for the given target asynchronously. 
		/// Executes all (normal and async) validation rules for the target object specified in the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		public void ValidateAsync(object target, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(target != null);

			ValidateInternalAsync(target, onCompleted, null);
		}

		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		public void ValidateAllAsync(Action<ValidationResult> onCompleted)
		{
			ValidateInternalAsync(null, onCompleted, null);
		}

		private void ValidateInternalAsync(object target, Action<ValidationResult> onCompleted, Action<Exception> onException)
		{
			onCompleted = onCompleted ?? (r => { });
			onException = onException ?? (ex =>
			{
				throw new ValidationException("An exception occurred during validation. See inner exception for details.", ex);
			});

			if (isValidationSuspanded)
			{
				onCompleted(ValidationResult.Valid);
				return;
			}

			ExecuteValidationRulesAsync(target, r => ThreadingUtils.RunOnUI(() => onCompleted(r)), ex => ThreadingUtils.RunOnUI(() => onException(ex)));
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "Rethrowing the exception is not possible because execution is in a different thread and it would make it impossible to handle the exception on the calling side, so instead, calling a callback.")]
		private void ExecuteValidationRulesAsync(object target, Action<ValidationResult> completed, Action<Exception> onException)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					var rulesToExecute = GetRulesForTarget(target);

					ValidationResult result = ExecuteValidationRules(rulesToExecute);

					completed(result);
				}
				catch (Exception ex)
				{
					if (onException != null)
					{
						onException(ex);
					}
				}
			});
		}
	}
}
