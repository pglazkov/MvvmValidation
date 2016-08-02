using System.Windows;
using System.Windows.Threading;
using MvvmValidation;

namespace FormValidationExample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Overrides of Application

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is ValidationException)
            {
                MessageBox.Show(MainWindow, e.Exception.ToString());
            }
        }

        #endregion
    }
}