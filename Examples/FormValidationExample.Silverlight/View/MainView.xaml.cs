using System;
using System.Windows.Controls;
using FormValidationExample.Services;

namespace FormValidationExample
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();

			DataContext = new MainViewModel(new UserRegistrationService());
		}
	}
}
