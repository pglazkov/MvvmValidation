using System;
using System.Windows;

namespace MvvmValidation.Internal
{
	internal static class ThreadingUtils
	{
		public static void RunOnUI(Action action)
		{
			var disp = Deployment.Current.Dispatcher;

			if (disp.CheckAccess())
			{
				action();
			}
			else
			{
				disp.BeginInvoke(action);
			}
		}
	}
}