using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation.Internal
{
	internal class PropertyCollectionValidationTarget : IValidationTarget
	{
		public PropertyCollectionValidationTarget(IEnumerable<string> properties)
		{
			Guard.NotNull(properties != null, nameof(properties));
            Guard.Assert(properties.Any(), "properties.Any()");

			Properties = properties;
		}

		private IEnumerable<string> Properties { get; set; }

		#region IValidationTarget Members

		public IEnumerable<object> UnwrapTargets()
		{
			return Properties.ToArray();
		}

		public bool IsMatch(object target)
		{
			return Properties.Any(p => Equals(p, target));
		}

		#endregion
	}
}