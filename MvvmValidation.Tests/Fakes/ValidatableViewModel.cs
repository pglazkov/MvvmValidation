using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MvvmValidation.Tests.Fakes
{
	public class ValidatableViewModel : ViewModelBase, IValidatable, INotifyDataErrorInfo
	{
		public ValidatableViewModel()
		{
			Validator = new ValidationHelper();
			DataErrorInfoValidationAdapter = new NotifyDataErrorInfoAdapter(Validator);
		}

		private NotifyDataErrorInfoAdapter DataErrorInfoValidationAdapter { get; set; }
		public ValidationHelper Validator { get; set; }

		public string Foo { get; set; }

		public IValidatable Child { get; set; }

		public IEnumerable<IValidatable> Children { get; set; }

		#region INotifyDataErrorInfo Members

		public IEnumerable GetErrors(string propertyName)
		{
			return DataErrorInfoValidationAdapter.GetErrors(propertyName);
		}

		public bool HasErrors
		{
			get { return DataErrorInfoValidationAdapter.HasErrors; }
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
		{
			add { DataErrorInfoValidationAdapter.ErrorsChanged += value; }
			remove { DataErrorInfoValidationAdapter.ErrorsChanged -= value; }
		}

		#endregion

		public Task<ValidationResult> Validate()
		{
			return Validator.ValidateAllAsync();
		}
	}
}