using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	/// <summary>
	/// Represents a validation rule.
	/// </summary>
	[ContractClass(typeof(IValidationRuleContract))]
	public interface IValidationRule
	{

	}

	[ContractClassFor(typeof(IValidationRule))]
	// ReSharper disable InconsistentNaming
	internal abstract class IValidationRuleContract : IValidationRule
	{
	// ReSharper restore InconsistentNaming
	}
}