using System;

namespace MvvmValidation
{
    /// <summary>
    /// Contains settings that control the behavior of a particular instance of <see cref="ValidationHelper"/>.
    /// </summary>
    public class ValidationSettings
    {
        /// <summary>
        /// Creates an instance of validation settings.
        /// </summary>
        /// <param name="defaultRuleSettingsProvider">A function for specifying default settings for a rule</param>
        public ValidationSettings(Action<ValidationRuleSettingsBuilder> defaultRuleSettingsProvider = null)
        {
            DefaultRuleSettings = defaultRuleSettingsProvider;
        }
        
        /// <summary>
        /// Default validation rule settings for all rules (each rule can still specify its own settings during registration). 
        /// </summary>
        public Action<ValidationRuleSettingsBuilder> DefaultRuleSettings { get; set; }
    }
}