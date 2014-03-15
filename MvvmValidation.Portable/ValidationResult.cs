using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;

namespace MvvmValidation
{
	/// <summary>
	/// Encapsulates result of a validation. Contains a boolean <see cref="IsValid"/> and a collection of errors <see cref="ErrorList"/>.
	/// </summary>
	public class ValidationResult
	{
		internal ValidationResult()
			: this(new ValidationErrorCollection())
		{
		}

		internal ValidationResult(object target, IEnumerable<string> errors)
			: this(new ValidationErrorCollection(errors.Select(e => new ValidationError(e, target)).ToList()))
		{
		}

		private ValidationResult(ValidationErrorCollection errors)
		{
			ErrorList = errors;
		}

		internal static ValidationResult Valid
		{
			get { return new ValidationResult(); }
		}

		/// <summary>
		/// Gets the list of errors if any. If valid, returns an empty collection.
		/// </summary>
		[NotNull]
		public ValidationErrorCollection ErrorList { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the validation was sucessful. If not, see <see cref="ErrorList"/> for the list of errors.
		/// </summary>
		public bool IsValid
		{
			get { return !ErrorList.Any(); }
		}

		/// <summary>
		/// Gets an error by <paramref name="target"/>, or <c>null</c> if valid.
		/// </summary>
		[CanBeNull]
		public string this[object target]
		{
			get
			{
				ValidationError firstErrorForTarget = ErrorList.FirstOrDefault(e => e.Target == target);

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

		/// <summary>
		/// Formats this instance to a string using given <see cref="IValidationResultFormatter"/>.
		/// </summary>
		/// <param name="formatter">The formatter that can format the validation result.</param>
		/// <returns>
		/// A string that represents this validation result.
		/// </returns>
		[NotNull]
		public string ToString([NotNull] IValidationResultFormatter formatter)
		{
			Contract.Requires(formatter != null);

			string result = formatter.Format(this);

			return result;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			var result = ToString(new NumberedListValidationResultFormatter());

			return result;
		}
	}
}