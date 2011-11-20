using System;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	public class ValidationCompletedEventArgs : EventArgs
	{
		public ValidationCompletedEventArgs(ValidationResult validationResult)
		{
			Contract.Requires(validationResult != null);
			ValidationResult = validationResult;
		}

		public ValidationResult ValidationResult { get; private set; }
	}
}