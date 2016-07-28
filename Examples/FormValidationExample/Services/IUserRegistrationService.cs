using System;

namespace FormValidationExample.Services
{
	public interface IUserRegistrationService
	{
		IObservable<bool> IsUserNameAvailable(string userName);
	}
}