using System.Linq;
using JetBrains.Annotations;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	/// <summary>
	/// Contains helper extension methods for working with <see cref="RuleResult"/>.
	/// </summary>
	public static class RuleResultExtensions
	{
		/// <summary>
		/// Merges <paramref name="firstRuleResult"/> with given <paramref name="secondRuleResult"/> and returns a new instance of <see cref="ValidationResult"/>
		/// that represents the merged result (the result that contains errors from both results whithout duplicates).
		/// </summary>
		/// <param name="firstRuleResult">The first validation result to merge.</param>
		/// <param name="secondRuleResult">The second validation result to merge.</param>
		/// <returns>A new instance of <see cref="RuleResult"/> that represents the merged result (the result that contains errors from both results whithout duplicates).</returns>
		[NotNull]
		public static RuleResult Combine([NotNull] this RuleResult firstRuleResult, [NotNull] RuleResult secondRuleResult)
		{
			Guard.NotNull(firstRuleResult, nameof(firstRuleResult));
			Guard.NotNull(secondRuleResult, nameof(secondRuleResult));

			var result = new RuleResult();

			foreach (string error in firstRuleResult.Errors)
			{
				result.AddError(error);
			}

			foreach (string error in secondRuleResult.Errors)
			{
				if (!result.Errors.Contains(error))
				{
					result.AddError(error);
				}
			}

			return result;
		}
	}
}