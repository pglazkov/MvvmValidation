using System;

namespace MvvmValidation
{
	public delegate void AsyncRuleValidateCallback(Action<RuleValidationResult> resultCallback);
}