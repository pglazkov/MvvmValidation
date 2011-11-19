using System;

namespace MvvmValidation
{
	public interface IValidationResultFormatter
	{
		string Format(ValidationResult validationResult);
	}
}