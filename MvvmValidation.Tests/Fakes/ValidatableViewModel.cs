using System;

namespace MvvmValidation.Tests.Fakes
{
	public class ValidatableViewModel : ViewModelBase, IValidatable
	{
		public ValidatableViewModel()
		{
			Validator = new ValidationHelper();
		}

		public ValidationHelper Validator { get; set; }

		public string Foo { get; set; }

		public IValidatable Child { get; set; }

		public void Validate(Action<ValidationResult> onCompleted)
		{
			Validator.ValidateAllAsync(onCompleted);
		}
	}
}