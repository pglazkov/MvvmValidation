using System;
using System.Threading.Tasks;

namespace MvvmValidation
{
	/// <summary>
	/// Represents a method that takes a callback method for setting rule validation result as a parameter. 
	/// </summary>
	/// <param name="resultCallback">A continuation callback that should be called when the rule validation result is available.</param>
	public delegate void AsyncRuleValidateAction(Action<RuleResult> resultCallback);

	internal static class AsyncRuleValdateActionExceptions
	{
		public static Func<Task<RuleResult>> ToTaskFunc(this AsyncRuleValidateAction action)
		{
			if (action == null)
			{
				return null;
			}

			return () =>
			{
				var tcs = new TaskCompletionSource<RuleResult>();

				try
				{
					action(result => tcs.TrySetResult(result));
				}
				catch (Exception e)
				{
					if (!tcs.TrySetException(e))
					{
						throw;
					}
				}

				return tcs.Task;
			};
		}
	}
}