using System;
using System.Text;

namespace MvvmValidation
{
	public class NumberedListValidationResultFormatter : IValidationResultFormatter
	{
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
				result.AppendFormat("{0}. {1}", i, validationResult.ErrorList[i - 1].ErrorText);
				result.AppendLine();
			}

			return result.ToString().Trim();
		}
	}
}