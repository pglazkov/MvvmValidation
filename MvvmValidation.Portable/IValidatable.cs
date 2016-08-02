using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MvvmValidation
{
    /// <summary>
    /// Represents an object that can be validated.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validates the object asyncrhonously.
        /// </summary>
        /// <returns>Task that represents the validation operation.</returns>
        [NotNull]
        Task<ValidationResult> Validate();
    }
}