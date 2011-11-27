namespace FormValidationExample
{
	public partial class ValidatableViewModelBase
	{
		partial void OnCreated()
		{
			HookUpValidationNotification();
		}

		private void HookUpValidationNotification()
		{
			// Due to limitation of IDataErrorInfo, in WPF we need to explicitly indicated that something has changed
			// about the property in order for the framework to look for errors for the property.
			Validator.ResultChanged += (o, e) =>
			{
				var propertyName = e.Target as string;

				if (!string.IsNullOrEmpty(propertyName))
				{
					RaisePropertyChanged(propertyName);
				}
			};
		}
	}
}