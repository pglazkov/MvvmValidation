using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Threading;
using Xunit;

namespace MvvmValidation.Tests.Helpers
{
    public static class TestUtils
    {
        public static void ExecuteWithDispatcher(Action<Dispatcher, Action> testBodyDelegate,
            int timeoutMilliseconds = 20000, string timeoutMessage = "Test did not complete in the spefied timeout")
        {
            var uiThreadDispatcher = Dispatcher.CurrentDispatcher;
            //ThreadingHelpers.UISynchronizationContext = new DispatcherSynchronizationContext(uiThreadDispatcher);

            var frame = new DispatcherFrame();

            // Set-up timer that will call Fail if the test is not completed in specified timeout

            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            if (Debugger.IsAttached)
            {
                timeout = TimeSpan.FromDays(1);
            }

            Observable.Timer(timeout)
                .Subscribe(
                    _ => { uiThreadDispatcher.BeginInvoke(new Action(() => { Assert.True(false, timeoutMessage); })); });


            // Shedule the test body with current dispatcher (UI thread)
            uiThreadDispatcher.BeginInvoke(
                new Action(() => { testBodyDelegate(uiThreadDispatcher, () => { frame.Continue = false; }); }));

            // Run the dispatcher loop that will execute the above logic
            Dispatcher.PushFrame(frame);
        }
    }
}