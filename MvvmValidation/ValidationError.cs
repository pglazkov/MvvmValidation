using System;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	public class ValidationError : IEquatable<ValidationError>
	{
		public ValidationError(string errorText, object target)
		{
			Contract.Requires(!string.IsNullOrEmpty(errorText));

			ErrorText = errorText;
			Target = target;
		}

		public string ErrorText { get; private set; }
		public object Target { get; private set; }

		#region Equality Members

		public bool Equals(ValidationError other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other.ErrorText, ErrorText) && Equals(other.Target, Target);
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
			if (obj.GetType() != typeof(ValidationError))
			{
				return false;
			}
			return Equals((ValidationError)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (ErrorText != null ? ErrorText.GetHashCode() : 0);
				result = (result * 397) ^ (Target != null ? Target.GetHashCode() : 0);
				result = (result * 397);
				return result;
			}
		}

		#endregion

		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return ErrorText;
		}

		public static implicit operator string(ValidationError error)
		{
			return error.ToString();
		}
	}
}