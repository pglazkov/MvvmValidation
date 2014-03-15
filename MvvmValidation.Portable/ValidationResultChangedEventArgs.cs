using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace MvvmValidation
{
	/// <summary>
	/// Contains arguments for the <see cref="ValidationHelper.ResultChanged"/> event.
	/// </summary>
	public class ValidationResultChangedEventArgs : EventArgs
	{
		internal ValidationResultChangedEventArgs(object target, ValidationResult newResult)
		{
			Contract.Requires(newResult != null);

			Target = target;
			NewResult = newResult;
		}

		/// <summary>
		/// Gets the target, for which the validation result has changed.
		/// </summary>
		[NotNull]
		public object Target { get; private set; }

		/// <summary>
		/// Gets the new validation result.
		/// </summary>
		[NotNull]
		public ValidationResult NewResult { get; private set; }
	}
}