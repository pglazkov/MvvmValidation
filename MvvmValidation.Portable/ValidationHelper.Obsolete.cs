using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MvvmValidation
{
	public partial class ValidationHelper
	{
		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty, ...).</param>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[Obsolete("Use method that returns Task<ValidationResult> instead.")]
		public void ValidateAsync([NotNull] Expression<Func<object>> propertyPathExpression, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(propertyPathExpression != null);

			RunValidationWithCallback(() => ValidateAsync(propertyPathExpression), onCompleted);
		}

		/// <summary>
		/// Executes validation for the given target asynchronously. 
		/// Executes all (normal and async) validation rules for the target object specified in the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		[Obsolete("Use method that returns Task<ValidationResult> instead.")]
		public void ValidateAsync([NotNull] object target, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(target != null);

			RunValidationWithCallback(() => ValidateAsync(target), onCompleted);
		}

		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		[Obsolete("Use method that returns Task<ValidationResult> instead.")]
		public void ValidateAllAsync(Action<ValidationResult> onCompleted)
		{
			RunValidationWithCallback(ValidateAllAsync, onCompleted);
		}

		private static void RunValidationWithCallback(Func<Task<ValidationResult>> validateFunc, Action<ValidationResult> onCompleted)
		{
			var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

			validateFunc().ContinueWith(t => onCompleted(t.Result), uiScheduler);
		}
	}
}
