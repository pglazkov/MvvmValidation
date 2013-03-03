using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	/// <summary>
	/// Contains helper extension methods for working with <see cref="ValidationResult"/>.
	/// </summary>
	public static class ValidationResultExtensions
	{
		/// <summary>
		/// Merges <paramref name="firstResult"/> with given <paramref name="secondResult"/> and returns a new instance of <see cref="ValidationResult"/>
		/// that represents the merged result (the result that contains errors from both results whithout duplicates).
		/// </summary>
		/// <param name="firstResult">The first validation result to merge.</param>
		/// <param name="secondResult">The second validation result to merge.</param>
		/// <returns>A new instance of <see cref="ValidationResult"/> that represents the merged result (the result that contains errors from both results whithout duplicates).</returns>
		public static ValidationResult Combine(this ValidationResult firstResult, ValidationResult secondResult)
		{
			Contract.Requires(secondResult != null);

			var result = new ValidationResult();

			foreach (ValidationError error in firstResult.ErrorList)
			{
				result.AddError(error.Target, error.ErrorText);
			}

			foreach (ValidationError error in secondResult.ErrorList)
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