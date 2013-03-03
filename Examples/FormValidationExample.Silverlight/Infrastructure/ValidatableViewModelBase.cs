using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using MvvmValidation;

namespace FormValidationExample.Infrastructure
{
	public abstract partial class ValidatableViewModelBase : ViewModelBase, IValidatable
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

			OnCreated();
		}

		partial void OnCreated();

		Task<ValidationResult> IValidatable.Validate()
		{
			return Validator.ValidateAllAsync();
		}
	}
}