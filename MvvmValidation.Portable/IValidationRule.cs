using System;
using JetBrains.Annotations;
using MvvmValidation.Internal;

namespace MvvmValidation
{
    /// <summary>
    /// Represents a validation rule.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Allows changing the rule settings. 
        /// </summary>
        /// <param name="setSettingsDelegate">A function that accepts an instance of <see cref="ValidationRuleSettings"/> that contains settings for this rule.</param>
        /// <returns>The same fule instance (allows for "fluent" interface with chained calls).</returns>
        [NotNull]
        IValidationRule WithSettings([NotNull] Action<ValidationRuleSettingsBuilder> setSettingsDelegate);
    }
}