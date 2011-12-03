using System;
using System.ComponentModel;

namespace MvvmValidation.Tests.Fakes
{
	public class MockViewModel : ViewModelBase, IDataErrorInfo
	{
		private int intProperty;
		private int rangeEnd;
		private int rangeStart;
		private string stringProperty;
		private string stringProperty2;

		public MockViewModel()
		{
			SetupValidation();

			StringProperty = "Default Valid Value";
			IntProperty = 10; // Default Valid value
		}

		public ValidationHelper Validation { get; set; }
		private DataErrorInfoAdapter DataErrorInfoValidationAdapter { get; set; }

		public string StringProperty
		{
			get { return stringProperty; }

			set
			{
				if (Equals(stringProperty, value))
				{
					return;
				}

				stringProperty = value;

				Validation.Validate(() => StringProperty);
				RaisePropertyChanged(() => StringProperty);
			}
		}

		public string StringProperty2
		{
			get { return stringProperty2; }

			set
			{
				if (Equals(stringProperty2, value))
				{
					return;
				}

				stringProperty2 = value;

				Validation.ValidateAsync(() => StringProperty2);

				RaisePropertyChanged(() => StringProperty2);
			}
		}

		public int IntProperty
		{
			get { return intProperty; }

			set
			{
				if (Equals(intProperty, value))
				{
					return;
				}

				intProperty = value;

				Validation.Validate(() => IntProperty);
				RaisePropertyChanged(() => IntProperty);
			}
		}

		public int RangeStart
		{
			get { return rangeStart; }

			set
			{
				if (Equals(rangeStart, value))
				{
					return;
				}

				rangeStart = value;

				Validation.Validate(() => RangeStart);
				RaisePropertyChanged(() => RangeStart);
			}
		}

		public int RangeEnd
		{
			get { return rangeEnd; }

			set
			{
				if (Equals(rangeEnd, value))
				{
					return;
				}

				rangeEnd = value;

				Validation.Validate(() => RangeEnd);
				RaisePropertyChanged(() => RangeEnd);
			}
		}

		public Action SyncValidationRuleExecutedAsyncroniouslyDelegate { get; set; }

		#region IDataErrorInfo Members

		public string this[string columnName]
		{
			get { return DataErrorInfoValidationAdapter[columnName]; }
		}

		public string Error
		{
			get { return DataErrorInfoValidationAdapter.Error; }
		}

		#endregion

		private void SetupValidation()
		{
			var validationRules = new ValidationHelper();

			// Simple properties
			validationRules.AddRule(() => StringProperty,
			                        () => { return RuleResult.Assert(!string.IsNullOrEmpty(StringProperty), "StringProperty cannot be null or empty string"); });

			validationRules.AddRule(() => IntProperty,
			                        () => { return RuleResult.Assert(IntProperty > 0, "IntProperty should be greater than zero."); });

			// Dependant properties
			validationRules.AddRule(() => RangeStart,
			                        () => RangeEnd,
			                        () =>
			                        {
			                        	return RuleResult.Assert(RangeEnd > RangeStart, "RangeEnd must be grater than RangeStart");
			                        });
			
			// Long-running validation (simulates call to a web service or something)
			validationRules.AddRule(
				() => StringProperty2,
				() =>
				{
					if (SyncValidationRuleExecutedAsyncroniouslyDelegate != null)
					{
						SyncValidationRuleExecutedAsyncroniouslyDelegate();
					}

					return RuleResult.Assert(!string.IsNullOrEmpty(StringProperty2), "StringProperty2 cannot be null or empty string");
				});

			Validation = validationRules;
			DataErrorInfoValidationAdapter = new DataErrorInfoAdapter(Validation);
		}
	}
}