using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using MvvmValidation;

namespace FormValidationExample
{
	public class MainViewModel : ValidatableViewModelBase
	{
		private string firstName;

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
				if (Equals(firstName, value))
				{
					return;
				}

				firstName = value;
				RaisePropertyChanged("FirstName");
			}
		}

		private void ConfigureValidationRules()
		{
			Validator.AddRule(() => FirstName, () =>
			{
				return RuleValidationResult.Assert(!string.IsNullOrEmpty(FirstName), "First Name is required");
			});
		}

		private void Submit()
		{
			Validator.ValidateAll();
		}
	}
}