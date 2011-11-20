using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	public class RuleValidationResult
	{
		private readonly IList<string> errors;

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

		public bool IsValid { get; private set; }

		public IEnumerable<string> Errors
		{
			get { return errors; }
		}

		public static RuleValidationResult Invalid(string error)
		{
			return new RuleValidationResult(error);
		}

		public static RuleValidationResult Valid()
		{
			return new RuleValidationResult();
		}

		public static RuleValidationResult Assert(bool condition, string errorMessage)
		{
			if (!condition)
			{
				return Invalid(errorMessage);
			}

			return Valid();
		}

		public void AddError(string error)
		{
			Contract.Requires(!string.IsNullOrEmpty(error));

			errors.Add(error);
			IsValid = false;
		}
	}
}