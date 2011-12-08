using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FormValidationExample.Infrastructure;
using MvvmValidation;

namespace FormValidationExample.ViewModel
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

			ConfigureValidationRules();
		}

		public IEnumerable<InterestItemViewModel> Interests { get; private set; }

		public IEnumerable<InterestItemViewModel> SelectedInterests
		{
			get { return Interests.Where(i => i.IsSelected).ToArray(); }
		}

		private void ConfigureValidationRules()
		{
			Validator.AddRule(() => RuleResult.Assert(SelectedInterests.Count() >= 3, "Please select at least 3 interests"));
		}

		public void OnInterestSelectionChanged()
		{
			OnSelectedInterestsChanged();
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

	}
}