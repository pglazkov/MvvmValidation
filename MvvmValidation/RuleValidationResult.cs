using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	/// <summary>
	/// Represents the outcome of a validation rule when executed.
	/// </summary>
	public class RuleValidationResult
	{
		private readonly IList<string> errors;

		#region Factory Methods

		/// <summary>
		/// Creates an "Invalid" result with the given error <paramref name="error"/>.
		/// </summary>
		/// <param name="error">The error text that describes why this rule is invalid.</param>
		/// <returns>An instance of <see cref="RuleValidationResult"/> that represents an invalid result.</returns>
		public static RuleValidationResult Invalid(string error)
		{
			return new RuleValidationResult(error);
		}

		/// <summary>
		/// Creates a "Valid" result.
		/// </summary>
		/// <returns>An instance of <see cref="RuleValidationResult"/> that represents a valid outcome of the rule.</returns>
		public static RuleValidationResult Valid()
		{
			return new RuleValidationResult();
		}

		/// <summary>
		/// Asseses the specified assertion and if <c>false</c> then creates and invalid result with the given <paramref name="errorMessage"/>. 
		/// If <c>true</c>, returns a valid result.
		/// </summary>
		/// <param name="condition">The assertion.</param>
		/// <param name="errorMessage">The error message in case if the <paramref name="condition"/> is not <c>true</c>.</param>
		/// <returns>An instance of <see cref="RuleValidationResult"/> that represents the result of the assertion.</returns>
		public static RuleValidationResult Assert(bool condition, string errorMessage)
		{
			if (!condition)
			{
				return Invalid(errorMessage);
			}

			return Valid();
		}

		#endregion

		/// <summary>
		/// Creates an empty (valid) instance of <see cref="RuleValidationResult"/>. 
		/// The <see cref="AddError"/> method can be used to add errors to the result later.
		/// </summary>
		public RuleValidationResult()
			: this(true, new string[] {})
		{
		}

		private RuleValidationResult(string error)
			: this(false, new[] {error})
		{
			Contract.Requires(!string.IsNullOrEmpty(error));
		}

		private RuleValidationResult(bool isValid, IEnumerable<string> errors)
		{
			Contract.Requires(errors != null);

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
		public IEnumerable<string> Errors
		{
			get { return errors; }
		}

		/// <summary>
		/// Adds an error to the result.
		/// </summary>
		/// <param name="error">The error message to add.</param>
		public void AddError(string error)
		{
			Contract.Requires(!string.IsNullOrEmpty(error));

			errors.Add(error);
			IsValid = false;
		}
	}
}