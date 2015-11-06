using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	/// <summary>
	/// Main helper class that contains the functionality of managing validation rules, 
	/// executing validation using those rules and keeping validation results.
	/// </summary>
	public partial class ValidationHelper
	{
		#region Fields

		private readonly IDictionary<object, IDictionary<ValidationRule, RuleResult>> ruleValidationResultMap =
			new Dictionary<object, IDictionary<ValidationRule, RuleResult>>();

		private readonly object syncRoot = new object();
		private bool isValidationSuspended;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationHelper"/> class.
		/// </summary>
		public ValidationHelper()
		{
			ValidationRules = new ValidationRuleCollection();
		}

		#endregion

		#region Properties

		private ValidationRuleCollection ValidationRules { get; set; }

		/// <summary>
		/// Gets or sets a timeout that indicates how much time is allocated for an async rule to complete.
		/// If a rule did not complete in this timeout, then an exception will be thrown.
		/// </summary>
		[Obsolete("This property has no effect anymore. The library always waits until the rule completes.")]
		public TimeSpan AsyncRuleExecutionTimeout { get; set; }

		/// <summary>
		/// Indicates whether the validation is currently suspended using the <see cref="SuppressValidation"/> method.
		/// </summary>
		public bool IsValidationSuspended
		{
			get { return isValidationSuspended; }
		}

		#endregion

		#region Rules Construction

		/// <summary>
		/// Adds a validation rule that validates the <paramref name="target"/> object.
		/// </summary>
		/// <param name="target">The validation target (object that is being validated by <paramref name="validateDelegate"/>).</param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull]
		public IValidationRule AddRule([NotNull] object target, [NotNull] Func<RuleResult> validateDelegate)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateDelegate != null);
			Contract.Ensures(Contract.Result<IValidationRule>() != null);

			var rule = AddRuleCore(new GenericValidationTarget(target), validateDelegate, null);

			return rule;
		}

		/// <summary>
		/// Adds a simple validation rule.
		/// </summary>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull]
		public IValidationRule AddRule([NotNull] Func<RuleResult> validateDelegate)
		{
			Contract.Requires(validateDelegate != null);
			Contract.Ensures(Contract.Result<IValidationRule>() != null);

			var rule = AddRuleCore(new UndefinedValidationTarget(), validateDelegate, null);

			return rule;
		}

		/// <summary>
		/// Adds a validation rule that validates a property of an object. The target property is specified in the <paramref name="propertyExpression"/> parameter.
		/// </summary>
		/// <param name="propertyExpression">The target property expression. Example: AddRule(() => MyProperty, ...).</param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, , () => RuleResult.Assert(Foo > 10, "Foo must be greater than 10"))
		/// </code>
		/// </example>
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IValidationRule AddRule([NotNull] Expression<Func<object>> propertyExpression, [NotNull] Func<RuleResult> validateDelegate)
		{
			Contract.Requires(propertyExpression != null);
			Contract.Requires(validateDelegate != null);
			Contract.Ensures(Contract.Result<IValidationRule>() != null);

			var rule = AddRule(new[] { propertyExpression }, validateDelegate);

			return rule;
		}

		/// <summary>
		/// Adds a validation rule that validates two dependent properties.
		/// </summary>
		/// <param name="property1Expression">The first target property expression. Example: AddRule(() => MyProperty, ...).</param>
		/// <param name="property2Expression">The second target property expression. Example: AddRule(..., () => MyProperty, ...).</param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, () => Bar, () => RuleResult.Assert(Foo > Bar, "Foo must be greater than bar"))
		/// </code>
		/// </example>
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IValidationRule AddRule([NotNull] Expression<Func<object>> property1Expression, [NotNull] Expression<Func<object>> property2Expression, [NotNull] Func<RuleResult> validateDelegate)
		{
			Contract.Requires(property1Expression != null);
			Contract.Requires(property2Expression != null);
			Contract.Requires(validateDelegate != null);
			Contract.Ensures(Contract.Result<IValidationRule>() != null);

			var rule = AddRule(new[] { property1Expression, property2Expression }, validateDelegate);

			return rule;
		}

		/// <summary>
		/// Adds a validation rule that validates a collection of dependent properties.
		/// </summary>
		/// <param name="properties">The collection of target property expressions. Example: AddRule(new [] { () => MyProperty1, () => MyProperty2, () => MyProperty3 }, ...).</param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <returns>An instance of <see cref="IValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IValidationRule AddRule([NotNull] IEnumerable<Expression<Func<object>>> properties, [NotNull] Func<RuleResult> validateDelegate)
		{
			Contract.Requires(properties != null);
			Contract.Requires(properties.Any());
			Contract.Requires(validateDelegate != null);
			Contract.Ensures(Contract.Result<IValidationRule>() != null);

			IValidationTarget target = CreatePropertyValidationTarget(properties);

			var rule = AddRuleCore(target, validateDelegate, null);

			return rule;
		}

		#region Async Rules

		/// <summary>
		/// Adds an asynchronious validation rule that validates the <paramref name="target"/> object.
		/// </summary>
		/// <param name="target">The validation target (object that is being validated by <paramref name="validateAction"/>).</param>
		/// <param name="validateAction">The validation delegate - a function that performs asyncrhonious validation.</param>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull]
		public IAsyncValidationRule AddAsyncRule([NotNull] object target, [NotNull] Func<Task<RuleResult>> validateAction)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateAction != null);
			Contract.Ensures(Contract.Result<IAsyncValidationRule>() != null);

			var rule = AddRuleCore(new GenericValidationTarget(target), null, validateAction);

			return rule;
		}

		/// <summary>
		/// Adds an asynchronious validation rule.
		/// </summary>
		/// <param name="validateAction">The validation delegate - a function that performs asyncrhonious validation.</param>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull]
		public IAsyncValidationRule AddAsyncRule([NotNull] Func<Task<RuleResult>> validateAction)
		{
			Contract.Requires(validateAction != null);
			Contract.Ensures(Contract.Result<IAsyncValidationRule>() != null);

			var rule = AddRuleCore(new UndefinedValidationTarget(), null, validateAction);

			return rule;
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a property of an object. The target property is specified in the <paramref name="propertyExpression"/> parameter.
		/// </summary>
		/// <param name="propertyExpression">The target property expression. Example: AddAsyncRule(() => MyProperty, ...).</param>
		/// <param name="validateAction">The validation delegate - a function that performs asyncrhonious validation.</param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, 
		///			() => 
		///         {
		///				return ValidationServiceFacade.ValidateFooAsync(Foo)
		///                 .ContinueWith(t => return RuleResult.Assert(t.Result.IsValid, "Foo must be greater than 10"));
		///			})
		/// </code>
		/// </example>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IAsyncValidationRule AddAsyncRule([NotNull] Expression<Func<object>> propertyExpression, [NotNull] Func<Task<RuleResult>> validateAction)
		{
			Contract.Requires(propertyExpression != null);
			Contract.Requires(validateAction != null);
			Contract.Ensures(Contract.Result<IAsyncValidationRule>() != null);

			var rule = AddAsyncRule(new[] { propertyExpression }.Select(c => c), validateAction);

			return rule;
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates two dependent properties.
		/// </summary>
		/// <param name="property1Expression">The first target property expression. Example: AddRule(() => MyProperty, ...).</param>
		/// <param name="property2Expression">The second target property expression. Example: AddRule(..., () => MyProperty, ...).</param>
		/// <param name="validateAction">The validation delegate - a function that performs asyncrhonious validation.</param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, () => Bar
		///			() => 
		///         {
		///				return ValidationServiceFacade.ValidateFooAndBar(Foo, Bar)
		///                       .ContinueWith(t => RuleResult.Assert(t.Result.IsValid, "Foo must be greater than 10"));
		///			})
		/// </code>
		/// </example>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IAsyncValidationRule AddAsyncRule([NotNull] Expression<Func<object>> property1Expression, [NotNull] Expression<Func<object>> property2Expression, [NotNull] Func<Task<RuleResult>> validateAction)
		{
			Contract.Requires(property1Expression != null);
			Contract.Requires(property2Expression != null);
			Contract.Requires(validateAction != null);
			Contract.Ensures(Contract.Result<IAsyncValidationRule>() != null);

			var rule = AddAsyncRule(new[] { property1Expression, property2Expression }, validateAction);

			return rule;
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a collection of dependent properties.
		/// </summary>
		/// <param name="properties">The collection of target property expressions. Example: AddAsyncRule(new [] { () => MyProperty1, () => MyProperty2, () => MyProperty3 }, ...).</param>
		/// <param name="validateAction">The validation delegate - a function that performs asyncrhonious validation.</param>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IAsyncValidationRule AddAsyncRule([NotNull] IEnumerable<Expression<Func<object>>> properties, [NotNull] Func<Task<RuleResult>> validateAction)
		{
			Contract.Requires(properties != null);
			Contract.Requires(properties.Any());
			Contract.Requires(validateAction != null);
			Contract.Ensures(Contract.Result<IAsyncValidationRule>() != null);

			IValidationTarget target = CreatePropertyValidationTarget(properties);

			var rule = AddRuleCore(target, null, validateAction);

			return rule;
		}

		#endregion

		#region Obsolete

		/// <summary>
		/// Adds an asynchronious validation rule that validates the <paramref name="target"/> object.
		/// </summary>
		/// <param name="target">The validation target (object that is being validated by <paramref name="validateAction"/>).</param>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[Obsolete("Use the overload that takes Func<Task<RuleResult>> as a second parameter instead.")]
		public IAsyncValidationRule AddAsyncRule(object target, AsyncRuleValidateAction validateAction)
		{
			return AddAsyncRule(target, validateAction.ToTaskFunc());
		}

		/// <summary>
		/// Adds an asynchronious validation rule.
		/// </summary>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[Obsolete("Use the overload that takes Func<Task<RuleResult>> as a parameter instead.")]
		public IAsyncValidationRule AddAsyncRule(AsyncRuleValidateAction validateAction)
		{
			return AddAsyncRule(validateAction.ToTaskFunc());
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a property of an object. The target property is specified in the <paramref name="propertyExpression"/> parameter.
		/// </summary>
		/// <param name="propertyExpression">The target property expression. Example: AddAsyncRule(() => MyProperty, ...).</param>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, 
		///			(onCompleted) => 
		///         {
		///				ValidationServiceFacade.ValidateFoo(Foo, result => onCompleted(RuleResult.Assert(result.IsValid, "Foo must be greater than 10")));
		///			})
		/// </code>
		/// </example>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[Obsolete("Use the overload that takes Func<Task<RuleResult>> as a second parameter instead.")]
		public IAsyncValidationRule AddAsyncRule(Expression<Func<object>> propertyExpression,
			AsyncRuleValidateAction validateAction)
		{
			return AddAsyncRule(propertyExpression, validateAction.ToTaskFunc());
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates two dependent properties.
		/// </summary>
		/// <param name="property1Expression">The first target property expression. Example: AddRule(() => MyProperty, ...).</param>
		/// <param name="property2Expression">The second target property expression. Example: AddRule(..., () => MyProperty, ...).</param>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, () => Bar
		///			(onCompleted) => 
		///         {
		///				ValidationServiceFacade.ValidateFooAndBar(Foo, Bar, result => onCompleted(RuleResult.Assert(result.IsValid, "Foo must be greater than 10")));
		///			})
		/// </code>
		/// </example>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[Obsolete("Use the overload that takes Func<Task<RuleResult>> as a third parameter instead.")]
		public IAsyncValidationRule AddAsyncRule(Expression<Func<object>> property1Expression,
			Expression<Func<object>> property2Expression,
			AsyncRuleValidateAction validateAction)
		{
			return AddAsyncRule(property1Expression, property2Expression, validateAction.ToTaskFunc());
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a collection of dependent properties.
		/// </summary>
		/// <param name="properties">The collection of target property expressions. Example: AddAsyncRule(new [] { () => MyProperty1, () => MyProperty2, () => MyProperty3 }, ...).</param>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <returns>An instance of <see cref="IAsyncValidationRule"/> that represents the newly created validation rule.</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[Obsolete("Use the overload that takes Func<Task<RuleResult>> as a second parameter instead.")]
		public IAsyncValidationRule AddAsyncRule(IEnumerable<Expression<Func<object>>> properties,
			AsyncRuleValidateAction validateAction)
		{
			return AddAsyncRule(properties, validateAction.ToTaskFunc());
		}

		#endregion
		
		/// <summary>
		/// Removes the specified <paramref name="rule"/>.
		/// </summary>
		/// <param name="rule">Validation rule instance.</param>
		public void RemoveRule([NotNull] IValidationRule rule)
		{
			Contract.Requires(rule != null);

			var typedRule = rule as ValidationRule;

			Contract.Assert(typedRule != null, string.Format(CultureInfo.InvariantCulture, "Rule must be of type \"{0}\".", typeof(ValidationRule).FullName));

			lock (syncRoot)
			{
				UnregisterValidationRule(typedRule);

				lock (ruleValidationResultMap)
				{
					// Clear the results if any
					foreach (var ruleResultsPair in ruleValidationResultMap)
					{
						bool removed = ruleResultsPair.Value.Remove(typedRule);

						if (removed)
						{
							// Notify that validation result for the target has changed
							NotifyResultChanged(ruleResultsPair.Key, GetResult(ruleResultsPair.Key), null, false);
						}
					}
				}
			}
			
		}

		/// <summary>
		/// Removes all validation rules.
		/// </summary>
		public void RemoveAllRules()
		{
			lock (syncRoot)
			{
				UnregisterAllValidationRules();

				object[] targets;

				lock (ruleValidationResultMap)
				{
					targets = ruleValidationResultMap.Keys.ToArray();

					// Clear the results
					ruleValidationResultMap.Clear();
				}

				// Notify that validation result has changed
				foreach (var target in targets)
				{
					NotifyResultChanged(target, ValidationResult.Valid, null, false);
				}
			}
		}

		private IAsyncValidationRule AddRuleCore(IValidationTarget target, Func<RuleResult> validateDelegate,
												 Func<Task<RuleResult>> asyncValidateAction)
		{
			var rule = new ValidationRule(target, validateDelegate, asyncValidateAction);

			RegisterValidationRule(rule);

			return rule;
		}

		private static IValidationTarget CreatePropertyValidationTarget(IEnumerable<Expression<Func<object>>> properties)
		{
			IValidationTarget target;

			// Getting rid of thw "Possible multiple enumerations of IEnumerable" warning
			properties = properties.ToArray();

			if (properties.Count() == 1)
			{
				target = new PropertyValidationTarget(properties.First());
			}
			else
			{
				target = new PropertyCollectionValidationTarget(properties.Select(p => PropertyName.For(p)));
			}
			return target;
		}

		private void RegisterValidationRule(ValidationRule rule)
		{
			lock (syncRoot)
			{
				ValidationRules.Add(rule);
			}
		}

		private void UnregisterValidationRule(ValidationRule rule)
		{
			lock (syncRoot)
			{
				ValidationRules.Remove(rule);
			}
		}

		private void UnregisterAllValidationRules()
		{
			lock (syncRoot)
			{
				ValidationRules.Clear();
			}
		}

		#endregion

		#region Getting Validation Results

		/// <summary>
		/// Returns the current validation state (all errors tracked by this instance of <see cref="ValidationHelper"/>).
		/// </summary>
		/// <returns>An instance of <see cref="ValidationResult"/> that contains an indication whether the object is valid and a collection of errors if not.</returns>
		[NotNull]
		public ValidationResult GetResult()
		{
			return GetResultInternal();
		}

		/// <summary>
		/// Returns the current validation state for the given <paramref name="target"/> (all errors tracked by this instance of <see cref="ValidationHelper"/>).
		/// </summary>
		/// <param name="target">The validation target for which to retrieve the validation state.</param>
		/// <returns>An instance of <see cref="ValidationResult"/> that contains an indication whether the object is valid and a collection of errors if not.</returns>
		[NotNull]
		public ValidationResult GetResult([NotNull] object target)
		{
			Contract.Requires(target != null);
			Contract.Ensures(Contract.Result<ValidationResult>() != null);

			bool returnAllResults = string.IsNullOrEmpty(target as string);

			ValidationResult result = returnAllResults ? GetResultInternal() : GetResultInternal(target);

			return result;
		}

		/// <summary>
		/// Returns the current validation state for a property represented by <paramref name="propertyExpression"/> (all errors tracked by this instance of <see cref="ValidationHelper"/>).
		/// </summary>
		/// <param name="propertyExpression">The property for which to retrieve the validation state. Example: GetResult(() => MyProperty)</param>
		/// <returns>An instance of <see cref="ValidationResult"/> that contains an indication whether the object is valid and a collection of errors if not.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ValidationResult GetResult([NotNull] Expression<Func<object>> propertyExpression)
		{
			Contract.Requires(propertyExpression != null);
			Contract.Ensures(Contract.Result<ValidationResult>() != null);

			return GetResult(PropertyName.For(propertyExpression));
		}

		private ValidationResult GetResultInternal(object target)
		{
			ValidationResult result = ValidationResult.Valid;

			lock (ruleValidationResultMap)
			{
				IDictionary<ValidationRule, RuleResult> ruleResultMap;

				if (ruleValidationResultMap.TryGetValue(target, out ruleResultMap))
				{
					foreach (var ruleValidationResult in ruleResultMap.Values)
					{
						result = result.Combine(new ValidationResult(target, ruleValidationResult.Errors));
					}
				}
			}

			return result;
			
		}

		private ValidationResult GetResultInternal()
		{
			ValidationResult result = ValidationResult.Valid;

			lock (ruleValidationResultMap)
			{
				foreach (var ruleResultsMapPair in ruleValidationResultMap)
				{
					var ruleTarget = ruleResultsMapPair.Key;
					var ruleResultsMap = ruleResultsMapPair.Value;

					foreach (var validationResult in ruleResultsMap.Values)
					{
						result = result.Combine(new ValidationResult(ruleTarget, validationResult.Errors));
					}
				}
			}

			return result;
		}

		#endregion

		#region Validation Execution

		/// <summary>
		/// Validates (executes validation rules) the property specified in the <paramref name="propertyPathExpression"/> parameter.
		/// </summary>
		/// <param name="propertyPathExpression">Expression that specifies the property to validate. Example: Validate(() => MyProperty).</param>
		/// <returns>Result that indicates whether the given property is valid and a collection of errors, if not valid.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ValidationResult Validate([NotNull] Expression<Func<object>> propertyPathExpression)
		{
			Contract.Requires(propertyPathExpression != null);
			Contract.Ensures(Contract.Result<ValidationResult>() != null);

			return ValidateInternal(PropertyName.For(propertyPathExpression));
		}

		/// <summary>
		/// Validates (executes validation rules) the specified target object.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		/// <returns>Result that indicates whether the given target object is valid and a collection of errors, if not valid.</returns>
		[NotNull]
		public ValidationResult Validate([NotNull] object target)
		{
			Contract.Requires(target != null);

			return ValidateInternal(target);
		}

		/// <summary>
		/// Executes validation using all validation rules. 
		/// </summary>
		/// <returns>Result that indicates whether the validation was succesfull and a collection of errors, if it wasn't.</returns>
		[NotNull]
		public ValidationResult ValidateAll()
		{
			return ValidateInternal(null);
		}

		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ValidateAsync")]
		private ValidationResult ValidateInternal(object target)
		{
			Contract.Ensures(Contract.Result<ValidationResult>() != null);

			ReadOnlyCollection<ValidationRule> rulesToExecute;

			lock (syncRoot)
			{
				if (isValidationSuspended)
				{
					return ValidationResult.Valid;
				}

				rulesToExecute = GetRulesForTarget(target);

				if (rulesToExecute.Any(r => !r.SupportsSyncValidation))
				{
					throw new InvalidOperationException("There are asynchronous rules that cannot be executed synchronously. Please use ValidateAsync method to execute validation instead.");
				}
			}

			try
			{
				ValidationResult validationResult = ExecuteValidationRulesAsync(rulesToExecute).Result;
				return validationResult;
			}
			catch (Exception ex)
			{
				throw new ValidationException("An exception occurred during validation. See inner exception for details.", ExceptionUtils.UnwrapException(ex));
			}
		}

		private Task<ValidationResult> ExecuteValidationRulesAsync(IEnumerable<ValidationRule> rulesToExecute, SynchronizationContext syncContext = null)
		{
			syncContext = syncContext ?? SynchronizationContext.Current;

			var resultTcs = new TaskCompletionSource<ValidationResult>();
			var result = new ValidationResult();
			var failedTargets = new HashSet<object>();
			var rulesQueue = new Queue<ValidationRule>(rulesToExecute.ToArray());

			Action executeRuleFromQueueRecursive = null;

			executeRuleFromQueueRecursive = () =>
			{
				ExecuteNextRuleFromQueueAsync(rulesQueue, failedTargets, result, syncContext).ContinueWith(t =>
				{
					if (t.Exception != null)
					{
						resultTcs.TrySetException(t.Exception);
						return;
					}

					if (t.IsCanceled)
					{
						resultTcs.TrySetCanceled();
						return;
					}

					if (t.Result)
					{
						executeRuleFromQueueRecursive();
						return;
					}

					resultTcs.TrySetResult(result);
				});
			};

			executeRuleFromQueueRecursive();

			return resultTcs.Task;
		}

		private Task<bool> ExecuteNextRuleFromQueueAsync(Queue<ValidationRule> rulesQueue, ISet<object> failedTargets, ValidationResult validationResultAccumulator, SynchronizationContext syncContext)
		{
			if (rulesQueue.Count == 0)
			{
				return TaskEx.FromResult(false);
			}

			var rule = rulesQueue.Dequeue();

			// Skip rule if the target is already invalid
			if (failedTargets.Contains(rule.Target))
			{
				// Assume that the rule is valid at this point because we are not interested in this error until
				// previous rule is fixed.
				SaveRuleValidationResultAndNotifyIfNeeded(rule, RuleResult.Valid(), syncContext);

				return TaskEx.FromResult(true);
			}

			return ExecuteRuleCoreAsync(rule).ContinueWith(t =>
			{
				RuleResult ruleResult = t.Result;

				SaveRuleValidationResultAndNotifyIfNeeded(rule, ruleResult, syncContext);

				AddErrorsFromRuleResult(validationResultAccumulator, rule, ruleResult);

				if (!ruleResult.IsValid)
				{
					failedTargets.Add(rule.Target);
				}

				return true;
			});
		}

		private ReadOnlyCollection<ValidationRule> GetRulesForTarget(object target)
		{
			lock (syncRoot)
			{
				Func<ValidationRule, bool> ruleFilter = CreateRuleFilterFor(target);

				var result = new ReadOnlyCollection<ValidationRule>(ValidationRules.Where(ruleFilter).ToList());

				return result;
			}
		}

		private static Task<RuleResult> ExecuteRuleCoreAsync(ValidationRule rule)
		{
			Task<RuleResult> resultTask;

			if (!rule.SupportsSyncValidation)
			{
				resultTask = rule.EvaluateAsync();
			}
			else
			{
				var tcs = new TaskCompletionSource<RuleResult>();

				var result = rule.Evaluate();

				tcs.SetResult(result);

				resultTask = tcs.Task;
			}

			return resultTask;
		}

		private static void AddErrorsFromRuleResult(ValidationResult resultToAddTo, ValidationRule validationRule,
													RuleResult ruleResult)
		{
			if (!ruleResult.IsValid)
			{
				IEnumerable<object> errorTargets = validationRule.Target.UnwrapTargets();

				foreach (object errorTarget in errorTargets)
				{
					foreach (string ruleError in ruleResult.Errors)
					{
						resultToAddTo.AddError(errorTarget, ruleError);
					}
				}
			}
		}

		private static Func<ValidationRule, bool> CreateRuleFilterFor(object target)
		{
			Func<ValidationRule, bool> ruleFilter = r => true;

			if (target != null)
			{
				ruleFilter = r => r.Target.IsMatch(target);
			}

			return ruleFilter;
		}

		private void SaveRuleValidationResultAndNotifyIfNeeded(ValidationRule rule, RuleResult ruleResult, SynchronizationContext syncContext)
		{
			lock (syncRoot)
			{
				IEnumerable<object> ruleTargets = rule.Target.UnwrapTargets();

				foreach (object ruleTarget in ruleTargets)
				{
					IDictionary<ValidationRule, RuleResult> targetRuleMap = GetRuleMapForTarget(ruleTarget);

					RuleResult currentRuleResult = GetCurrentValidationResultForRule(targetRuleMap, rule);

					if (!Equals(currentRuleResult, ruleResult))
					{
						lock (ruleValidationResultMap)
						{
							targetRuleMap[rule] = ruleResult;
						}

						// Notify that validation result for the target has changed
						NotifyResultChanged(ruleTarget, GetResult(ruleTarget), syncContext);
					}
				}
			}
		}

		private static RuleResult GetCurrentValidationResultForRule(
			IDictionary<ValidationRule, RuleResult> ruleMap, ValidationRule rule)
		{
			lock (ruleMap)
			{
				if (!ruleMap.ContainsKey(rule))
				{
					ruleMap.Add(rule, RuleResult.Valid());
				}

				return ruleMap[rule];
			}
		}

		private IDictionary<ValidationRule, RuleResult> GetRuleMapForTarget(object target)
		{
			lock (ruleValidationResultMap)
			{
				if (!ruleValidationResultMap.ContainsKey(target))
				{
					ruleValidationResultMap.Add(target, new Dictionary<ValidationRule, RuleResult>());
				}

				IDictionary<ValidationRule, RuleResult> ruleMap = ruleValidationResultMap[target];

				return ruleMap;
			}
		}

		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty, ...).</param>
		/// <returns>Task that represents the validation operation.</returns>
		[NotNull, SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Task<ValidationResult> ValidateAsync([NotNull] Expression<Func<object>> propertyPathExpression)
		{
			Contract.Requires(propertyPathExpression != null);
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(PropertyName.For(propertyPathExpression));
		}

		/// <summary>
		/// Executes validation for the given target asynchronously. 
		/// Executes all (normal and async) validation rules for the target object specified in the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		/// <returns>Task that represents the validation operation.</returns>
		[NotNull]
		public Task<ValidationResult> ValidateAsync([NotNull] object target)
		{
			Contract.Requires(target != null);
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(target);
		}

		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		/// <returns>Task that represents the validation operation.</returns>
		[NotNull]
		public Task<ValidationResult> ValidateAllAsync()
		{
			Contract.Ensures(Contract.Result<Task<ValidationResult>>() != null);

			return ValidateInternalAsync(null);
		}

		private Task<ValidationResult> ValidateInternalAsync(object target)
		{
			if (isValidationSuspended)
			{
				return TaskEx.FromResult(ValidationResult.Valid);
			}

			var rulesToExecute = GetRulesForTarget(target);

			var syncContext = SynchronizationContext.Current;

			return Task.Factory.StartNew(() => ExecuteValidationRulesAsync(rulesToExecute, syncContext), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default)
				.Unwrap()
				.ContinueWith(t =>
				{
					if (t.Exception != null)
					{
						throw new ValidationException("An exception occurred during validation. See inner exception for details.", ExceptionUtils.UnwrapException(t.Exception));
					}

					return t.Result;
				});
		}

		#endregion

		#region ResultChanged

		/// <summary>
		/// Occurs when the validation result have changed for a property or for the entire entity (the result that is returned by the <see cref="GetResult()"/> method).
		/// </summary>
		public event EventHandler<ValidationResultChangedEventArgs> ResultChanged;

		private void NotifyResultChanged(object target, ValidationResult newResult, SynchronizationContext syncContext, bool useSyncContext = true)
		{
			syncContext = syncContext ?? SynchronizationContext.Current;

			if (useSyncContext && syncContext != null)
			{
				syncContext.Post(_ => NotifyResultChanged(target, newResult, syncContext, useSyncContext: false), null);
				return;
			}

			var handler = ResultChanged;
			if (handler != null)
			{
				handler(this, new ValidationResultChangedEventArgs(target, newResult));
			}
		}

		#endregion

		#region Misc

		/// <summary>
		/// Suppresses all the calls to the Validate* methods until the returned <see cref="IDisposable"/> is disposed
		/// by calling <see cref="IDisposable.Dispose"/>. 
		/// </summary>
		/// <remarks>
		/// This method is convenient to use when you want to suppress validation when setting initial value to a property. In this case you would
		/// wrap the code that sets the property into a <c>using</c> block. Like this:
		/// <code>
		/// using (Validation.SuppressValidation()) 
		/// {
		///     MyProperty = "Initial Value";
		/// }
		/// </code>
		/// </remarks>
		/// <returns>An instance of <see cref="IDisposable"/> that serves as a handle that you can call <see cref="IDisposable.Dispose"/> on to resume validation. The value can also be used in a <c>using</c> block.</returns>
		[NotNull, SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IDisposable SuppressValidation()
		{
			Contract.Ensures(Contract.Result<IDisposable>() != null);

			isValidationSuspended = true;

			return new DelegateDisposable(() => { isValidationSuspended = false; });
		}

		/// <summary>
		/// Resets the validation state. If there were any broken rules 
		/// then the targets for those rules will become valid again and the <see cref="ResultChanged"/> event will be rised.
		/// </summary>
		public void Reset()
		{
			lock (syncRoot)
			{
				lock (ruleValidationResultMap)
				{
					var targets = ruleValidationResultMap.Keys.ToArray();

					foreach (var target in targets)
					{
						var resultForTarget = GetResultInternal(target);

						ruleValidationResultMap.Remove(target);

						if (!resultForTarget.IsValid)
						{
							NotifyResultChanged(target, ValidationResult.Valid, null, false);
						}
					}
				}
			}
		}

		#endregion
	}
}