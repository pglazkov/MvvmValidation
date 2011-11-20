using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation
{
	public static class RuleValidationResultExtensions
	{
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), 
		 SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static RuleValidationResult Combine(this RuleValidationResult firstRuleResult, RuleValidationResult secondRuleResult)
		{
			Contract.Requires(firstRuleResult != null);
			Contract.Requires(secondRuleResult != null);

			var result = new RuleValidationResult();

			foreach (var error in firstRuleResult.Errors)
			{
				result.AddError(error);
			}

			foreach (var error in secondRuleResult.Errors)
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