using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Threading;

namespace MvvmValidation.Internal
{
	internal static class CurrentDispatcher
	{
		private static Dispatcher currentDispatcher;

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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