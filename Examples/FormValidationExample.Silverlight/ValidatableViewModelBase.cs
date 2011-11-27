using GalaSoft.MvvmLight;
using MvvmValidation;

namespace FormValidationExample
{
	public partial class ValidatableViewModelBase : ViewModelBase
	{
		protected ValidationHelper Validator { get; private set; }

#if SILVERLIGHT 
		private NotifyDataErrorInfoAdapter NotifyDataErrorInfoAdapter { get; set; }
#else
		private DataErrorInfoAdapter DataErrorInfoAdapter { get; set; }
#endif

		public ValidatableViewModelBase()
		{
			Validator = new ValidationHelper();

#if SILVERLIGHT
			NotifyDataErrorInfoAdapter = new NotifyDataErrorInfoAdapter(Validator);
#else
			DataErrorInfoAdapter = new DataErrorInfoAdapter(Validator);
#endif
		}
	}
}