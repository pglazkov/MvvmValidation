using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using FormValidationExample.Infrastructure;
using FormValidationExample.Services;
using GalaSoft.MvvmLight.Command;
using MvvmValidation;

namespace FormValidationExample.ViewModel
{
	public class MainViewModel : ValidatableViewModelBase
	{
		private string email;
		private readonly NameInfo nameInfo = new NameInfo();
		private bool? isValid;
		private string password;
		private string passwordConfirmation;
		private string userName;
		private string validationErrorsString;

		public MainViewModel(IUserRegistrationService userRegistrationService)
		{
			UserRegistrationService = userRegistrationService;

			InterestSelectorViewModel = new InterestSelectorViewModel();
			InterestSelectorViewModel.SelectedInterestsChanged += OnSelectedInterestsChanged;

			ValidateCommand = new RelayCommand(Validate);

			ConfigureValidationRules();
			Validator.ResultChanged += OnValidationResultChanged;
		}

		private IUserRegistrationService UserRegistrationService { get; set; }

		public ICommand ValidateCommand { get; private set; }

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
			get { return nameInfo.FirstName; }
			set
			{
				nameInfo.FirstName = value;
				RaisePropertyChanged("FirstName");
				Validator.Validate(() => FirstName);
			}
		}

		public string LastName
		{
			get { return nameInfo.LastName; }
			set
			{
				nameInfo.LastName = value;
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
			Validator.AddRequiredRule(() => UserName, "User Name is required");

			Validator.AddAsyncRule(() => UserName,
				async () =>
				{
					var isAvailable = await UserRegistrationService.IsUserNameAvailable(UserName).ToTask();

					return RuleResult.Assert(isAvailable,
						string.Format("User Name {0} is taken. Please choose a different one.", UserName));
				});

			Validator.AddRequiredRule(() => FirstName, "First Name is required");

			Validator.AddRequiredRule(() => LastName, "Last Name is required");

			Validator.AddRequiredRule(() => Email, "Email is required");

			Validator.AddRule(() => Email,
			                  () =>
			                  {
			                  	const string regexPattern =
			                  		@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$";
			                  	return RuleResult.Assert(Regex.IsMatch(Email, regexPattern),
			                  	                         "Email must by a valid email address");
			                  });

			Validator.AddRequiredRule(() => Password, "Password is required");

			Validator.AddRule(() => Password,
			                  () => RuleResult.Assert(Password.Length >= 6,
			                                          "Password must contain at least 6 characters"));

			Validator.AddRule(() => Password,
			                  () => RuleResult.Assert((!Password.All(Char.IsLower) &&
			                                           !Password.All(Char.IsUpper) &&
			                                           !Password.All(Char.IsDigit)),
			                                          "Password must contain both lower case and upper case letters"));

			Validator.AddRule(() => Password,
			                  () => RuleResult.Assert(Password.Any(Char.IsDigit),
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

			Validator.AddChildValidatable(() => InterestSelectorViewModel);
		}

		private void OnSelectedInterestsChanged(object sender, EventArgs e)
		{
			var currentState = Validator.GetResult(() => InterestSelectorViewModel);

			if (!currentState.IsValid)
			{
				Validator.ValidateAsync(() => InterestSelectorViewModel);
			}
		}

		private void Validate()
		{
			var uiThread = TaskScheduler.FromCurrentSynchronizationContext();

			Validator.ValidateAllAsync().ContinueWith(r => OnValidateAllCompleted(r.Result), uiThread);
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