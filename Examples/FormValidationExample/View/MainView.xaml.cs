using System;
using System.Windows.Controls;
using FormValidationExample.Services;
using FormValidationExample.ViewModel;

namespace FormValidationExample.View
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
