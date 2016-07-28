using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmValidation.Tests.Fakes;
using Xunit;

namespace MvvmValidation.Tests
{
	public class ValidationHelperTests
	{
		[Fact]
		public void StringProperty_InvalidValue_HasValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.StringProperty = null;

			// Assert
			
			Assert.True(vm.GetErrors("StringProperty").Cast<string>().Any());
		}

		[Fact]
		public void StringProperty_ValidValue_DoesNotHaveValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.StringProperty = "Not empty string";

			// Assert
			Assert.False(vm.GetErrors("StringProperty").Cast<string>().Any());
		}

		[Fact]
		public void RangeProperties_InvalidRange_BothPropertiesHaveValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.RangeStart = 10;
			vm.RangeEnd = 1;

			// Assert
			Assert.True(!string.IsNullOrEmpty(vm.GetErrors("RangeStart").Cast<string>().FirstOrDefault()));
			Assert.True(!string.IsNullOrEmpty(vm.GetErrors("RangeEnd").Cast<string>().FirstOrDefault()));
		}

		[Fact]
		public void RangeProperties_ValidRange_NonOfThePropertiesHaveValidationError()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.RangeStart = 1;
			vm.RangeEnd = 10;

			// Assert
			Assert.True(string.IsNullOrEmpty(vm.GetErrors("RangeStart").Cast<string>().FirstOrDefault()));
			Assert.True(string.IsNullOrEmpty(vm.GetErrors("RangeEnd").Cast<string>().FirstOrDefault()));
		}

		[Fact]
		public void RangeProperties_ChangeFromInvalidToValid_ValidationErrorsDisappear()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.RangeStart = 10;
			vm.RangeEnd = 1;

			// Assert
			Assert.False(string.IsNullOrEmpty(vm.GetErrors("RangeStart").Cast<string>().FirstOrDefault()));
			Assert.False(string.IsNullOrEmpty(vm.GetErrors("RangeEnd").Cast<string>().FirstOrDefault()));

			// Act
			vm.RangeEnd = 11;

			// Assert
			Assert.True(string.IsNullOrEmpty(vm.GetErrors("RangeStart").Cast<string>().FirstOrDefault()));
			Assert.True(string.IsNullOrEmpty(vm.GetErrors("RangeEnd").Cast<string>().FirstOrDefault()));
		}

		[Fact]
		public void GetValidationResultFor_EmptyString_GetsErrorsForEntireObject()
		{
			// Arrange
			var vm = new MockViewModel();

			// Act
			vm.StringProperty = null;

			// Assert
			Assert.False(string.IsNullOrEmpty(vm.GetErrors("").Cast<string>().FirstOrDefault()));
		}

		[Fact]
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
			Assert.True(string.IsNullOrEmpty(vm.GetErrors("RangeStart").Cast<string>().FirstOrDefault()));
			Assert.True(string.IsNullOrEmpty(vm.GetErrors("RangeEnd").Cast<string>().FirstOrDefault()));
		}

		[Fact]
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
			Assert.False(result.IsValid, "The validation must fail");
			Assert.Equal(2, result.ErrorList.Count);
			Assert.True(result.ErrorList.Any(e => e.ErrorText == "Error1"));
			Assert.True(result.ErrorList.Any(e => e.ErrorText == "Error2"));
		}

		[Fact]
		public void ValidationCompleted_ValidateRuleWithMultipleTargets_ResultContainsErrorsForAllTargsts()
		{
			// Arrange
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();
			validation.AddRule(nameof(dummy.Foo), nameof(dummy.Bar),
							   () => RuleResult.Invalid("Error"));

			// Act
			var result = validation.ValidateAll();

			// Assert
			Assert.False(result.IsValid);

			Assert.True(result.ErrorList.Count == 2, "There must be two errors: one for each property target");
			Assert.True(Equals(result.ErrorList[0].Target, "Foo"), "Target for the first error must be Foo");
			Assert.True(Equals(result.ErrorList[1].Target, "Bar"), "Target for the second error must be Bar");
		}

		[Fact]
		public void ValidationResultChanged_ValidateExecutedForOneRule_FiresOneTime()
		{
			// Arrange
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			validation.AddRule(nameof(dummy.Foo),
							   () => RuleResult.Invalid("Error"));

			var eventFiredTimes = 0;

			validation.ResultChanged += (o, e) =>
			{
				eventFiredTimes++;
			};

			// Act
			validation.ValidateAll();

			// Verity
			Assert.Equal(1, eventFiredTimes);
		}

		[Fact]
		public void ResultChanged_ValidateExecutedForSeveralRules_FiresForEachTarget()
		{
			// Arrange
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			validation.AddRule(nameof(dummy.Foo),
							   () => RuleResult.Invalid("Error"));
			validation.AddRule(nameof(dummy.Foo),
							   RuleResult.Valid);
			validation.AddRule(nameof(dummy.Bar),
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
			Assert.Equal(expectedTimesToFire, eventFiredTimes);
		}

		[Fact]
		public void ResultChanged_CorrectingValidationError_EventIsFiredForWithValidResultAfterCorrection()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			var fooResult = RuleResult.Valid();

			// ReSharper disable AccessToModifiedClosure // Intended
			validation.AddRule(nameof(dummy.Foo), () => fooResult);
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
				Assert.False(r.NewResult.IsValid, "ResultChanged must be fired with invalid result first.");
			};
			validation.ValidateAll();


			// Second, verify that after second validation when error was corrected, the event fires with the valid result

			fooResult = RuleResult.Valid();

			onResultChanged = r =>
			{
				Assert.True(r.NewResult.IsValid, "ResultChanged must be fired with valid result after succesfull validation.");
			};

			validation.ValidateAll();
		}

		[Fact]
		public void ResultChanged_RuleErrorsChangedButRuleValidityDidNotChange_EventStillFires()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			validation.AddRule(nameof(dummy.Foo),
				() =>
				{
					if (string.IsNullOrEmpty(dummy.Foo))
					{
						return RuleResult.Invalid("Foo should not be empty");
					}

					return
						RuleResult.Assert(dummy.Foo.Length > 5, "Length must be greater than 5").Combine(
							RuleResult.Assert(dummy.Foo.Any(Char.IsDigit), "Must contain digit"));
				});

			var resultChangedCalledTimes = 0;
			const int expectedResultChangedCalls = 1 /* First invalid value */+ 1 /* Second invalid value */+ 1 /* Third invalid value */ + 1 /* Valid value */;

			validation.ResultChanged += (o, e) =>
			{
				resultChangedCalledTimes++;
			};

			// ACT

			dummy.Foo = null;

			// Should generage "Foo should not be empty" error
			validation.ValidateAll();

			dummy.Foo = "123";

			// Should generate the "Length must be greater than 5" error
			validation.ValidateAll();

			dummy.Foo = "sdfldlssd";

			// Should generate the "Must contain digit" error
			validation.ValidateAll();

			dummy.Foo = "lsdklfjsld2342";

			// Now should be valid
			validation.ValidateAll();

			// VERIFY
			Assert.Equal(expectedResultChangedCalls, resultChangedCalledTimes);
		}

		[Fact]
		public void Validate_MultipleRulesForSameTarget_DoesNotExecuteRulesIfPerviousFailed()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			bool firstRuleExecuted = false;
			bool secondRuleExecuted = false;

			validation.AddRule(nameof(dummy.Foo),
							   () =>
							   {
								   firstRuleExecuted = true;
								   return RuleResult.Invalid("Error1");
							   });
			validation.AddRule(nameof(dummy.Foo),
							   () =>
							   {
								   secondRuleExecuted = true;
								   return RuleResult.Invalid("Error2");
							   });

			// ACT

			validation.ValidateAll();

			// VERIFY

			Assert.True(firstRuleExecuted, "First rule must have been executed");
			Assert.False(secondRuleExecuted, "Second rule should not have been executed because first rule failed.");
		}

		[Fact]
		public void Validate_MultipleRulesForSameTarget_ClearsResultsBeforeValidation()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			RuleResult firstRuleResult = RuleResult.Valid();
			RuleResult secondRuleResult = RuleResult.Invalid("Error2");

			validation.AddRule(nameof(dummy.Foo),
							   () =>
							   {
								   return firstRuleResult;
							   });
			validation.AddRule(nameof(dummy.Foo),
							   () =>
							   {
								   return secondRuleResult;
							   });

			// ACT

			validation.ValidateAll();

			firstRuleResult = RuleResult.Invalid("Error1");

			validation.ValidateAll();

			// VERIFY

			var result = validation.GetResult(nameof(dummy.Foo));

			Assert.False(result.IsValid);
			Assert.Equal(1, result.ErrorList.Count);
			Assert.Equal("Error1", result.ErrorList[0].ErrorText);
		}

		[Fact]
		public void AddRequiredRule_AddsRuleThatChecksTheObjectNotNullOrEmptyString()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			var dummy = new DummyViewModel();

			validation.AddRequiredRule(() => dummy.Foo, "Foo cannot be empty");

			// ACT
			var result = validation.ValidateAll();

			// VERIFY
			Assert.False(result.IsValid, "Validation must fail");

			// ACT
			dummy.Foo = "abc";
			var resultAfterCorrection = validation.ValidateAll();

			// VERIFY
			Assert.True(resultAfterCorrection.IsValid, "The result must be valid after the correction of the error");
		}

		[Fact]
		public void Validate_ThereAreAsyncRules_ThrowsException()
		{
			// ARRANGE
			var validation = new ValidationHelper();

			// Add a simple sync rule
			validation.AddRule(RuleResult.Valid);

			// Add an async rule
			validation.AddAsyncRule(() => Task.Run(() => RuleResult.Invalid("Error")));

			// ACT
			Assert.Throws<InvalidOperationException>(() =>
				{
					validation.ValidateAll();
				});

		}

		[Fact]
		public void RemoveRule_ReExecuteValidation_RemovedRuleDoesNotExecute()
		{
			// ARRANGE
			var validation = new ValidationHelper();

			validation.AddRule(RuleResult.Valid);
			var invalidRule = validation.AddRule(() => RuleResult.Invalid("error"));

			var validationResult = validation.ValidateAll();

			Assert.False(validationResult.IsValid);


			// ACT
			validation.RemoveRule(invalidRule);

			validationResult = validation.ValidateAll();

			// VERIFY
			Assert.True(validationResult.IsValid);
		}

		[Fact]
		public void RemoveRule_ThisRuleHadValidationError_ErrorGetsRemovedAlso()
		{
			// ARRANGE
			var validation = new ValidationHelper();

			validation.AddRule(RuleResult.Valid);
			var invalidRule = validation.AddRule(() => RuleResult.Invalid("error"));

			var validationResult = validation.ValidateAll();

			Assert.False(validationResult.IsValid);

			// ACT
			validation.RemoveRule(invalidRule);

			validationResult = validation.GetResult();

			// VERIFY
			Assert.True(validationResult.IsValid);
		}

		[Fact]
		public void RemoveRule_ThisRuleHadValidationError_ResultChangedEventIsFiredForCorrectTargetAndTargetIsValid()
		{
			// ARRANGE
			var validation = new ValidationHelper();

			validation.AddRule(RuleResult.Valid);
			var invalidRule = validation.AddRule("test_target", () => RuleResult.Invalid("error"));

			var validationResult = validation.ValidateAll();

			Assert.False(validationResult.IsValid);

			bool resultChangedEventFired = false;

			validation.ResultChanged += (sender, args) =>
			{
				Assert.Equal("test_target", args.Target);
				Assert.True(args.NewResult.IsValid);

				resultChangedEventFired = true;
			};

			// ACT
			validation.RemoveRule(invalidRule);

			// VERIFY
			Assert.True(resultChangedEventFired);
		}

		[Fact]
		public void RemoveRule_ThereAreTwoFailedRules_RemoveOne_ResultChangedShouldBeFiredWithNewResultStillInvalid()
		{
			// ARRANGE
			var dummy = new DummyViewModel();

			var validation = new ValidationHelper();

			validation.AddRule(nameof(dummy.Foo), () => RuleResult.Invalid("error2"));
			var invalidRule = validation.AddRule(nameof(dummy.Foo), () => RuleResult.Invalid("error"));

			var validationResult = validation.ValidateAll();

			Assert.False(validationResult.IsValid);

			bool resultChangedEventFired = false;

			validation.ResultChanged += (sender, args) =>
			{
				Assert.Equal(nameof(dummy.Foo), args.Target);
				Assert.False(args.NewResult.IsValid);

				resultChangedEventFired = true;
			};

			// ACT
			validation.RemoveRule(invalidRule);

			// VERIFY
			Assert.True(resultChangedEventFired);
		}

		[Fact]
		public void RemoveAllRules_HadTwoFailedRules_ErrorGetsRemovedAlso()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			validation.AddRule(() => RuleResult.Invalid("error1"));
			validation.AddRule(() => RuleResult.Invalid("error2"));

			var validationResult = validation.ValidateAll();

			// ACT
			validation.RemoveAllRules();

			validationResult = validation.GetResult();

			// VERIFY
			Assert.True(validationResult.IsValid, "Validation should not produce any errors after all rules were removed.");
		}


		[Fact]
		public void RemoveAllRules_HadTwoNegativeRulesRegistered_ValidationSucceds()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			validation.AddRule(() => RuleResult.Invalid("error1"));
			validation.AddRule(() => RuleResult.Invalid("error2"));

			// ACT
			validation.RemoveAllRules();

			var validationResult = validation.ValidateAll();

			// VERIFY
			Assert.True(validationResult.IsValid, "Validation should not produce any errors after all rules were removed.");
		}

		[Fact]
		public void RemoveAllRules_HadTwoNegativeRulesRegisteredForDifferentTargets_ResultChangedIsFiredForAllTargets()
		{
			// ARRANGE
			var dummy = new DummyViewModel();

			var validation = new ValidationHelper();
			validation.AddRule(nameof(dummy.Foo), () => RuleResult.Invalid("error1"));
			validation.AddRule(nameof(dummy.Bar), () => RuleResult.Invalid("error2"));

			validation.ValidateAll();

			int resultChangedFiredCount = 0;

			validation.ResultChanged += (sender, args) =>
			{
				resultChangedFiredCount++;
			};

			// ACT
			validation.RemoveAllRules();

			// VERIFY
			Assert.Equal(2, resultChangedFiredCount);
		}

		[Fact]
		public void Reset_AllTargetsBecomeValidAgain()
		{
			// ARRANGE
			var dummy = new DummyViewModel();

			var validation = new ValidationHelper();
			validation.AddRule(nameof(dummy.Foo), () => RuleResult.Invalid("error1"));
			validation.AddRule(nameof(dummy.Bar), () => RuleResult.Invalid("error2"));

			validation.ValidateAll();

			// ACT
			validation.Reset();

			// VERIFY
			Assert.True(validation.GetResult().IsValid);
			Assert.True(validation.GetResult(nameof(dummy.Foo)).IsValid);
			Assert.True(validation.GetResult(nameof(dummy.Bar)).IsValid);
		}

		[Fact]
		public void Reset_ResultChangedFiresForInvalidTargets()
		{
			// ARRANGE
			var dummy = new DummyViewModel();

			var validation = new ValidationHelper();
			validation.AddRule(RuleResult.Valid);
			validation.AddRule(nameof(dummy.Foo), () => RuleResult.Invalid("error1"));
			validation.AddRule(nameof(dummy.Bar), () => RuleResult.Invalid("error2"));

			validation.ValidateAll();

			bool eventFiredForFoo = false;
			bool evernFiredForBar = false;

			validation.ResultChanged += (sender, args) =>
			{
				if (Equals(args.Target, nameof(dummy.Foo)))
				{
					eventFiredForFoo = true;
				}
				else if (Equals(args.Target, nameof(dummy.Bar)))
				{
					evernFiredForBar = true;
				}
				else
				{
					Assert.False(true, "ResultChanged event fired for an unexpected target.");
				}

				Assert.True(args.NewResult.IsValid);
			};

			// ACT
			validation.Reset();

			// VERIFY
			Assert.True(eventFiredForFoo);
			Assert.True(evernFiredForBar);
		}

		[Fact]
		public void Validate_ValidationResultIsValid_ToStringReturnsEmptyString()
		{
			// ARRANGE
			var validation = new ValidationHelper();
			validation.AddRule(RuleResult.Valid);

			// ACT
			var r = validation.ValidateAll();

			// VERIFY
			Assert.True(r.ToString() == string.Empty);
		}
	}
}