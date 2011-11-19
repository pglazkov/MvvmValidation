using System;
using System.Windows;
using System.Windows.Threading;

namespace MvvmValidation
{
	public static class CurrentDispatcher
	{
		private static Dispatcher currentDispatcher;

		public static Dispatcher Instance
		{
			get
			{
				if (currentDispatcher == null)
				{
#if !SILVERLIGHT
					currentDispatcher = Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher;
#else
					currentDispatcher = Application.Current.RootVisual.Dispatcher;
#endif
				}

				return currentDispatcher;
			}
			set { currentDispatcher = value; }
		}
	}
}