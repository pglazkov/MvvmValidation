using System;
using System.Collections.Generic;
using System.Linq;
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

        private bool IsEnabled {
            get { return Settings.Conditions.All(c => c()); }
        }

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

            return !IsEnabled ? RuleResult.Valid() : ValidateDelegate();
        }

        public Task<RuleResult> EvaluateAsync()
        {
            return !IsEnabled ? Task.FromResult(RuleResult.Valid()) : AsyncValidateAction();
        }

        public IValidationRule WithSettings(Action<ValidationRuleSettingsBuilder> setSettingsDelegate)
        {
            Guard.NotNull(setSettingsDelegate, nameof(setSettingsDelegate));

            var builder = new ValidationRuleSettingsBuilder();
            
            setSettingsDelegate(builder);

            Settings = builder.Build();

            return this;
        }
    }
}