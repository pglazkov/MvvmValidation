using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MvvmValidation
{
	public static class RuleValidationResultExtensions
	{
		public static RuleValidationResult Combine(this RuleValidationResult r1, RuleValidationResult r2)
		{
			Contract.Requires(r1 != null);
			Contract.Requires(r2 != null);

			var result = new RuleValidationResult();

			foreach (var error in r1.Errors)
			{
				result.AddError(error);
			}

			foreach (var error in r2.Errors)
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