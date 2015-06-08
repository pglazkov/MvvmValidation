using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	/// <summary>
	/// Provides a possibility to comibine multiple instances of <see cref="ValidationHelper"/> into one scope and 
	/// execute the validation in one go with multiple validators. Useful when validating multiple view models that are
	/// not aware of each other and know only about the scope. 
	/// </summary>
	public sealed class ValidationScope
	{
		private readonly IList<ValidationHelper> registeredValidators = new List<ValidationHelper>();
		private readonly IDictionary<ValidationHelper, ValidationResult> resultsByValidator = new Dictionary<ValidationHelper, ValidationResult>();

		/// <summary>
		/// Occurs when validation result changes.
		/// </summary>
		public event EventHandler<ValidationResultChangedEventArgs> ResultChanged;

		private void OnResultChanged(ValidationResultChangedEventArgs e)
		{
			var handler = ResultChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// Registers a validator with this scope. 
		/// </summary>
		/// <param name="validator">Validator to register.</param>
		public void RegisterValidator([NotNull] ValidationHelper validator)
		{
			Guard.NotNull(validator, () => validator);

			registeredValidators.Add(validator);
			resultsByValidator.Add(validator, ValidationResult.Valid);

			validator.ResultChanged += OnValidatorResultChanged;
		}

		private void OnValidatorResultChanged(object sender, ValidationResultChangedEventArgs e)
		{
			var validator = (ValidationHelper)sender;

			resultsByValidator[validator] = e.NewResult;

			NotifyCombinedResultChanged();
		}

		private void NotifyCombinedResultChanged()
		{
			var combinedResult = GetResult();

			OnResultChanged(new ValidationResultChangedEventArgs(null, combinedResult));
		}

		/// <summary>
		/// Executes the validation of all registered validators and combines the result from all of them.
		/// </summary>
		/// <returns>The validation result.</returns>
		public Task<ValidationResult> ValidateAll()
		{
			return
				TaskEx.WhenAll(registeredValidators.Select(x => x.ValidateAllAsync()).ToList()).ContinueWith(t => CombineResults(t.Result));
		}

		/// <summary>
		/// Gets the result of last validation (without executing the validation).
		/// </summary>
		/// <returns>The result of last validation.</returns>
		public ValidationResult GetResult()
		{
			return CombineResults(resultsByValidator.Values);
		}

		private static ValidationResult CombineResults(IEnumerable<ValidationResult> results)
		{
			ValidationResult result = ValidationResult.Valid;

			foreach (var r in results)
			{
				result = result.Combine(r);
			}

			return result;
		}
	}
}