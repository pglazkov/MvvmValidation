using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace MvvmValidation
{
	/// <summary>
	/// An implementation of <see cref="IValidationResultFormatter"/> that formats the validation result as 
	/// a numbered list of errors or an empty string if valid.
	/// </summary>
	public class NumberedListValidationResultFormatter : IValidationResultFormatter
	{
		#region IValidationResultFormatter Members

		/// <summary>
		/// Converts the specified validation result object to a string.
		/// </summary>
		/// <param name="validationResult">The validation result to format.</param>
		/// <returns>
		/// A string representation of <paramref name="validationResult"/>
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public string Format(ValidationResult validationResult)
		{
			if (validationResult.IsValid)
			{
				return string.Empty;
			}

			if (validationResult.ErrorList.Count == 1)
			{
				return validationResult.ErrorList[0].ErrorText;
			}

			var result = new StringBuilder();
			for (int i = 1; i < validationResult.ErrorList.Count + 1; i++)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, "{0}. {1}", i, validationResult.ErrorList[i - 1].ErrorText);
				result.AppendLine();
			}

			return result.ToString().Trim();
		}

		#endregion
	}
}