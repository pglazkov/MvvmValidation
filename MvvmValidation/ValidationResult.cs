using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation
{
	public class ValidationResult
	{
		private ValidationResult() : this(null, new ValidationErrorCollection())
		{
		}

		internal ValidationResult(object target)
			: this(target, new ValidationErrorCollection())
		{
		}

		internal ValidationResult(object target, IEnumerable<string> errors)
			: this(target, new ValidationErrorCollection(errors.Select(e => new ValidationError(e, target)).ToList()))
		{
		}

		private ValidationResult(object target, ValidationErrorCollection errors)
		{
			Target = target;
			ErrorList = errors;
		}

		internal static ValidationResult Valid
		{
			get { return new ValidationResult(); }
		}

		public object Target { get; private set; }
		public ValidationErrorCollection ErrorList { get; private set; }

		public bool IsValid
		{
			get { return !ErrorList.Any(); }
		}

		public string this[object target]
		{
			get
			{
				ValidationError firstErrorForTarget = ErrorList.Where(e => e.Target == target).FirstOrDefault();

				if (firstErrorForTarget != null)
				{
					return firstErrorForTarget.ErrorText;
				}

				return null;
			}
		}

		private void AddError(ValidationError error)
		{
			Contract.Requires(error != null);

			ErrorList.Add(error);
		}

		internal void AddError(object target, string error)
		{
			AddError(new ValidationError(error, target));
		}

		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public string Format(IValidationResultFormatter formatter)
		{
			Contract.Requires(formatter != null);

			string result = formatter.Format(this);

			return result;
		}

		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			string result = Format(new NumberedListValidationResultFormatter());

			return !string.IsNullOrEmpty(result) ? result : "Valid";
		}

		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ValidationResult MergeWith(ValidationResult validationResult)
		{
			Contract.Requires(validationResult != null);

			var result = new ValidationResult();
			result.Target = Target;

			foreach (ValidationError error in ErrorList)
			{
				result.AddError(error.Target, error.ErrorText);
			}

			foreach (ValidationError error in validationResult.ErrorList)
			{
				if (result.ErrorList.Contains(error))
				{
					continue;
				}

				result.AddError(error.Target, error.ErrorText);
			}

			return result;
		}
	}
}