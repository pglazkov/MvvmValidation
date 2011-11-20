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
						foreach (var error in e.ValidationResult.ErrorList)
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

		public IEnumerable GetErrors(string propertyName)
		{
			return Validator.GetLastValidationResult(propertyName).ErrorList.Select(er => er.ToString());
		}

		public bool HasErrors
		{
			get { return !Validator.GetLastValidationResult().IsValid; }
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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