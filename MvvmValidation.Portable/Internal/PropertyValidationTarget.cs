using System;
using System.Collections.Generic;

namespace MvvmValidation.Internal
{
	internal class PropertyValidationTarget : IValidationTarget, IEquatable<PropertyValidationTarget>
	{
		public PropertyValidationTarget(string propertyName)
		{
			Guard.NotNullOrEmpty(propertyName, nameof(propertyName));

			PropertyName = propertyName;
		}

		private string PropertyName { get; set; }

		#region IEquatable<PropertyValidationTarget> Members

		public bool Equals(PropertyValidationTarget other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other.PropertyName, PropertyName);
		}

		#endregion

		#region IValidationTarget Members

		public IEnumerable<object> UnwrapTargets()
		{
			return new[] {PropertyName};
		}

		public bool IsMatch(object target)
		{
			return Equals(PropertyName, target);
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(PropertyValidationTarget))
			{
				return false;
			}
			return Equals((PropertyValidationTarget)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (PropertyName != null ? PropertyName.GetHashCode() : 0);
			}
		}
	}
}