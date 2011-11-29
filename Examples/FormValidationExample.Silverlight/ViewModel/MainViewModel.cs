using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using FormValidationExample.Infrastructure;
using GalaSoft.MvvmLight.Command;
using MvvmValidation;

namespace FormValidationExample
{
	public class MainViewModel : ValidatableViewModelBase
	{
		private string email;
		private string firstName;
		private bool? isValid;
		private string lastName;
		private string password;
		private string passwordConfirmation;
		private string validationErrorsString;

		public MainViewModel()
		{
			InterestSelectorViewModel = new InterestSelectorViewModel();
			InterestSelectorViewModel.SelectedInterestsChanged += OnSelectedInterestsChanged;

			SubmitCommant = new RelayCommand(Submit);

			ConfigureValidationRules();
			Validator.ResultChanged += OnValidationResultChanged;
		}

		public ICommand SubmitCommant { get; private set; }

		public string FirstName
		{
			get { return firstName; }
			set
			{
				firstName = value;
				RaisePropertyChanged("FirstName");
				Validator.Validate(() => FirstName);
			}
		}

		public string LastName
		{
			get { return lastName; }
			set
			{
				lastName = value;
				RaisePropertyChanged("LastName");
				Validator.Validate(() => LastName);
			}
		}

		public string Email
		{
			get { return email; }
			set
			{
				email = value;
				RaisePropertyChanged("Email");
				Validator.Validate(() => Email);
			}
		}

		public string Password
		{
			get { return password; }
			set
			{
				password = value;
				RaisePropertyChanged("Password");
				Validator.Validate(() => Password);
			}
		}

		public string PasswordConfirmation
		{
			get { return passwordConfirmation; }
			set
			{
				passwordConfirmation = value;
				RaisePropertyChanged("PasswordConfirmation");
				Validator.Validate(() => PasswordConfirmation);
			}
		}

		public string ValidationErrorsString
		{
			get { return validationErrorsString; }
			private set
			{
				validationErrorsString = value;
				RaisePropertyChanged("ValidationErrorsString");
			}
		}

		public bool? IsValid
		{
			get { return isValid; }
			private set
			{
				isValid = value;
				RaisePropertyChanged("IsValid");
			}
		}

		public InterestSelectorViewModel InterestSelectorViewModel { get; private set; }

		private void ConfigureValidationRules()
		{
			Validator.AddRule(() => FirstName,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(FirstName), "First Name is required"));
			Validator.AddRule(() => LastName,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(LastName), "Last Name is required"));
			Validator.AddRule(() => Email,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(Email), "Email is required"));
			Validator.AddRule(() => Email,
			                  () =>
			                  {
			                  	if (string.IsNullOrEmpty(Email))
			                  	{
			                  		return RuleResult.Valid();
			                  	}

			                  	const string regexPattern =
			                  		@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$";
			                  	return RuleResult.Assert(Regex.IsMatch(Email, regexPattern), "Email must by a valid email address");
			                  });
			Validator.AddRule(() => Password,
			                  () =>
			                  {
			                  	RuleResult result = RuleResult.Assert(!string.IsNullOrEmpty(Password), "Password is required");

			                  	if (result.IsValid)
			                  	{
			                  		Debug.Assert(Password != null);

			                  		result = RuleResult.Assert(Password.Length >= 6, "Password must contain at least 6 characters").
			                  			Combine(
			                  				RuleResult.Assert(
			                  					!Password.All(Char.IsLower) && !Password.All(Char.IsUpper) && !Password.All(Char.IsDigit),
			                  					"Password must contain both lower case and upper case letters")).Combine(
			                  						RuleResult.Assert(Password.Any(Char.IsDigit), "Password must contain at least one digit"));
			                  	}

			                  	return result;
			                  });
			Validator.AddRule(() => PasswordConfirmation,
			                  () =>
			                  {
			                  	if (!string.IsNullOrEmpty(Password) && string.IsNullOrEmpty(PasswordConfirmation))
			                  	{
			                  		return RuleResult.Invalid("Please confirm password");
			                  	}

			                  	return RuleResult.Valid();
			                  });
			Validator.AddRule(() => Password,
			                  () => PasswordConfirmation,
			                  () =>
			                  {
			                  	if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(PasswordConfirmation))
			                  	{
			                  		return RuleResult.Assert(Password == PasswordConfirmation, "Passwords do not match");
			                  	}

			                  	return RuleResult.Valid();
			                  });

			Validator.AddRule(() => InterestSelectorViewModel,
			                  () =>
			                  RuleResult.Assert(InterestSelectorViewModel.SelectedInterests.Count() >= 3,
			                                    "Please select at least 3 interests"));
		}

		private void OnSelectedInterestsChanged(object sender, EventArgs e)
		{
			Validator.Validate(() => InterestSelectorViewModel);
		}

		private void Submit()
		{
			Validator.ValidateAll();
			//var validationResult = Validator.ValidateAll();

			//IsValid = validationResult.IsValid;
			//ValidationErrorsString = validationResult.ToString();
		}

		private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
		{
			var validationResult = Validator.GetResult();

			IsValid = validationResult.IsValid;
			ValidationErrorsString = validationResult.ToString();
		}
	}
}