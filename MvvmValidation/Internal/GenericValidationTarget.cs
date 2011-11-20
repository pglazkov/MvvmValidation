using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MvvmValidation.Internal
{
	internal class GenericValidationTarget : IValidationTarget, IEquatable<GenericValidationTarget>
	{
		public GenericValidationTarget(object targetId)
		{
			Contract.Requires(targetId != null);

			TargetId = targetId;
		}

		public object TargetId { get; set; }

		#region IEquatable<GenericValidationTarget> Members

		public bool Equals(GenericValidationTarget other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other.TargetId, TargetId);
		}

		#endregion

		#region IValidationTarget Members

		public IEnumerable<object> UnwrapTargets()
		{
			return new[] {TargetId};
		}

		public bool IsMatch(object target)
		{
			return Equals(target, TargetId);
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
			if (obj.GetType() != typeof(GenericValidationTarget))
			{
				return false;
			}
			return Equals((GenericValidationTarget)obj);
		}

		public override int GetHashCode()
		{
			return (TargetId != null ? TargetId.GetHashCode() : 0);
		}
	}
}