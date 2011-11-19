using System;
using System.Collections.Generic;

namespace MvvmValidation
{
	internal class UndefinedValidationTarget : IValidationTarget
	{
		private static readonly object FakeTarget = new object();

		public IEnumerable<object> UnwrapTargets()
		{
			return new[] {FakeTarget};
		}

		public bool IsMatch(object target)
		{
			return ReferenceEquals(target, null);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return true;
			}

			if (obj is UndefinedValidationTarget)
			{
				return true;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}