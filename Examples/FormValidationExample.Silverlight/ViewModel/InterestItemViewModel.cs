using GalaSoft.MvvmLight;

namespace FormValidationExample
{
	public class InterestItemViewModel : ViewModelBase
	{
		private bool isSelected;

		public InterestItemViewModel(string name, InterestSelectorViewModel parentSelector)
		{
			Name = name;
			ParentSelector = parentSelector;
		}

		public string Name { get; private set; }
		private InterestSelectorViewModel ParentSelector { get; set; }

		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				RaisePropertyChanged("IsSelected");
				ParentSelector.OnInterestSelectionChanged();
			}
		}
	}
}