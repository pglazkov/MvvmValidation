using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	[ContractClass(typeof(IValidationTargetContract))]
	internal interface IValidationTarget
	{
		IEnumerable<object> UnwrapTargets();

		bool IsMatch(object target);
	}
}