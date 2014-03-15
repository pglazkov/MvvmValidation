using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
		/// <returns>Task that represents the validation operation.</returns>
		[NotNull]
		Task<ValidationResult> Validate();
	}

	// ReSharper disable InconsistentNaming
	[ContractClassFor(typeof(IValidatable))]
	abstract class IValidatableContract : IValidatable
	{
		public Task<ValidationResult> Validate()
		{
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			throw new NotImplementedException();
		}
	}
	// ReSharper restore InconsistentNaming
}