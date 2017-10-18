using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MvvmValidation.Internal;
using MvvmValidation.Tests.Fakes;
using MvvmValidation.Tests.Helpers;
using Xunit;

namespace MvvmValidation.Tests.IntegrationTests
{
    // ReSharper disable InconsistentNaming
    public class ValidationHelperIntegrationTests
    {
        [Fact]
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
                Assert.Null(dummy); // Getting rid of the "unused variable" warning.

                validation.AddAsyncRule(async () =>
                {
                    return await Task.Run(() =>
                    {
                        ruleExecuted = true;

                        return RuleResult.Invalid("Foo cannot be empty string.");
                    });
                });

                validation.ResultChanged += (o, e) =>
                {
                    Assert.True(ruleExecuted,
                        "Validation rule must be executed before ValidationCompleted event is fired.");

                    var isUiThread = dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

                    Assert.True(isUiThread, "ValidationResultChanged must be executed on UI thread");
                };

                var ui = TaskScheduler.FromCurrentSynchronizationContext();

                validation.ValidateAllAsync().ContinueWith(r =>
                {
                    var isUiThread = dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

                    Assert.True(isUiThread, "Validation callback must be executed on UI thread");

                    Assert.False(r.Result.IsValid, "Validation must fail according to the validaton rule");
                    Assert.False(validation.GetResult().IsValid, "Validation must fail according to the validaton rule");

                    Assert.True(ruleExecuted, "Rule must be executed before validation completed callback is executed.");

                    completedAction();
                }, ui);
            });
        }

        [Fact]
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
                    nameof(vm.Foo),
                    nameof(vm.Bar),
                    async () =>
                    {
                        return
                            await Task.Run(() => RuleResult.Assert(validCondition(), "Foo must be different than bar"));
                    });

                validation.ValidateAsync(nameof(vm.Bar)).ContinueWith(r =>
                {
                    Assert.False(r.Result.IsValid, "Validation must fail");
                    Assert.True(r.Result.ErrorList.Count == 2, "There must be 2 errors: one for each dependant property");

                    testCompleted();
                });
            });
        }

        [Fact]
        public void AsyncValidation_SeveralAsyncRules_AllExecutedBeforeValidationCompleted()
        {
            TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
            {
                var vm = new DummyViewModel();
                vm.Foo = null;

                var validation = new ValidationHelper();

                bool rule1Executed = false;

                validation.AddAsyncRule(() =>
                {
                    return Task.Run(() =>
                    {
                        rule1Executed = true;
                        return RuleResult.Valid();
                    });
                });

                bool rule2Executed = false;

                validation.AddAsyncRule(() =>
                {
                    return Task.Run(() =>
                    {
                        rule2Executed = true;
                        return RuleResult.Valid();
                    });
                });

                bool rule3Executed = false;

                validation.AddAsyncRule(() =>
                {
                    return Task.Run(() =>
                    {
                        rule3Executed = true;
                        return RuleResult.Valid();
                    });
                });

                validation.ValidateAllAsync().ContinueWith(r =>
                {
                    Assert.True(rule1Executed);
                    Assert.True(rule2Executed);
                    Assert.True(rule3Executed);
                    Assert.True(r.Result.IsValid);

                    completedAction();
                });
            });
        }

        [Fact]
        public void AsyncValidation_MixedAsyncAndNotAsyncRules_AllExecutedBeforeValidationCompleted()
        {
            TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
            {
                var vm = new DummyViewModel();

                var validation = new ValidationHelper();

                bool rule1Executed = false;

                validation.AddAsyncRule(vm, () =>
                {
                    return Task.Run(() =>
                    {
                        rule1Executed = true;
                        return RuleResult.Valid();
                    });
                });

                bool rule2Executed = false;

                validation.AddRule(vm, () =>
                {
                    rule2Executed = true;
                    return RuleResult.Valid();
                });

                bool rule3Executed = false;

                validation.AddAsyncRule(vm, () =>
                {
                    return Task.Run(() =>
                    {
                        rule3Executed = true;
                        return RuleResult.Valid();
                    });
                });

                validation.ValidateAllAsync().ContinueWith(r =>
                {
                    Assert.True(rule1Executed);
                    Assert.True(rule2Executed);
                    Assert.True(rule3Executed);
                    Assert.True(r.Result.IsValid);

                    completedAction();
                });
            });
        }

        [Fact]
        public void AsyncValidation_SyncRule_ExecutedAsyncroniously()
        {
            TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
            {
                var vm = new DummyViewModel();
                vm.Foo = null;

                var validation = new ValidationHelper();

                validation.AddRule(() =>
                {
                    Assert.False(dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId,
                        "Rule must be executed in a background thread.");

                    if (string.IsNullOrEmpty(vm.Foo))
                    {
                        return RuleResult.Invalid("Foo cannot be empty string.");
                    }

                    return RuleResult.Valid();
                });

                validation.ValidateAllAsync().ContinueWith(r => completedAction());
            });
        }

        [Fact]
        public void SyncValidation_CallResultsChangedOnUIThread() {
            TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) => {
                var validation = new ValidationHelper();

                validation.AddRule(() => RuleResult.Invalid("error"));

                validation.ResultChanged += (o, e) => {
                    // VERIFY
                    if (Thread.CurrentThread.ManagedThreadId != uiThreadDispatcher.Thread.ManagedThreadId) {
                        Assert.True(false, "ValidationCompleted must be called on the UI thread.");
                    }
                   

                    completedAction();
                };

                // ACT
                validation.ValidateAll();
            });
        }

        [Fact]
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
                        Assert.True(false, "Validation rule must be called on a different thread than UI thread.");
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
                        Assert.True(false, "ValidationCompleted must be called on the UI thread.");
                    }

                    if (!validationRuleCalled)
                    {
                        Assert.True(false, "Validation rule hasn't been called before validation completed.");
                    }

                    if (validationExecutionThreadID == validationCompletedThreadID)
                    {
                        Assert.True(false,
                            "Validation execution thread must be a different thread than validation completed thread");
                    }

                    Assert.False(e.NewResult.IsValid, "Validation must fail");

                    completedAction();
                };

                // ACT
                vm.StringProperty2 = null;
            });
        }

        [Fact]
        public void AsyncValidation_MultipleRulesForSameTarget_DoesNotExecuteRulesIfPerviousFailed()
        {
            TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
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

                validation.AddAsyncRule(nameof(dummy.Foo),
                    () =>
                    {
                        return Task.Run(() =>
                        {
                            secondRuleExecuted = true;
                            return RuleResult.Invalid("Error2");
                        });
                    });

                // ACT

                validation.ValidateAllAsync().ContinueWith(result =>
                {
                    // VERIFY

                    Assert.True(firstRuleExecuted, "First rule must have been executed");
                    Assert.False(secondRuleExecuted,
                        "Second rule should not have been executed because first rule failed.");

                    completedAction();
                });
            });
        }

        [Fact]
        public void AsyncValidation_ExecutedInsideUIThreadTask_ValidationRunsInBackgroundThread()
        {
            // This test checks the situation described here:
            // http://stackoverflow.com/questions/6800705/why-is-taskscheduler-current-the-default-taskscheduler
            // In short: if a continuation of a task runs on the UI thread, then the tasks that you start using
            // Task.Factory.StartNew will run also on the UI thread. This is unexpected an may cause a deadlock, or freezing UI.

            TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
            {
                // ARRANGE
                var validation = new ValidationHelper();

                var dummy = new DummyViewModel();

                int uiThreadId = Thread.CurrentThread.ManagedThreadId;
                int validationThreadId = uiThreadId;

                validation.AddAsyncRule(nameof(dummy.Foo),
                    () =>
                    {
                        return Task.Run(() =>
                        {
                            validationThreadId = Thread.CurrentThread.ManagedThreadId;

                            return RuleResult.Valid();
                        });
                    });

                // ACT

                // Execute validation within a continuation of another task that runs on
                // current synchronization context, so that the TaskScheduler.Current is set to the 
                // UI context.
                Task.Factory.StartNew(() => { }).ContinueWith(t =>
                {
                    Task<ValidationResult> task = validation.ValidateAllAsync();

                    task.ContinueWith(result =>
                    {
                        // VERIFY

                        // Schedule the varification code with the dispatcher in order to take it
                        // out of the ContinueWith continuation. Otherwise the exceptions will be stored inside the task and 
                        // unit testing framework will not know about them.
                        uiThreadDispatcher.BeginInvoke(new Action(() =>
                        {
                            Assert.False(uiThreadId == validationThreadId,
                                "Validation must be executed in a background thread, so that the UI thread is not blocked.");

                            completedAction();
                        }));
                    });
                }, TaskScheduler.FromCurrentSynchronizationContext());
            });
        }

        [Fact]
        public void AsyncValidation_SimilteniousCalls_DoesNotFail()
        {
            TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
            {
                const int numThreadsPerIternation = 4;
                const int iterationCount = 10;
                const int numThreads = numThreadsPerIternation*iterationCount;
                var resetEvent = new ManualResetEvent(false);
                int toProcess = numThreads;

                var validation = new ValidationHelper();

                for (int i = 0; i < iterationCount; i++)
                {
                    var target1 = new object();
                    var target2 = new object();

                    validation.AddAsyncRule(() => { return Task.Run(() => RuleResult.Invalid("Error1")); });

                    validation.AddAsyncRule(target1, () => { return Task.Run(() => RuleResult.Valid()); });

                    validation.AddRule(target2, () => { return RuleResult.Invalid("Error2"); });

                    validation.AddRule(target2, RuleResult.Valid);

                    Action<Action> testThreadBody = exercise =>
                    {
                        try
                        {
                            exercise();

                            if (Interlocked.Decrement(ref toProcess) == 0)
                                resetEvent.Set();
                        }
                        catch (Exception ex)
                        {
                            dispatcher.BeginInvoke(new Action(() => { throw new AggregateException(ex); }));
                        }
                    };

                    var thread1 = new Thread(() => { testThreadBody(() => { validation.ValidateAllAsync().Wait(); }); });

                    var thread2 = new Thread(() => { testThreadBody(() => { validation.ValidateAllAsync().Wait(); }); });

                    var thread3 = new Thread(() => { testThreadBody(() => { validation.Validate(target2); }); });

                    var thread4 = new Thread(() => { testThreadBody(() => { validation.Validate(target2); }); });

                    thread1.Start();
                    thread2.Start();
                    thread3.Start();
                    thread4.Start();
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    resetEvent.WaitOne();
                    completedAction();
                });
            });
        }

        [Fact]
        public async Task AsyncValidation_AsyncRuleRegisteredWithNewSyntax_RuleIsExecuted()
        {
            // ARRANGE
            var validator = new ValidationHelper();

            validator.AddAsyncRule(async () => await Task.Factory.StartNew(() => RuleResult.Invalid("error")));

            // ACT
            var result = await validator.ValidateAllAsync();

            // VERIFY
            Assert.False(result.IsValid);
        }

        [Fact]
        public async void AsyncValidation_RuleThrowsException_ExceptionIsPropogated()
        {
            // ARRANGE
            var validator = new ValidationHelper();

            validator.AddAsyncRule(async () =>
            {
                await
                    Task.Factory.StartNew(() => { throw new InvalidOperationException("Test"); }, CancellationToken.None,
                        TaskCreationOptions.None, TaskScheduler.Default);

                return RuleResult.Valid();
            });

            // ACT & VERIFY
            await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAllAsync());
        }

        [Fact]
        public void SyncValidation_SeveralRulesForOneTarget_ValidWhenAllRulesAreValid()
        {
            var vm = new DummyViewModel();

            var validation = new ValidationHelper();

            validation.AddRule(vm, RuleResult.Valid);

            validation.AddRule(vm, () => { return RuleResult.Invalid("Rule 2 failed"); });

            validation.AddRule(vm, RuleResult.Valid);

            var r = validation.Validate(vm);

            Assert.False(r.IsValid);
        }

        [Fact]
        public async void SyncValidation_RuleThrowsException_ExceptionIsPropogated()
        {
            // ARRANGE
            var validator = new ValidationHelper();

            validator.AddRule(() => { throw new InvalidOperationException("Test"); });

            // ACT & VERIFY
            await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAllAsync());
        }

        [Fact]
        public async void MixedValidation_SyncRuleThrowsExceptionAfterSuccesfullAsyncRule_ExceptionIsPropogated()
        {
            // ARRANGE
            var validator = new ValidationHelper();

            validator.AddAsyncRule(async () =>
            {
                return await Task.Run(() => RuleResult.Valid());
            });

            validator.AddRule(() => { throw new InvalidOperationException("Test"); });

            // ACT & VERIFY
            var task = Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAllAsync());
            if (task != await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5))))
            {
                Assert.True(false, "Looks like the validation is stuck (didn't complete in a timeout), which means it didn't handle the exception properly.");
            }
        }

        [Fact]
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
                parent.Validator.ValidateAllAsync().ContinueWith(result =>
                {
                    // VERIFY
                    Assert.False(result.Result.IsValid, "Validation must fail");
                    Assert.Equal("Error1", result.Result.ErrorList[0].ErrorText);

                    completedAction();
                });
            });
        }

        [Fact]
        public void AddChildValidatable_AddsRuleWithProperTarget()
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
                parent.Validator.ValidateAllAsync().ContinueWith(result =>
                {
                    // VERIFY
                    Assert.False(result.Result.IsValid, "Validation must fail");
                    Assert.Equal("parent.Child", result.Result.ErrorList[0].Target);

                    completedAction();
                });
            });
        }

        [Fact]
        public void AddChildValidatable_ChildValidatableIsNull_NoErrorsAreAdded()
        {
            TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
            {
                // ARRANGE
                var parent = new ValidatableViewModel();

                parent.Validator.AddChildValidatable(() => parent.Child);

                // ACT
                parent.Validator.ValidateAllAsync().ContinueWith(result =>
                {
                    // VERIFY
                    Assert.True(result.Result.IsValid, "Validation must not fail");

                    completedAction();
                });
            });
        }

        [Fact]
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
                parent.Validator.ValidateAllAsync().ContinueWith(result =>
                {
                    // VERIFY
                    Assert.False(result.Result.IsValid, "Validation must fail");
                    Assert.Equal("Error1", result.Result.ErrorList[0].ErrorText);

                    completedAction();
                });
            });
        }

        [Fact]
        public void AddChildValidatableCollection_MoreThan64Items_DoesNotFail()
        {
            TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
            {
                // ARRANGE
                var parent = new ValidatableViewModel();

                var children = new List<IValidatable>();

                for (int i = 0; i < 65; i++)
                {
                    var child = new ValidatableViewModel();
                    child.Validator.AddRule(RuleResult.Valid);

                    children.Add(child);
                }

                parent.Children = children;

                parent.Validator.AddChildValidatableCollection(() => parent.Children);

                // ACT
                parent.Validator.ValidateAllAsync().ContinueWith(result =>
                {
                    // VERIFY
                    uiThreadDispatcher.BeginInvoke(new Action(() =>
                    {
                        Assert.Null(result.Exception);
                        Assert.True(result.Result.IsValid, "Validation must pass");

                        completedAction();
                    }));
                });
            });
        }

        [Fact]
        public void AddChildValidatableCollection_ChildCollectionIsNullOrEmpty_NoErrorsAreAdded()
        {
            TestUtils.ExecuteWithDispatcher((uiThreadDispatcher, completedAction) =>
            {
                // ARRANGE
                var parent = new ValidatableViewModel();

                parent.Validator.AddChildValidatableCollection(() => parent.Children);

                // ACT
                parent.Validator.ValidateAllAsync().ContinueWith(r1 =>
                {
                    // VERIFY
                    Assert.True(r1.Result.IsValid, "Validation must not fail");

                    // ARRANGE
                    parent.Children = new List<IValidatable>();

                    // ACT
                    parent.Validator.ValidateAllAsync().ContinueWith(r2 =>
                    {
                        // VERIFY
                        Assert.True(r2.Result.IsValid, "Validation must not fail.");

                        completedAction();
                    });
                });
            });
        }

        [Fact]
        public void AddChildValidatableCollection_OneRuleThrowsException_EntireValidationFails()
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

                parent.Validator.AddAsyncRule(() => TaskEx.FromResult(RuleResult.Valid()));
                child1.Validator.AddAsyncRule(
                    () => Task.Factory.StartNew<RuleResult>(() => { throw new FakeException(); }));

                child2.Validator.AddAsyncRule(() => TaskEx.FromResult(RuleResult.Valid()));

                parent.Validator.AddChildValidatableCollection(() => parent.Children);

                // ACT
                parent.Validator.ValidateAllAsync().ContinueWith(r1 =>
                {
                    uiThreadDispatcher.BeginInvoke(new Action(() =>
                    {
                        Assert.NotNull(r1.Exception);
                        Assert.IsType<FakeException>(ExceptionUtils.UnwrapException(r1.Exception));
                        completedAction();
                    }));
                });
            });
        }

        [Fact]
        public void ErrorChanged_RaisedOnUIThread()
        {
            TestUtils.ExecuteWithDispatcher((dispatcher, completedAction) =>
            {
                var vm = new DummyViewModel();
                var syncEvent = new ManualResetEvent(false);

                vm.Validator.AddAsyncRule(nameof(vm.Foo), () => { return Task.Run(() => RuleResult.Invalid("Test")); });

                vm.ErrorsChanged += (o, e) =>
                {
                    var threadId = Thread.CurrentThread.ManagedThreadId;

                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        Assert.Equal(dispatcher.Thread.ManagedThreadId, threadId);
                        syncEvent.Set();
                    }));
                };

                vm.Validate();

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    if (!syncEvent.WaitOne(TimeSpan.FromSeconds(5)))
                    {
                        dispatcher.BeginInvoke(
                            new Action(
                                () =>
                                    Assert.True(false,
                                        "ErrorsChanged was not raised within specified timeout (5 sec)")));
                    }

                    completedAction();
                });
            });
        }

        [Fact]
        public void ConditionalRules_ConditionIsFalse_RuleIsSkipped()
        {
            // ARRANGE
            var validator = new ValidationHelper();

            validator.AddRule(() => RuleResult.Invalid("Should not be executed."))
                     .WithSettings(s => s.EnabledWhen(() => false));
            
            // ACT
            var result = validator.ValidateAll();
            
            // VERIFY
            Assert.True(result.IsValid);
        }
        
        [Fact]
        public void ConditionalRules_ConditionIsTrue_RuleIsExecutued()
        {
            // ARRANGE
            var validator = new ValidationHelper();

            validator.AddRule(() => RuleResult.Invalid("Should be executed."))
                     .WithSettings(s => s.EnabledWhen(() => true));
            
            // ACT
            var result = validator.ValidateAll();
            
            // VERIFY
            Assert.False(result.IsValid);
        }
        
        [Fact]
        public void ConditionalRules_TwoRules_FirstIsTrue_SecondIsFalse_RuleIsSkipped()
        {
            // ARRANGE
            var validator = new ValidationHelper();

            validator.AddRule(() => RuleResult.Invalid("Should not be executed."))
                     .WithSettings(s => s.EnabledWhen(() => true).EnabledWhen(() => false));
            
            // ACT
            var result = validator.ValidateAll();
            
            // VERIFY
            Assert.True(result.IsValid);
        }
    }

    // ReSharper restore InconsistentNaming
}