using System;

namespace MvvmValidation
{
	/// <summary>
	/// Represents a formatter that can be used to format an instance of <see cref="ValidationResult"/> to a string.
	/// </summary>
	public interface IValidationResultFormatter
	{
		/// <summary>
		/// Converts the specified validation result object to a string.
		/// </summary>
		/// <param name="validationResult">The validation result to format.</param>
		/// <returns>A string representation of <paramref name="validationResult"/></returns>
		string Format(ValidationResult validationResult);
	}
}