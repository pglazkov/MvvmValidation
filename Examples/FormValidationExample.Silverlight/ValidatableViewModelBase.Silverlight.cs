using System;
using System.Collections;
using System.ComponentModel;

namespace FormValidationExample
{
	public partial class ValidatableViewModelBase : INotifyDataErrorInfo
	{
		public IEnumerable GetErrors(string propertyName)
		{
			return NotifyDataErrorInfoAdapter.GetErrors(propertyName);
		}

		public bool HasErrors
		{
			get { return NotifyDataErrorInfoAdapter.HasErrors; }
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
		{
			add { NotifyDataErrorInfoAdapter.ErrorsChanged += value; }
			remove { NotifyDataErrorInfoAdapter.ErrorsChanged -= value; }
		}
	}
}