using System;
using System.Diagnostics.Contracts;
using System.Threading;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	internal class ValidationRule
	{
		public ValidationRule(IValidationTarget target, Func<RuleValidationResult> validateDelegate,
		                        AsyncRuleValidateDelegate asyncValidateDelegate)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateDelegate != null || asyncValidateDelegate != null);

			Target = target;
			ValidateDelegate = validateDelegate;
			AsyncValidateDelegate = asyncValidateDelegate ?? (completed => completed(ValidateDelegate()));
		}

		private AsyncRuleValidateDelegate AsyncValidateDelegate { get; set; }
		private Func<RuleValidationResult> ValidateDelegate { get; set; }

		public bool SupportsAsyncValidation
		{
			get { return AsyncValidateDelegate != null || AsyncValidateDelegate != null; }
		}

		public bool SupportsSyncValidation
		{
			get { return ValidateDelegate != null; }
		}

		public IValidationTarget Target { get; private set; }

		public RuleValidationResult Evaluate()
		{
			if (!SupportsSyncValidation)
			{
				throw new NotSupportedException(
					"Synchronous validation is not supported by this rule. EvaluateAsync must be called instead.");
			}

			RuleValidationResult result = ValidateDelegate();

			return result;
		}

		public void EvaluateAsync(Action<RuleValidationResult> completed)
		{
			Contract.Requires(completed != null);

			if (AsyncValidateDelegate != null)
			{
				AsyncValidateDelegate(completed);
			}
			else
			{
				ThreadPool.QueueUserWorkItem(_ =>
				{
					RuleValidationResult result = Evaluate();
					completed(result);
				});
			}
		}
	}
}