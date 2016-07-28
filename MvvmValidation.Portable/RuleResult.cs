using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	/// <summary>
	/// Represents the outcome of a validation rule when executed.
	/// </summary>
	public class RuleResult : IEquatable<RuleResult>
	{
		private readonly IList<string> errors;

		#region Factory Methods

		/// <summary>
		/// Creates an "Invalid" result with the given error <paramref name="error"/>.
		/// </summary>
		/// <param name="error">The error text that describes why this rule is invalid.</param>
		/// <returns>An instance of <see cref="RuleResult"/> that represents an invalid result.</returns>
		[NotNull]
		public static RuleResult Invalid([NotNull] string error)
		{
			Guard.NotNullOrEmpty(error, nameof(error));

			return new RuleResult(error);
		}

		/// <summary>
		/// Creates a "Valid" result.
		/// </summary>
		/// <returns>An instance of <see cref="RuleResult"/> that represents a valid outcome of the rule.</returns>
		[NotNull]
		public static RuleResult Valid()
		{
			return new RuleResult();
		}

		/// <summary>
		/// Asserts the specified condition and if <c>false</c> then creates and invalid result with the given <paramref name="errorMessage"/>. 
		/// If <c>true</c>, returns a valid result.
		/// </summary>
		/// <param name="condition">The assertion.</param>
		/// <param name="errorMessage">The error message in case if the <paramref name="condition"/> is not <c>true</c>.</param>
		/// <returns>An instance of <see cref="RuleResult"/> that represents the result of the assertion.</returns>
		[NotNull]
		public static RuleResult Assert(bool condition, [NotNull] string errorMessage)
		{
			Guard.NotNullOrEmpty(errorMessage, nameof(errorMessage));

			if (!condition)
			{
				return Invalid(errorMessage);
			}

			return Valid();
		}

		#endregion

		/// <summary>
		/// Creates an empty (valid) instance of <see cref="RuleResult"/>. 
		/// The <see cref="AddError"/> method can be used to add errors to the result later.
		/// </summary>
		public RuleResult()
			: this(true, new string[] {})
		{
		}

		private RuleResult(string error)
			: this(false, new[] {error})
		{
			Guard.NotNullOrEmpty(error, nameof(error));
		}

		private RuleResult(bool isValid, IEnumerable<string> errors)
		{
			Guard.NotNull(errors, nameof(errors));

			IsValid = isValid;

			this.errors = new List<string>(errors);
		}

		/// <summary>
		/// Gets a value indicating whether the validation rule passed (valid).
		/// </summary>
		public bool IsValid { get; private set; }

		/// <summary>
		/// Gets the error messages in case if the target is invalid according to this validation rule.
		/// </summary>
		[NotNull]
		public IEnumerable<string> Errors
		{
			get
			{
				return errors;
			}
		}

		/// <summary>
		/// Adds an error to the result.
		/// </summary>
		/// <param name="error">The error message to add.</param>
		public void AddError([NotNull] string error)
		{
			Guard.NotNullOrEmpty(error, nameof(error));

			errors.Add(error);
			IsValid = false;
		}

		#region Equality Members

		#region IEquatable<RuleResult> Members

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(RuleResult other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return other.errors.ItemsEqual(errors) && other.IsValid.Equals(IsValid);
		}

		#endregion

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
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
			if (obj.GetType() != typeof(RuleResult))
			{
				return false;
			}
			return Equals((RuleResult)obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return ((errors != null ? errors.GetHashCode() : 0) * 397) ^ IsValid.GetHashCode();
			}
		} 

		#endregion
	}
}