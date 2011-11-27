using System;
using System.Windows.Controls;

namespace FormValidationExample
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();

			DataContext = new MainViewModel();
		}
	}
}
