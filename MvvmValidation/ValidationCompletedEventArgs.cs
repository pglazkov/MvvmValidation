using System;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	public class ValidationCompletedEventArgs : EventArgs
	{
		public ValidationResult ValidationResult { get; private set; }

		public ValidationCompletedEventArgs(ValidationResult validationResult)
		{
			Contract.Requires(validationResult != null);
			ValidationResult = validationResult;
		}
	}
}