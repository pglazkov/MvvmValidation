using System;
using MvvmValidation.Internal;

namespace MvvmValidation
{
    /// <summary>
    /// Composable builder of validation rule settings.
    /// </summary>
    /// <example><code>
    /// validator.AddRule(MyProperty, () => { // rule logic })
    ///          .WithSettings(s => s.ExecuteOnAlreadyInvalidTarget(true)
    ///                              .EnabledWhen(() => AnotherProperty == "foo"));
    /// </code></example>
    public class ValidationRuleSettingsBuilder
    {
        private readonly ValidationRuleSettings settings = new ValidationRuleSettings();

        /// <summary>
        /// Sets tha value that indicates whether the rule should be executed whan the target is already invalid after executing previous rule(s). 
        /// If not set, the default behavior applies - to skip the rule.
        /// </summary>
        public ValidationRuleSettingsBuilder ExecuteOnAlreadyInvalidTarget(bool value)
        {
            settings.ExecuteOnAlreadyInvalidTarget = value;
            
            return this;
        }

        /// <summary>
        /// Adds a condition when the rule is enabled. If the condition is evaluated as False at the time
        /// of validation, the rule is skipped.
        /// </summary>
        /// <param name="isEnabledDelegate">Function that will be executed at the time of validation to determine if the rule is enabled.</param>
        public ValidationRuleSettingsBuilder EnabledWhen(Func<bool> isEnabledDelegate)
        {
            settings.Conditions.Add(isEnabledDelegate);

            return this;
        }
        
        internal ValidationRuleSettings Build()
        {
            return settings;
        }
    }
}