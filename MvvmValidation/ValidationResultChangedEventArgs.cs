using System;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	public class ValidationResultChangedEventArgs : EventArgs
	{
		public ValidationResultChangedEventArgs(object target, ValidationResult newResult)
		{
			Contract.Requires(newResult != null);

			Target = target;
			NewResult = newResult;
		}

		public object Target { get; private set; }
		public ValidationResult NewResult { get; private set; }
	}
}