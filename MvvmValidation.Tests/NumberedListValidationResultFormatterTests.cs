using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmValidation.Tests
{
	// ReSharper disable InconsistentNaming
	[TestClass]
	public class NumberedListValidationResultFormatterTests
	{
		[TestMethod]
		public void Format_SeveralErrorsWithSameMessageButDifferentTarges_OutputsOnlyOneMessage()
		{
			// ARRANGE
			var validationResult = new ValidationResult();
			validationResult.AddError("target1", "Error1");
			validationResult.AddError("target2", validationResult.ErrorList[0].ErrorText);

			var formatter = new NumberedListValidationResultFormatter();
			var expectedFormattedString = validationResult.ErrorList[0].ErrorText;

			// ACT
			var actualFormattedString = formatter.Format(validationResult);

			// VERIFY
			Assert.AreEqual(expectedFormattedString, actualFormattedString);
		}
	}
	// ReSharper restore InconsistentNaming
}