using Xunit;

namespace MvvmValidation.Tests
{
	// ReSharper disable InconsistentNaming
	public class NumberedListValidationResultFormatterTests
	{
		[Fact]
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
			Assert.Equal(expectedFormattedString, actualFormattedString);
		}
	}
	// ReSharper restore InconsistentNaming
}