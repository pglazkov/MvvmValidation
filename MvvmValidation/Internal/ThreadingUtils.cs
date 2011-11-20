using System;

namespace MvvmValidation.Internal
{
	internal static class ThreadingUtils
	{
		public static void RunOnUI(Action action)
		{
			if (!CurrentDispatcher.Instance.CheckAccess())
			{
				CurrentDispatcher.Instance.BeginInvoke(action);
			}
			else
			{
				action();
			}
		}
	}
}