using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace MvvmValidation.Internal
{
	internal class PropertyValidationTarget : IValidationTarget, IEquatable<PropertyValidationTarget>
	{
		private string PropertyName { get; set; }

		public PropertyValidationTarget(Expression<Func<object>> propertyNameExpression)
			: this((string)Internal.PropertyName.For(propertyNameExpression))
		{
		}

		private PropertyValidationTarget(string propertyName)
		{
			Contract.Requires(!string.IsNullOrEmpty(propertyName));

			PropertyName = propertyName;
		}

		public IEnumerable<object> UnwrapTargets()
		{
			return new[] { PropertyName };
		}

		public bool IsMatch(object target)
		{
			return Equals(PropertyName, target);
		}

		#region Equality Members

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

		#endregion
	}
}