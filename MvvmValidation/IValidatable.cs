using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	/// <summary>
	/// Represents an object that can be validated.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validatable")]
	[ContractClass(typeof(IValidatableContract))]
	public interface IValidatable
	{
		/// <summary>
		/// Validates the object and calls <paramref name="onCompleted"/> callback with the validation result.
		/// </summary>
		/// <param name="onCompleted">A callback that is called when validation is completed.</param>
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