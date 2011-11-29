using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FormValidationExample.Infrastructure;

namespace FormValidationExample
{
	public class InterestSelectorViewModel : ValidatableViewModelBase
	{
		public InterestSelectorViewModel()
		{
			Interests = new ObservableCollection<InterestItemViewModel>
			{
				new InterestItemViewModel("Music", this),
				new InterestItemViewModel("Movies", this),
				new InterestItemViewModel("Sports", this),
				new InterestItemViewModel("Shopping", this),
				new InterestItemViewModel("Hunting", this),
				new InterestItemViewModel("Books", this),
				new InterestItemViewModel("Physics", this),
				new InterestItemViewModel("Comics", this)
			};
		}

		public IEnumerable<InterestItemViewModel> Interests { get; private set; }

		public IEnumerable<InterestItemViewModel> SelectedInterests
		{
			get { return Interests.Where(i => i.IsSelected).ToArray(); }
		}

		#region SelectedInterestsChanged Event

		public event EventHandler SelectedInterestsChanged;

		private void OnSelectedInterestsChanged()
		{
			EventHandler handler = SelectedInterestsChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		} 

		#endregion

		public void NotifyInterestSelectionChanged()
		{
			OnSelectedInterestsChanged();
		}
	}
}