using System;

namespace MvvmValidation
{
	/// <summary>
	/// Represents a method that takes a callback method for setting rule validation result as a parameter. 
	/// </summary>
	/// <param name="resultCallback">A continuation callback that should be called when the rule validation result is available.</param>
	public delegate void AsyncRuleValidateAction(Action<RuleValidationResult> resultCallback);
}