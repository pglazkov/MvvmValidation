using System;
using System.Linq;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmValidation.Internal;
using MvvmValidation.Tests.Fakes;

namespace MvvmValidation.Tests
{
	[TestClass]
	public class ValidationHelperTests
	{
		[TestInitialize]
		public void TestInitialize()
		{
			var uiThreadDispatcher = Dispatcher.CurrentDispatcher;
			CurrentDispatcher.Instance = uiThreadDispatcher;
		}

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
		public void CombineRuleResults_ResultContainsErrorsFromAllCombinedResults()
		{
			// Arrange
			var validation = new ValidationHelper();
			validation.AddRule(() =>
			{
				//var r1 = RuleResult.Invalid("Error1");
				//var r2 = RuleResult.Valid();
				//var r3 = RuleResult.Invalid("Error2");

				return
					RuleResult.Assert(false, "Error1").Combine(
						RuleResult.Assert(true, "Error0")).Combine(
							RuleResult.Assert(false, "Error2"));

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
			                   	return RuleResult.Invalid("Error");
			                   });

			// Act
			var result = validation.ValidateAll();

			// Assert
			Assert.IsFalse(result.IsValid);

			Assert.IsTrue(result.ErrorList.Count == 2, "There must be two errors: one for each property target");
			Assert.IsTrue(Equals(result.ErrorList[0].Target, "dummy.Foo"), "Target for the first error must be dummy.Foo");
			Assert.IsTrue(Equals(result.ErrorList[1].Target, "dummy.Bar"), "Target for the second error must be dummy.Bar");
		}

		[TestMethod]
		public void ValidationResultChanged_ValidateExecutedForOneRule_FiresOneTime()
		{
			// Arrange
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			validation.AddRule(() => dummy.Foo,
			                   () => RuleResult.Invalid("Error"));

			var eventFiredTimes = 0;

			validation.ResultChanged += (o, e) =>
			{
				eventFiredTimes++;
			};

			// Act
			validation.ValidateAll();

			// Verity
			Assert.AreEqual(1, eventFiredTimes, "Event should have been fired");
		}

		[TestMethod]
		public void ResultChanged_ValidateExecutedForSeveralRules_FiresForEachTarget()
		{
			// Arrange
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			validation.AddRule(() => dummy.Foo,
							   () => RuleResult.Invalid("Error"));
			validation.AddRule(() => dummy.Foo,
			                   RuleResult.Valid);
			validation.AddRule(() => dummy.Bar,
								RuleResult.Valid);
			validation.AddRule(() => RuleResult.Invalid("Error"));

			const int expectedTimesToFire = 0 + 1 /*Invalid Foo*/+ 1 /* Invalid general target */;
			var eventFiredTimes = 0;

			validation.ResultChanged += (o, e) =>
			{
				eventFiredTimes++;
			};

			// Act
			validation.ValidateAll();

			// Verify
			Assert.AreEqual(expectedTimesToFire, eventFiredTimes);
		}

		[TestMethod]
		public void ResultChanged_CorrectingValidationError_EventIsFiredForWithValidResultAfterCorrection()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			var fooResult = RuleResult.Valid();

// ReSharper disable AccessToModifiedClosure // Intended
			validation.AddRule(() => dummy.Foo, () => fooResult);
// ReSharper restore AccessToModifiedClosure

			var onResultChanged = new Action<ValidationResultChangedEventArgs>(r => { });

// ReSharper disable AccessToModifiedClosure // Intended
			validation.ResultChanged += (o, e) => onResultChanged(e);
// ReSharper restore AccessToModifiedClosure


			// ACT & VERIFY

			// First, verify that the event is fired with invalid result 

			fooResult = RuleResult.Invalid("Error");

			onResultChanged = r =>
			{
				Assert.IsFalse(r.NewResult.IsValid, "ResultChanged must be fired with invalid result first.");
			};
			validation.ValidateAll();


			// Second, verify that after second validation when error was corrected, the event fires with the valid result

			fooResult = RuleResult.Valid();

			onResultChanged = r =>
			{
				Assert.IsTrue(r.NewResult.IsValid, "ResultChanged must be fired with valid result after succesfull validation.");
			};

			validation.ValidateAll();
		}
	}
}