using System;
using System.Diagnostics;
using System.Windows;
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
