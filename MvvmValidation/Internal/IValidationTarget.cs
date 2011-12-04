using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MvvmValidation.Internal
{
	[ContractClass(typeof(IValidationTargetContract))]
	internal interface IValidationTarget
	{
		IEnumerable<object> UnwrapTargets();

		bool IsMatch(object target);
	}
}