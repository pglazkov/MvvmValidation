using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation
{
	public class ValidationResult
	{
		internal static ValidationResult Valid
		{
			get { return new ValidationResult(); }
		}

		public object Target { get; private set; }
		public ValidationErrorCollection ErrorList { get; private set; }

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

		public bool IsValid
		{
			get { return !ErrorList.Any(); }
		}

		public string this[object target]
		{
			get
			{
				var firstErrorForTarget = ErrorList.Where(e => e.Target == target).FirstOrDefault();

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

		public string Format(IValidationResultFormatter formatter)
		{
			Contract.Requires(formatter != null);

			var result = formatter.Format(this);

			return result;
		}

		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			var result = Format(new NumberedListValidationResultFormatter());

			return !string.IsNullOrEmpty(result) ? result : "Valid";
		}

		public ValidationResult MergeWith(ValidationResult validationResult)
		{
			var result = new ValidationResult();
			result.Target = Target;

			foreach (var error in ErrorList)
			{
				result.AddError(error.Target, error.ErrorText);
			}

			foreach (var error in validationResult.ErrorList)
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