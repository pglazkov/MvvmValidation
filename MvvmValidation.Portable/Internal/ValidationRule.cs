using System;
using System.Threading.Tasks;

namespace MvvmValidation.Internal
{
    internal class ValidationRule : IAsyncValidationRule
    {
        public ValidationRule(IValidationTarget target, Func<RuleResult> validateDelegate, Func<Task<RuleResult>> asyncValidateAction)
        {
            Guard.NotNull(target, nameof(target));
            Guard.Assert(validateDelegate != null || asyncValidateAction != null,
                "validateDelegate != null || asyncValidateAction != null");

            Target = target;
            ValidateDelegate = validateDelegate;
            AsyncValidateAction = asyncValidateAction ?? (() => Task.Run(() => ValidateDelegate()));
            Settings = new ValidationRuleSettings();
        }

        private Func<Task<RuleResult>> AsyncValidateAction { get; set; }
        private Func<RuleResult> ValidateDelegate { get; set; }

        public bool SupportsSyncValidation
        {
            get { return ValidateDelegate != null; }
        }

        public IValidationTarget Target { get; private set; }
        public ValidationRuleSettings Settings { get; private set; }

        public RuleResult Evaluate()
        {
            if (!SupportsSyncValidation)
            {
                throw new NotSupportedException(
                    "Synchronous validation is not supported by this rule. Method EvaluateAsync must be called instead.");
            }

            RuleResult result = ValidateDelegate();

            return result;
        }

        public Task<RuleResult> EvaluateAsync()
        {
            return AsyncValidateAction();
        }

        #region Implementation of IValidationRule

        public IValidationRule WithSettings(Action<ValidationRuleSettings> setSettingsDelegate)
        {
            Guard.NotNull(setSettingsDelegate, nameof(setSettingsDelegate));

            setSettingsDelegate(Settings);

            return this;
        }

        #endregion
    }
}