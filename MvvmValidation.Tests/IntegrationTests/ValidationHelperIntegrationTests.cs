using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmValidation.Tests.Fakes;
using MvvmValidation.Tests.Helpers;

namespace MvvmValidation.Tests.IntegrationTests
{
	// ReSharper disable InconsistentNaming
	[TestClass]
	public class ValidationHelperIntegrationTests
	{
		[TestMethod]
		public void AsyncValidation_GeneralSmokeTest()
		{
			TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
			{
				var vm = new DummyViewModel {Foo = null};

				var validation = new ValidationHelper();
				
				bool ruleExecuted = false;

				// OK, this is really strange, but if Action<bool> is not mentioned anywhere in the project, then ReSharter would fail to build and run the test... 
				// So including the following line to fix it.
				Action<RuleResult> dummy = null;
				Assert.IsNull(dummy); // Getting rid of the "unused variable" warning.

				validation.AddAsyncRule(setResult => ThreadPool.QueueUserWorkItem(_ =>
				{
					ruleExecuted = true;

					setResult(RuleResult.Invalid("Foo cannot be empty string."));
				}));

				validation.ResultChanged += (o, e) =>
				{
					Assert.IsTrue(ruleExecuted, "Validation rule must be executed before ValidationCompleted event is fired.");

					var isUiThread = dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

					Assert.IsTrue(isUiThread, "ValidationResultChanged must be executed on UI thread");
				};

				validation.ValidateAllAsync(r =>
				{
					var isUiThread = dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

					Assert.IsTrue(isUiThread, "Validation callback must be executed on UI thread");

					Assert.IsFalse(r.IsValid, "Validation must fail according to the validaton rule");
					Assert.IsFalse(validation.GetResult().IsValid, "Validation must fail according to the validaton rule");

					Assert.IsTrue(ruleExecuted, "Rule must be executed before validation completed callback is executed.");

					completedAction();
				});
			});
		}

		/// <summary>
		/// Asyncs the validation_ dependant properties_ if one invalid second is invalid too.
		/// </summary>
		[TestMethod]
		public void AsyncValidation_DependantProperties_IfOneInvalidSecondIsInvalidToo()
		{
			TestUtils.ExecuteWithDispatcher((dispatcher, testCompleted) =>
			{
				var vm = new DummyViewModel
				{
					Foo = "abc", 
					Bar = "abc"
				};

				Func<bool> validCondition = () => vm.Foo != vm.Bar;

				var validation = new ValidationHelper();

				validation.AddAsyncRule(
					() => vm.Foo, 
					() => vm.Bar,
					setResult =>
					{
						ThreadPool.QueueUserWorkItem(_ =>
						{
							setResult(RuleResult.Assert(validCondition(), "Foo must be different than bar"));
						});
					});

				validation.ValidateAsync(() => vm.Bar, r =>
				{
					Assert.IsFalse(r.IsValid, "Validation must fail");
					Assert.IsTrue(r.ErrorList.Count == 2, "There must be 2 errors: one for each dependant property");

					testCompleted();
				});
			});
		}

