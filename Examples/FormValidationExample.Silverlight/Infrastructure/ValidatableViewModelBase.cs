using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using MvvmValidation;

namespace FormValidationExample.Infrastructure
{
	public partial class ValidatableViewModelBase : ViewModelBase, IDataErrorInfo
	{
		protected ValidationHelper Validator { get; private set; }

#if SILVERLIGHT 
		private NotifyDataErrorInfoAdapter NotifyDataErrorInfoAdapter { get; set; }
#endif
		private DataErrorInfoAdapter DataErrorInfoAdapter { get; set; }


		public ValidatableViewModelBase()
		{
			Validator = new ValidationHelper();

#if SILVERLIGHT
			NotifyDataErrorInfoAdapter = new NotifyDataErrorInfoAdapter(Validator);
#endif
			DataErrorInfoAdapter = new DataErrorInfoAdapter(Validator);
			OnCreated();
		}

		partial void OnCreated();

		public string this[string columnName]
		{
			get { return DataErrorInfoAdapter[columnName]; }
		}

		public string Error
		{
			get { return DataErrorInfoAdapter.Error; }
		}
	}
}