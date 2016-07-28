using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using MvvmValidation;

namespace FormValidationExample.Infrastructure
{
	public abstract class ValidatableViewModelBase : ViewModelBase, IValidatable, INotifyDataErrorInfo
    {
		protected ValidationHelper Validator { get; }

		private NotifyDataErrorInfoAdapter NotifyDataErrorInfoAdapter { get; }

	    protected ValidatableViewModelBase()
		{
			Validator = new ValidationHelper();

			NotifyDataErrorInfoAdapter = new NotifyDataErrorInfoAdapter(Validator);
		    NotifyDataErrorInfoAdapter.ErrorsChanged += OnErrorsChanged;
		}

        private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            // Notify the UI that the property has changed so that the validation error gets displayed (or removed).
            RaisePropertyChanged(e.PropertyName);
        }

        Task<ValidationResult> IValidatable.Validate()
		{
			return Validator.ValidateAllAsync();
		}

        #region Implementation of INotifyDataErrorInfo

        public IEnumerable GetErrors(string propertyName)
        {
            return NotifyDataErrorInfoAdapter.GetErrors(propertyName);
        }

        public bool HasErrors => NotifyDataErrorInfoAdapter.HasErrors;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
        {
            add { NotifyDataErrorInfoAdapter.ErrorsChanged += value; }
            remove { NotifyDataErrorInfoAdapter.ErrorsChanged -= value; }
        }

        #endregion
    }
}