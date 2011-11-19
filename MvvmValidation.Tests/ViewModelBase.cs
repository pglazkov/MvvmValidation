using System;
using System.ComponentModel;
using System.Linq.Expressions;
using MvvmValidation;

namespace ZyLAB.EDiscovery.Common.Presentation.Test.Validation.V2
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(Expression<Func<object>> propertyExpression)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(PropertyName.For(propertyExpression)));
			}
		}
	}
}