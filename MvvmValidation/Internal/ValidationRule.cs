using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;

namespace MvvmValidation.Internal
{
	internal class ValidationRule
	{
		public ValidationRule(IValidationTarget target, Func<RuleResult> validateDelegate,
		                      AsyncRuleValidateAction asyncValidateAction)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateDelegate != null || asyncValidateAction != null);

			Target = target;
			ValidateDelegate = validateDelegate;
			AsyncValidateAction = asyncValidateAction ?? (completed => completed(ValidateDelegate()));
		}

		private AsyncRuleValidateAction AsyncValidateAction { get; set; }
		private Func<RuleResult> ValidateDelegate { get; set; }

		public bool SupportsAsyncValidation
		{
			get { return AsyncValidateAction != null || AsyncValidateAction != null; }
		}

		public bool SupportsSyncValidation
		{
			get { return ValidateDelegate != null; }
		}

		public IValidationTarget Target { get; private set; }

		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EvaluateAsync")]
		public RuleResult Evaluate()
		{
			if (!SupportsSyncValidation)
			{
				throw new NotSupportedException(
					"Synchronous validation is not supported by this rule. Method EvaluateAsync must be called instead.");
			}

			RuleResult result = ValidateDelegate();

			return result;
		}

		public void EvaluateAsync(Action<RuleResult> completed)
		{
			Contract.Requires(completed != null);

			if (AsyncValidateAction != null)
			{
				AsyncValidateAction(completed);
			}
			else
			{
				ThreadPool.QueueUserWorkItem(_ =>
				{
					RuleResult result = Evaluate();
					completed(result);
				});
			}
		}
	}
}