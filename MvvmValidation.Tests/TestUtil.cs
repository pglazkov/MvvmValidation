using System;
using System.Reactive.Linq;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmValidation.Internal;

namespace MvvmValidation.Tests
{
	public static class TestUtils
	{
		public static void ExecuteWithDispatcher(Action<Dispatcher, Action> testBodyDelegate, int timeoutMilliseconds = 20000, string timeoutMessage = "Test did not complete in the spefied timeout")
		{
			var uiThreadDispatcher = Dispatcher.CurrentDispatcher;
			CurrentDispatcher.Instance = uiThreadDispatcher;

			var frame = new DispatcherFrame();

			// Set-up timer that will call Fail if the test is not completed in specified timeout
			Observable.Timer(TimeSpan.FromMilliseconds(timeoutMilliseconds)).Subscribe(_ =>
			{
				uiThreadDispatcher.BeginInvoke(new Action(() =>
				{
					Assert.Fail(timeoutMessage);
				}));
			});


			// Shedule the test body with current dispatcher (UI thread)
			uiThreadDispatcher.BeginInvoke(new Action(() =>
			{
				testBodyDelegate(uiThreadDispatcher, () => { frame.Continue = false; });
			}));

			// Run the dispatcher loop that will execute the above logic
			Dispatcher.PushFrame(frame);
		}
	}
}