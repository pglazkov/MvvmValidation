using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmValidation.Tests.Fakes;

namespace MvvmValidation.Tests
{
	[TestClass]
	public class ValidationHelperTests
	{
		[TestMethod]
		public void StringProperty_InvalidValue_HasValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.StringProperty = null;

			// Assert
			Assert.IsFalse(string.IsNullOrEmpty(vm["StringProperty"]));
		}

		[TestMethod]
		public void StringProperty_ValidValue_DoesNotHaveValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.StringProperty = "Not empty string";

			// Assert
			Assert.IsTrue(string.IsNullOrEmpty(vm["StringProperty"]));
		}

		[TestMethod]
		public void RangeProperties_InvalidRange_BothPropertiesHaveValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.RangeStart = 10;
			vm.RangeEnd = 1;

			// Assert
			Assert.IsFalse(string.IsNullOrEmpty(vm["RangeStart"]));
			Assert.IsFalse(string.IsNullOrEmpty(vm["RangeEnd"]));
		}

		[TestMethod]
		public void RangeProperties_ValidRange_NonOfThePropertiesHaveValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.RangeStart = 1;
			vm.RangeEnd = 10;

			// Assert
			Assert.IsTrue(string.IsNullOrEmpty(vm["RangeStart"]));
			Assert.IsTrue(string.IsNullOrEmpty(vm["RangeEnd"]));
		}

		[TestMethod]
		public void RangeProperties_ChangeFromInvalidToValid_ValidationErrorsDisappear()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.RangeStart = 10;
			vm.RangeEnd = 1;

			// Assert
			Assert.IsFalse(string.IsNullOrEmpty(vm["RangeStart"]));
			Assert.IsFalse(string.IsNullOrEmpty(vm["RangeEnd"]));

			// Act
			vm.RangeEnd = 11;

			// Assert
			Assert.IsTrue(string.IsNullOrEmpty(vm["RangeStart"]));
			Assert.IsTrue(string.IsNullOrEmpty(vm["RangeEnd"]));
		}

		[TestMethod]
		public void GetValidationResultFor_EmptyString_GetsErrorsForEntireObject()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.StringProperty = null;

			// Assert
			Assert.IsFalse(string.IsNullOrEmpty(vm[""]));
		}

		[TestMethod]
		public void SuppressValidation_SetInvalidValue_ThereAreNoErrors()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			using (vm.Validation.SuppressValidation())
			{
				vm.RangeStart = 10;
				vm.RangeEnd = 1;
			}

			// Verify
			Assert.IsTrue(string.IsNullOrEmpty(vm["RangeStart"]));
			Assert.IsTrue(string.IsNullOrEmpty(vm["RangeEnd"]));
		}

		[TestMethod]
		public void CombineRuleValidationResults_ResultContainsErrorsFromAllCombinedResults()
		{
			// Arrange
			var validation = new ValidationHelper();
			validation.AddRule(() =>
			{
				//var r1 = RuleValidationResult.Invalid("Error1");
				//var r2 = RuleValidationResult.Valid();
				//var r3 = RuleValidationResult.Invalid("Error2");

				return
					RuleValidationResult.Assert(false, "Error1").Combine(
						RuleValidationResult.Assert(true, "Error0")).Combine(
							RuleValidationResult.Assert(false, "Error2"));

				//return r1.Combine(r2).Combine(r3);
			});

			// Act
			var result = validation.ValidateAll();

			// Assert
			Assert.IsFalse(result.IsValid, "The validation must fail");
			Assert.AreEqual(2, result.ErrorList.Count, "There must be 2 errors");
			Assert.IsTrue(result.ErrorList.Any(e => e.ErrorText == "Error1"));
			Assert.IsTrue(result.ErrorList.Any(e => e.ErrorText == "Error2"));
		}

		[TestMethod]
		public void ValidationCompleted_ValidateRuleWithMultipleTargets_ResultContainsErrorsForAllTargsts()
		{
			// Arrange
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();
			validation.AddRule(() => dummy.Foo, () => dummy.Bar,
			                   () =>
			                   {
			                   	return RuleValidationResult.Invalid("Error");
			                   });

			validation.ValidationCompleted += (o, e) =>
			{
				// Assert

				Assert.IsFalse(e.ValidationResult.IsValid);

				Assert.IsTrue(e.ValidationResult.ErrorList.Count == 2, "There must be two errors: one for each property target");
				Assert.IsTrue(Equals(e.ValidationResult.ErrorList[0].Target, "dummy.Foo"),
				              "Target for the first error must be dummy.Foo");
				Assert.IsTrue(Equals(e.ValidationResult.ErrorList[1].Target, "dummy.Bar"),
				              "Target for the second error must be dummy.Bar");
			};

			// Act
			validation.ValidateAll();
		}
	}
}