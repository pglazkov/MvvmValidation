namespace MvvmValidation
{
    /// <summary>
    /// Represents validation rule settings that control the rule behavior.
    /// </summary>
    public class ValidationRuleSettings
    {
        /// <summary>
        /// When set (not null), determines whether the rule should be executed whan the target is already invalid after executing previous rule(s). 
        /// If not set (null), the default behavior applies - to skip the rule.
        /// </summary>
        public bool? ExecuteOnAlreadyInvalidTarget { get; set; }
    }
}