using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace MvvmValidation
{
	public class NumberedListValidationResultFormatter : IValidationResultFormatter
	{
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
	}
}