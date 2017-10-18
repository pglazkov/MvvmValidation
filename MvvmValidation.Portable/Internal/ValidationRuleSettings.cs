using System;
using System.Collections.Generic;

namespace MvvmValidation.Internal
{
    internal class ValidationRuleSettings
    {
        public List<Func<bool>> Conditions { get; } = new List<Func<bool>>();

        public bool? ExecuteOnAlreadyInvalidTarget { get; set; }
    }
}