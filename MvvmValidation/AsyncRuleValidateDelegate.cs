using System;

namespace MvvmValidation
{
	public delegate void AsyncRuleValidateDelegate(Action<RuleValidationResult> resultCallback);
}