using System.Threading;

namespace MvvmValidation.Internal
{
	internal static class ThreadingHelpers
	{
		private static SynchronizationContext uiSynchronizationContext;

		public static SynchronizationContext UISynchronizationContext
		{
			get { return uiSynchronizationContext ?? SynchronizationContext.Current; }
			set { uiSynchronizationContext = value; }
		}
	}
}