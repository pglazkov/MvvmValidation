using System;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	/// <summary>
	/// Represents an object that can be validated.
	/// </summary>
	[ContractClass(typeof(IValidatableContract))]
	public interface IValidatable
	{
		/// <summary>
		/// Validates the object asyncrhonously.
		/// </summary>
		/// <param name="onCompleted">A callback function that should be called when validation is completed.</param>
		void Validate(Action<ValidationResult> onCompleted);
	}

	// ReSharper disable InconsistentNaming
	[ContractClassFor(typeof(IValidatable))]
	abstract class IValidatableContract : IValidatable
	{
		public void Validate(Action<ValidationResult> onCompleted)
		{
			Contract.Requires(onCompleted != null);

			throw new NotImplementedException();
		}
	}
	// ReSharper restore InconsistentNaming
}