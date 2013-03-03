using System;

namespace MvvmValidation.Internal
{
	internal sealed class DelegateDisposable : IDisposable
	{
		public DelegateDisposable(Action restoreStateDelegate)
		{
			RestoreStateDelegate = restoreStateDelegate;
		}

		private Action RestoreStateDelegate { get; set; }

		#region IDisposable Members

		public void Dispose()
		{
			RestoreStateDelegate();
		}

		#endregion
	}
}