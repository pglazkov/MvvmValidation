using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace MvvmValidation
{
	/// <summary>
	/// Represents a formatter that can be used to format an instance of <see cref="ValidationResult"/> to a string.
	/// </summary>
	[ContractClass(typeof(IValidationResultFormatterContract))]
	public interface IValidationResultFormatter
	{
		/// <summary>
		/// Converts the specified validation result object to a string.
		/// </summary>
		/// <param name="validationResult">The validation result to format.</param>
		/// <returns>A string representation of <paramref name="validationResult"/></returns>
		[NotNull]
		string Format([NotNull] ValidationResult validationResult);
	}

// ReSharper disable InconsistentNaming
	[ContractClassFor(typeof(IValidationResultFormatter))]
	internal abstract class IValidationResultFormatterContract : IValidationResultFormatter
// ReSharper restore InconsistentNaming
	{
		public string Format(ValidationResult validationResult)
		{
			Contract.Requires(validationResult != null);
			Contract.Ensures(Contract.Result<string>() != null);

			throw new NotImplementedException();
		}
	}
}