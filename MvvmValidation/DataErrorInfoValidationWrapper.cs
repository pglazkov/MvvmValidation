using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	public class DataErrorInfoValidationWrapper : IDataErrorInfo
	{
		public DataErrorInfoValidationWrapper(ValidationHelper validator)
		{
			Contract.Requires(validator != null);

			Validation = validator;
		}

		private ValidationHelper Validation { get; set; }

		public string this[string columnName]
		{
			get { return Validation.GetLastValidationResult(columnName).ErrorList.ToString(); }
		}

		public string Error
		{
			get { return Validation.GetLastValidationResult().ErrorList.ToString(); }
		}
	}
}