using System.ComponentModel;

namespace FormValidationExample
{
	public partial class ValidatableViewModelBase : IDataErrorInfo
	{
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