using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	public class DataErrorInfoAdapter : IDataErrorInfo
	{
		public DataErrorInfoAdapter(ValidationHelper validator)
		{
			Contract.Requires(validator != null);

			Validation = validator;
		}

		private ValidationHelper Validation { get; set; }

		#region IDataErrorInfo Members

		public string this[string columnName]
		{
			get { return Validation.GetResult(columnName).ErrorList.ToString(); }
		}

		public string Error
		{
			get { return Validation.GetResult().ErrorList.ToString(); }
		}

		#endregion
	}
}