		[TestMethod]
		public void AsyncValidation_SeveralAsyncRules_AllExecutedBeforeValidationCompleted()
		{
			TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
			{
				var vm = new DummyViewModel();
				vm.Foo = null;

				var validation = new ValidationHelper();

				bool rule1Executed = false;

				validation.AddAsyncRule(setResultDelegate => ThreadPool.QueueUserWorkItem(_ =>
				{
					rule1Executed = true;
					setResultDelegate(RuleResult.Valid());
				}));

				bool rule2Executed = false;

				validation.AddAsyncRule(setResultDelegate => ThreadPool.QueueUserWorkItem(_ =>
				{
					rule2Executed = true;
					setResultDelegate(RuleResult.Valid());
				}));

				bool rule3Executed = false;

				validation.AddAsyncRule(setResultDelegate => ThreadPool.QueueUserWorkItem(_ =>
				{
					rule3Executed = true;
					setResultDelegate(RuleResult.Valid());
				}));

				validation.ValidateAllAsync(r =>
				{
					Assert.IsTrue(rule1Executed);
					Assert.IsTrue(rule2Executed);
					Assert.IsTrue(rule3Executed);
					Assert.IsTrue(r.IsValid);

					completedAction();
				});
			});
		}

		[TestMethod]
		public void AsyncValidation_MixedAsyncAndNotAsyncRules_AllExecutedBeforeValidationCompleted()
		{
			TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
			{
				var vm = new DummyViewModel();

				var validation = new ValidationHelper();

				bool rule1Executed = false;

				validation.AddAsyncRule(vm, setResultDelegate => ThreadPool.QueueUserWorkItem(_ =>
				{
					rule1Executed = true;
					setResultDelegate(RuleResult.Valid());
				}));

				bool rule2Executed = false;

				validation.AddRule(vm, () =>
				{
					rule2Executed = true;
					return RuleResult.Valid();
				});

				bool rule3Executed = false;

				validation.AddAsyncRule(vm, setResultDelegate => ThreadPool.QueueUserWorkItem(_ =>
				{
					rule3Executed = true;
					setResultDelegate(RuleResult.Valid());
				}));

				validation.ValidateAllAsync(r =>
				{
					Assert.IsTrue(rule1Executed);
					Assert.IsTrue(rule2Executed);
					Assert.IsTrue(rule3Executed);
					Assert.IsTrue(r.IsValid);

					completedAction();
				});
			});
		}

		//[TestMethod]
		//[ExpectedException(typeof(InvalidOperationException))]
		//public void SyncValidation_ThereAreAsyncRules_ThrowsInvalidOperationException()
		//{
		//    TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
		//    {
		//        var vm = new DummyViewModel();

		//        var validation = new ValidationHelper();

		//        validation.AddAsyncRule(vm,
		//                                setResultDelegate =>
		//                                ThreadPool.QueueUserWorkItem(_ =>
		//                                                             setResultDelegate(RuleResult.Valid())));

		//        validation.AddRule(vm, () =>
		//        {
		//            return RuleResult.Invalid("Rule 2 failed");
		//        });

		//        validation.ValidateAll();
		//    });
		//}

		[TestMethod]
		public void SyncValidation_SeveralRulesForOneTarget_ValidWhenAllRulesAreValid()
		{
			var vm = new DummyViewModel();

			var validation = new ValidationHelper();

			validation.AddRule(vm, RuleResult.Valid);

			validation.AddRule(vm, () =>
			{
				return RuleResult.Invalid("Rule 2 failed");
			});

			validation.AddRule(vm, RuleResult.Valid);

			var r = validation.Validate(vm);

			Assert.IsFalse(r.IsValid);
		}

		[TestMethod]
		public void AsyncValidation_SyncRule_ExecutedAsyncroniously()
		{
			TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
			{
				var vm = new DummyViewModel();
				vm.Foo = null;

				var validation = new ValidationHelper();

				validation.AddRule(() =>
				{
					Assert.IsFalse(dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId,
					               "Rule must be executed in a background thread.");

					if (string.IsNullOrEmpty(vm.Foo))
					{
						return RuleResult.Invalid("Foo cannot be empty string.");
					}

					return RuleResult.Valid();

				});

				validation.ValidateAllAsync(r => completedAction());
			});
		}

		[TestMethod]
		public void AsyncValidation_InvalidValue_CallsValidationRuleInBackgroundThreadAndReportsInvalidOnUIThread()
		{
			TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
			{
				int validationExecutionThreadID = -1;
				bool validationRuleCalled = false;

				var vm = new MockViewModel();

				// When the rule gets executed verify that it gets executed in a background thread (doesn't block the UI)
				vm.SyncValidationRuleExecutedAsyncroniouslyDelegate = () =>
				{
					validationRuleCalled = true;
					validationExecutionThreadID = Thread.CurrentThread.ManagedThreadId;

					// VERIFY

					if (validationExecutionThreadID == uiThreadDispatcher.Thread.ManagedThreadId)
					{
						Assert.Fail("Validation rule must be called on a different thread than UI thread.");
					}
				};

				using (vm.Validation.SuppressValidation())
				{
					// Set the valid value first
					vm.StringProperty2 = "Valid value";
				}

				vm.Validation.ResultChanged += (o, e) =>
				{
					// VERIFY

					int validationCompletedThreadID = Thread.CurrentThread.ManagedThreadId;

					if (validationCompletedThreadID != uiThreadDispatcher.Thread.ManagedThreadId)
					{
						Assert.Fail("ValidationCompleted must be called on the UI thread.");
					}

					if (!validationRuleCalled)
					{
						Assert.Fail("Validation rule hasn't been called before validation completed.");
					}

					if (validationExecutionThreadID == validationCompletedThreadID)
					{
						Assert.Fail("Validation execution thread must be a different thread than validation completed thread");
					}

					Assert.IsFalse(e.NewResult.IsValid, "Validation must fail");

					completedAction();
				};

				// ACT
				vm.StringProperty2 = null;
			});
		}

		[TestMethod]
		public void ValidateAsync_MultipleRulesForSameTarget_DoesNotExecuteRulesIfPerviousFailed()
		{
			TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
			{
				// ARRANGE
				var validation = new ValidationHelper();
				var dummy = new DummyViewModel();

				bool firstRuleExecuted = false;
				bool secondRuleExecuted = false;

				validation.AddRule(() => dummy.Foo,
				                   () =>
				                   {
				                   	firstRuleExecuted = true;
				                   	return RuleResult.Invalid("Error1");
				                   });

				validation.AddAsyncRule(() => dummy.Foo,
				                        onCompleted =>
				                        {
				                        	secondRuleExecuted = true;
				                        	onCompleted(RuleResult.Invalid("Error2"));
				                        });

				// ACT

				validation.ValidateAllAsync(result =>
				{
					// VERIFY

					Assert.IsTrue(firstRuleExecuted, "First rule must have been executed");
					Assert.IsFalse(secondRuleExecuted, "Second rule should not have been executed because first rule failed.");

					completedAction();
				});
			});
		}

		[TestMethod]
		[ExpectedException(typeof(ValidationException))]
		public void ValidatedAsync_AsyncRuleDoesnotCallCallback_ThrowsAnExceptionAfterTimeout()
		{
			TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
			{
				// ARRANGE
				var validation = new ValidationHelper();
				validation.AsyncRuleExecutionTimeout = TimeSpan.FromSeconds(0.1);

				var dummy = new DummyViewModel();
				
				validation.AddAsyncRule(() => dummy.Foo,
										onCompleted =>
										{
											// Do nothing
										});

				// ACT

				validation.ValidateAllAsync(result => completedAction());
			});
		}

		[TestMethod]
		public void AddChildValidatable_AddsRuleThatExecutedValidationOnChild()
		{
			TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
			{
				// ARRANGE
				var parent = new ValidatableViewModel();
				var child = new ValidatableViewModel();
				parent.Child = child;

				child.Validator.AddRequiredRule(() => child.Foo, "Error1");
				parent.Validator.AddChildValidatable(() => parent.Child);

				// ACT
				parent.Validator.ValidateAllAsync(result =>
				{
					// VERIFY
					Assert.IsFalse(result.IsValid, "Validation must fail");
					Assert.AreEqual("Error1", result.ErrorList[0].ErrorText);

					completedAction();
				});
			});
		}

		[TestMethod]
		public void AddChildValidatable_ChildValidatableIsNull_NoErrorsAreAdded()
		{
			TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
			{
				// ARRANGE
				var parent = new ValidatableViewModel();

				parent.Validator.AddChildValidatable(() => parent.Child);

				// ACT
				parent.Validator.ValidateAllAsync(result =>
				{
					// VERIFY
					Assert.IsTrue(result.IsValid, "Validation must not fail");

					completedAction();
				});
			});
		}

		[TestMethod]
		public void AddChildValidatableCollection_AddsRuleThatExecutedValidationOnAllValidatableChildren()
		{
			TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
			{
				// ARRANGE
				var parent = new ValidatableViewModel();
				var child1 = new ValidatableViewModel();
				var child2 = new ValidatableViewModel();
				parent.Children = new List<IValidatable>
				{
					child1,
					child2
				};

				child1.Validator.AddRequiredRule(() => child1.Foo, "Error1");
				child2.Validator.AddRule(RuleResult.Valid);

				parent.Validator.AddChildValidatableCollection(() => parent.Children);

				// ACT
				parent.Validator.ValidateAllAsync(result =>
				{
					// VERIFY
					Assert.IsFalse(result.IsValid, "Validation must fail");
					Assert.AreEqual("Error1", result.ErrorList[0].ErrorText);

					completedAction();
				});
			});
		}

		[TestMethod]
		public void AddChildValidatableCollection_ChildCollectionIsNullOrEmpty_NoErrorsAreAdded()
		{
			TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
			{
				// ARRANGE
				var parent = new ValidatableViewModel();

				parent.Validator.AddChildValidatableCollection(() => parent.Children);

				// ACT
				parent.Validator.ValidateAllAsync(r1 =>
				{
					// VERIFY
					Assert.IsTrue(r1.IsValid, "Validation must not fail");

					// ARRANGE
					parent.Children = new List<IValidatable>();

					// ACT
					parent.Validator.ValidateAllAsync(r2 =>
					{
						// VERIFY
						Assert.IsTrue(r2.IsValid, "Validation must not fail.");

						completedAction();
					});
				});
			});
		}
	}
	// ReSharper restore InconsistentNaming
}