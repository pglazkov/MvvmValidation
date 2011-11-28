using System;
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
		private string lastName;

		public MainViewModel()
		{
			ConfigureValidationRules();
			SubmitCommant = new RelayCommand(Submit);
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
		}

		private void Submit()
		{
			Validator.ValidateAll();
		}
	}
}