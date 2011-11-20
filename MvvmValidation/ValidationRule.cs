using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	internal class ValidationRule
	{
		public ValidationRule(IValidationTarget target, Func<RuleValidationResult> validateDelegate,
		                        AsyncRuleValidateCallback asyncValidateCallback)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateDelegate != null || asyncValidateCallback != null);

			Target = target;
			ValidateDelegate = validateDelegate;
			AsyncValidateCallback = asyncValidateCallback ?? (completed => completed(ValidateDelegate()));
		}

		private AsyncRuleValidateCallback AsyncValidateCallback { get; set; }
		private Func<RuleValidationResult> ValidateDelegate { get; set; }

		public bool SupportsAsyncValidation
		{
			get { return AsyncValidateCallback != null || AsyncValidateCallback != null; }
		}

		public bool SupportsSyncValidation
		{
			get { return ValidateDelegate != null; }
		}

		public IValidationTarget Target { get; private set; }

		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EvaluateAsync")]
		public RuleValidationResult Evaluate()
		{
			if (!SupportsSyncValidation)
			{
				throw new NotSupportedException(
					"Synchronous validation is not supported by this rule. Method EvaluateAsync must be called instead.");
			}

			RuleValidationResult result = ValidateDelegate();

			return result;
		}

		public void EvaluateAsync(Action<RuleValidationResult> completed)
		{
			Contract.Requires(completed != null);

			if (AsyncValidateCallback != null)
			{
				AsyncValidateCallback(completed);
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