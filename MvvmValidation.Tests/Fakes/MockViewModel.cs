using System;
using System.Collections;
using System.ComponentModel;

namespace MvvmValidation.Tests.Fakes
{
    public class MockViewModel : ViewModelBase, INotifyDataErrorInfo
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
        private NotifyDataErrorInfoAdapter DataErrorInfoValidationAdapter { get; set; }

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

                Validation.Validate(nameof(StringProperty));
                RaisePropertyChanged(nameof(StringProperty));
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

                Validation.ValidateAsync(nameof(StringProperty2));

                RaisePropertyChanged(nameof(StringProperty2));
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

                Validation.Validate(nameof(IntProperty));
                RaisePropertyChanged(nameof(IntProperty));
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

                Validation.Validate(nameof(RangeStart));
                RaisePropertyChanged(nameof(RangeStart));
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

                Validation.Validate(nameof(RangeEnd));
                RaisePropertyChanged(nameof(RangeEnd));
            }
        }

        public Action SyncValidationRuleExecutedAsyncroniouslyDelegate { get; set; }

        #region INotifyDataErrorInfo Members

        public IEnumerable GetErrors(string propertyName)
        {
            return DataErrorInfoValidationAdapter.GetErrors(propertyName);
        }

        public bool HasErrors
        {
            get { return DataErrorInfoValidationAdapter.HasErrors; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
        {
            add { DataErrorInfoValidationAdapter.ErrorsChanged += value; }
            remove { DataErrorInfoValidationAdapter.ErrorsChanged -= value; }
        }

        #endregion

        private void SetupValidation()
        {
            var validationRules = new ValidationHelper();

            // Simple properties
            validationRules.AddRule(nameof(StringProperty),
                () =>
                    RuleResult.Assert(!string.IsNullOrEmpty(StringProperty),
                        "StringProperty cannot be null or empty string"));

            validationRules.AddRule(nameof(IntProperty),
                () => RuleResult.Assert(IntProperty > 0, "IntProperty should be greater than zero."));

            // Dependant properties
            validationRules.AddRule(nameof(RangeStart),
                nameof(RangeEnd),
                () => RuleResult.Assert(RangeEnd > RangeStart, "RangeEnd must be grater than RangeStart"));

            // Long-running validation (simulates call to a web service or something)
            validationRules.AddRule(
                nameof(StringProperty2),
                () =>
                {
                    SyncValidationRuleExecutedAsyncroniouslyDelegate?.Invoke();

                    return RuleResult.Assert(!string.IsNullOrEmpty(StringProperty2),
                        "StringProperty2 cannot be null or empty string");
                });

            Validation = validationRules;
            DataErrorInfoValidationAdapter = new NotifyDataErrorInfoAdapter(Validation);
        }
    }
}