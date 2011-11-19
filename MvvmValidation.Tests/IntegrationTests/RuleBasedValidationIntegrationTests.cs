using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmValidation.Tests.IntegrationTests
{
	[TestClass]
	public class RuleBasedValidationIntegrationTests
	{
		[TestMethod]
		public void AsyncValidation_GeneralSmokeTest()
		{
			TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
			{
				var vm = new DummyViewModel();
				vm.Foo = null;

				var validation = new ValidationHelper();
				
				bool ruleExecuted = false;

				// OK, this is really strange, but if Action<bool> is not mentioned anywhere in the project, then ReSharter would fail to build and run the test... 
				// So including the following line to fix it.
				Action<RuleValidationResult> dummy = null;
				Assert.IsNull(dummy); // Getting rid of the "unused variable" warning.

				validation.AddAsyncRule(setResult =>
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						ruleExecuted = true;

						if (string.IsNullOrEmpty(vm.Foo))
						{
							setResult(RuleValidationResult.Invalid("Foo cannot be empty string."));
						}
						else
						{
							setResult(RuleValidationResult.Valid());
						}
					});
				});

				validation.ValidationCompleted += (o, e) =>
				{
					Assert.IsTrue(ruleExecuted, "Validation rule must be executed before ValidationCompleted event is fired.");

					var isUiThread = dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

					Assert.IsTrue(isUiThread, "ValidationCompleted must be executed on UI thread");
				};

				validation.ValidateAllAsync(r =>
				{
					var isUiThread = dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

					Assert.IsTrue(isUiThread, "Validation callback must be executed on UI thread");

					Assert.IsFalse(r.IsValid, "Validation must fail according to the validaton rule");
					Assert.IsFalse(validation.GetLastValidationResult().IsValid, "Validation must fail according to the validaton rule");

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
				var vm = new DummyViewModel();
				vm.Foo = "abc";
				vm.Bar = "abc";
				Func<bool> validCondition = () => vm.Foo != vm.Bar;

				var validation = new ValidationHelper();

				validation.AddAsyncRule(
					() => vm.Foo, 
					() => vm.Bar,
					setResult =>
					{
						ThreadPool.QueueUserWorkItem(_ =>
						{
							setResult(RuleValidationResult.Assert(validCondition(), "Foo must be different than bar"));
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

				validation.AddAsyncRule(setResultDelegate =>
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						rule1Executed = true;
						setResultDelegate(RuleValidationResult.Valid());
					});
				});

				bool rule2Executed = false;

				validation.AddAsyncRule(setResultDelegate =>
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						rule2Executed = true;
						setResultDelegate(RuleValidationResult.Invalid("Rule 2 failed"));
					});
				});

				bool rule3Executed = false;

				validation.AddAsyncRule(setResultDelegate =>
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						rule3Executed = true;
						setResultDelegate(RuleValidationResult.Valid());
					});
				});

				validation.ValidateAllAsync(r =>
				{
					Assert.IsTrue(rule1Executed);
					Assert.IsTrue(rule2Executed);
					Assert.IsTrue(rule3Executed);
					Assert.IsFalse(r.IsValid);

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

				validation.AddAsyncRule(vm, setResultDelegate =>
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						rule1Executed = true;
						setResultDelegate(RuleValidationResult.Valid());
					});
				});

				bool rule2Executed = false;

				validation.AddRule(vm, () =>
				{
					rule2Executed = true;
					return RuleValidationResult.Invalid("Rule 2 failed");
				});

				bool rule3Executed = false;

				validation.AddAsyncRule(vm, setResultDelegate =>
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						rule3Executed = true;
						setResultDelegate(RuleValidationResult.Valid());
					});
				});

				validation.ValidateAllAsync(r =>
				{
					Assert.IsTrue(rule1Executed);
					Assert.IsTrue(rule2Executed);
					Assert.IsTrue(rule3Executed);
					Assert.IsFalse(r.IsValid);

					completedAction();
				});
			});
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SyncValidation_ThereAreAsyncRules_ThrowsInvalidOperationException()
		{
			TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
			{
				var vm = new DummyViewModel();

				var validation = new ValidationHelper();

				validation.AddAsyncRule(vm, setResultDelegate =>
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						setResultDelegate(RuleValidationResult.Valid());
					});
				});

				validation.AddRule(vm, () =>
				{
					return RuleValidationResult.Invalid("Rule 2 failed");
				});

				validation.ValidateAll();
			});
		}

		[TestMethod]
		public void SyncValidation_SeveralRulesForOneTarget_ValidWhenAllRulesAreValid()
		{
			var uiThreadDispatcher = Dispatcher.CurrentDispatcher;
			
			var vm = new DummyViewModel();

			var validation = new ValidationHelper();

			validation.AddRule(vm, RuleValidationResult.Valid);

			validation.AddRule(vm, () =>
			{
				return RuleValidationResult.Invalid("Rule 2 failed");
			});

			validation.AddRule(vm, RuleValidationResult.Valid);

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
						return RuleValidationResult.Invalid("Foo cannot be empty string.");
					}

					return RuleValidationResult.Valid();

				});

				validation.ValidateAllAsync(r =>
				{
					completedAction();
				});
			});
		}

		[TestMethod]
		public void AsyncValidation_InvalidValue_CallsValidationRuleInBackgroundTreadAndReportsInvalidOnUIThread()
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

				vm.Validation.ValidationCompleted += (o, e) =>
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

					Assert.IsFalse(e.ValidationResult.IsValid, "Validation must fail");

					completedAction();
				};

				// ACT
				vm.StringProperty2 = null;
			});
		}
	}
}