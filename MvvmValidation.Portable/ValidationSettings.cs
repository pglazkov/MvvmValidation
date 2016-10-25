namespace MvvmValidation
{
    /// <summary>
    /// Contains settings that control the behavior of a particular instance of <see cref="ValidationHelper"/>.
    /// </summary>
    public class ValidationSettings
    {
        /// <summary>
        /// When specified, overrides the default validation rule settings for all rules (each rule can still specify its own settings during registration). 
        /// </summary>
        public ValidationRuleSettings DefaultRuleSettings { get; set; }
    }
}