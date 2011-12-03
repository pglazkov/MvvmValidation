using System;
using System.ComponentModel;
using System.Linq.Expressions;
using MvvmValidation.Internal;

namespace MvvmValidation.Tests.Fakes
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