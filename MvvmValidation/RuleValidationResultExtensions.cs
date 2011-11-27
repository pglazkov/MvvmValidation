using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation
{
	public static class RuleValidationResultExtensions
	{
		public static RuleValidationResult Combine(this RuleValidationResult firstRuleResult,
												   RuleValidationResult secondRuleResult)
		{
			Contract.Requires(firstRuleResult != null);
			Contract.Requires(secondRuleResult != null);

			var result = new RuleValidationResult();

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