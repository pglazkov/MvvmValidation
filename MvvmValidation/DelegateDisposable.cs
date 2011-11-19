using System;

namespace MvvmValidation
{
	public sealed class DelegateDisposable : IDisposable
	{
		public DelegateDisposable(Action restoreStateDelegate)
		{
			RestoreStateDelegate = restoreStateDelegate;
		}

		private Action RestoreStateDelegate { get; set; }

		public void Dispose()
		{
			RestoreStateDelegate();
		}
	}
}