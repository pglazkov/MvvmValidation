using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation
{
	internal class PropertyCollectionValidationTarget : IValidationTarget
	{
		private IEnumerable<string> Properties { get; set; }

		public PropertyCollectionValidationTarget(IEnumerable<string> properties)
		{
			Contract.Requires(properties != null);
			Contract.Requires(properties.Any());

			Properties = properties;
		}

		public IEnumerable<object> UnwrapTargets()
		{
			return Properties.ToArray();
		}

		public bool IsMatch(object target)
		{
			return Properties.Any(p => Equals(p, target));
		}
	}
}