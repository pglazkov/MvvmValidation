using System.Collections.Generic;

namespace MvvmValidation.Internal
{
	internal interface IValidationTarget
	{
		IEnumerable<object> UnwrapTargets();

		bool IsMatch(object target);
	}
}