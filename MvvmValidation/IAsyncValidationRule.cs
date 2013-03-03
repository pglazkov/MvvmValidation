using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	/// <summary>
	/// Represents an asynchronious validation rule.
	/// </summary>
	[ContractClass(typeof(IAsyncValidationRuleContract))]
	public interface IAsyncValidationRule : IValidationRule
	{

	}

	[ContractClassFor(typeof(IAsyncValidationRule))]
// ReSharper disable InconsistentNaming
	internal abstract class IAsyncValidationRuleContract : IAsyncValidationRule
	{
// ReSharper restore InconsistentNaming
	}
}