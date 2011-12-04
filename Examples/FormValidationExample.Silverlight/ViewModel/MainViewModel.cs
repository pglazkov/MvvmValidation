using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using FormValidationExample.Infrastructure;
using FormValidationExample.Services;
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
		private string userName;
		private string validationErrorsString;

		public MainViewModel(IUserRegistrationService userRegistrationService)
		{
			Contract.Requires(userRegistrationService != null);

			UserRegistrationService = userRegistrationService;

			InterestSelectorViewModel = new InterestSelectorViewModel();
			InterestSelectorViewModel.SelectedInterestsChanged += OnSelectedInterestsChanged;

			SubmitCommant = new RelayCommand(Submit);

			ConfigureValidationRules();
			Validator.ResultChanged += OnValidationResultChanged;
		}

		private IUserRegistrationService UserRegistrationService { get; set; }

		public ICommand SubmitCommant { get; private set; }

		public string UserName
		{
			get { return userName; }
			set
			{
				userName = value;
				RaisePropertyChanged("UserName");
				Validator.ValidateAsync(() => UserName);
			}
		}

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
			Validator.AddRule(() => UserName,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(UserName), "User Name is required"));

			Validator.AddAsyncRule(() => UserName,
			                       ValidateUserNameIsAvailable);

			Validator.AddRule(() => FirstName,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(FirstName), "First Name is required"));

			Validator.AddRule(() => LastName,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(LastName), "Last Name is required"));

			Validator.AddRule(() => Email,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(Email), "Email is required"));

			Validator.AddRule(() => Email,
			                  () =>
			                  {
			                  	const string regexPattern =
			                  		@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$";
			                  	return RuleResult.Assert(string.IsNullOrEmpty(Email) || Regex.IsMatch(Email, regexPattern),
			                  	                         "Email must by a valid email address");
			                  });

			Validator.AddRule(() => Password,
			                  () => RuleResult.Assert(!string.IsNullOrEmpty(Password),
			                                          "Password is required"));

			Validator.AddRule(() => Password,
			                  () => RuleResult.Assert(string.IsNullOrEmpty(Password) || Password.Length >= 6,
			                                          "Password must contain at least 6 characters"));

			Validator.AddRule(() => Password,
			                  () => RuleResult.Assert(string.IsNullOrEmpty(Password) || (!Password.All(Char.IsLower) &&
			                                                                             !Password.All(Char.IsUpper) &&
			                                                                             !Password.All(Char.IsDigit)),
			                                          "Password must contain both lower case and upper case letters"));

			Validator.AddRule(() => Password,
			                  () => RuleResult.Assert(string.IsNullOrEmpty(Password) || Password.Any(Char.IsDigit),
			                                          "Password must contain at least one digit"));

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

		private void ValidateUserNameIsAvailable(Action<RuleResult> onCompleted)
		{
			if (string.IsNullOrEmpty(UserName))
			{
				onCompleted(RuleResult.Valid());
			}

			var asyncOperatoin = UserRegistrationService.IsUserNameAvailable(UserName);

			asyncOperatoin.Subscribe(
				isAvailable =>
				{
					var ruleResult = RuleResult.Assert(isAvailable, string.Format("User Name {0} is taken. Please choose a different one.", UserName));

					onCompleted(ruleResult);
				});
		}

		private void OnSelectedInterestsChanged(object sender, EventArgs e)
		{
			Validator.Validate(() => InterestSelectorViewModel);
		}

		private void Submit()
		{
			Validator.ValidateAllAsync(OnValidateAllCompleted);
		}

		private void OnValidateAllCompleted(ValidationResult validationResult)
		{
			UpdateValidationSummary(validationResult);
		}

		private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
		{
			if (!IsValid.GetValueOrDefault(true))
			{
				ValidationResult validationResult = Validator.GetResult();

				UpdateValidationSummary(validationResult);
			}
		}

		private void UpdateValidationSummary(ValidationResult validationResult)
		{
			IsValid = validationResult.IsValid;
			ValidationErrorsString = validationResult.ToString();
		}
	}
}