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

		Task<ValidationResult> IValidatable.Validate()
		{
			return Validator.ValidateAllAsync();
		}
	}
}