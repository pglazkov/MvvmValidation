using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation
{
	public class NotifyDataErrorInfoValidationWrapper : INotifyDataErrorInfo
	{
		public NotifyDataErrorInfoValidationWrapper(ValidationHelper validator)
		{
			Contract.Requires(validator != null);

			Validator = validator;

			Validator.ValidationCompleted +=
				(o, e) =>
				{
					if (e.ValidationResult.ErrorList.Any())
					{
						foreach (ValidationError error in e.ValidationResult.ErrorList)
						{
							OnErrorsChanged(new DataErrorsChangedEventArgs(error.Target as string));
						}
					}
					else
					{
						OnErrorsChanged(new DataErrorsChangedEventArgs(e.ValidationResult.Target as string));
					}
				};
		}

		private ValidationHelper Validator { get; set; }

		#region INotifyDataErrorInfo Members

		public IEnumerable GetErrors(string propertyName)
		{
			return Validator.GetResult(propertyName).ErrorList.Select(er => er.ToString());
		}

		public bool HasErrors
		{
			get { return !Validator.GetResult().IsValid; }
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		#endregion

		private void OnErrorsChanged(DataErrorsChangedEventArgs e)
		{
			EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}
}