using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MvvmValidation.Internal
{
	[ContractClassFor(typeof(IValidationTarget))]
	// ReSharper disable InconsistentNaming
	internal abstract class IValidationTargetContract : IValidationTarget
		// ReSharper restore InconsistentNaming
	{
		#region IValidationTarget Members

		public IEnumerable<object> UnwrapTargets()
		{
			Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);

			throw new NotImplementedException();
		}

		public bool IsMatch(object target)
		{
			Contract.Requires(target != null);

			throw new NotImplementedException();
		}

		#endregion
	}
}