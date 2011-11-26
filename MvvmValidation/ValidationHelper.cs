using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MvvmValidation.Internal;

namespace MvvmValidation
{
	/// <summary>
	/// Main helper class that contains the functionality of managing validation rules, 
	/// executing validation using those rules and keeping validation results.
	/// </summary>
	public class ValidationHelper
	{
		#region Fields

		private readonly IDictionary<object, IDictionary<ValidationRule, ValidationResult>> ruleValidationResultMap =
			new Dictionary<object, IDictionary<ValidationRule, ValidationResult>>();

		private readonly object syncRoot = new object();
		private bool isValidationSuspanded;

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

		#endregion

		#region Rules Construction

		/// <summary>
		/// Adds a validation rule that validates the <paramref name="target"/> object.
		/// </summary>
		/// <param name="target">The validation target (object that is being validated by <paramref name="validateDelegate"/>).</param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddRule(object target, Func<RuleValidationResult> validateDelegate)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateDelegate != null);

			AddRuleCore(new GenericValidationTarget(target), validateDelegate, null);
		}

		/// <summary>
		/// Adds a simple validation rule.
		/// </summary>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddRule(Func<RuleValidationResult> validateDelegate)
		{
			Contract.Requires(validateDelegate != null);

			AddRuleCore(new UndefinedValidationTarget(), validateDelegate, null);
		}

		/// <summary>
		/// Adds a validation rule that validates a property of an object. The target property is specified in the <paramref name="propertyExpression"/> parameter.
		/// </summary>
		/// <param name="propertyExpression">The target property expression.</param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, , () => RuleValidationResult.Assert(Foo > 10, "Foo must be greater than 10"))
		/// </code>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddRule(Expression<Func<object>> propertyExpression, Func<RuleValidationResult> validateDelegate)
		{
			Contract.Requires(propertyExpression != null);
			Contract.Requires(validateDelegate != null);

			AddRule(new[] {propertyExpression}, validateDelegate);
		}

		/// <summary>
		/// Adds a validation rule that validates two dependent properties.
		/// </summary>
		/// <param name="property1Expression">The first target property expression.</param>
		/// <param name="property2Expression">The second target property expression.</param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, () => Bar, () => RuleValidationResult.Assert(Foo > Bar, "Foo must be greater than bar"))
		/// </code>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddRule(Expression<Func<object>> property1Expression, Expression<Func<object>> property2Expression,
		                    Func<RuleValidationResult> validateDelegate)
		{
			Contract.Requires(property1Expression != null);
			Contract.Requires(property2Expression != null);
			Contract.Requires(validateDelegate != null);

			AddRule(new[] {property1Expression, property2Expression}, validateDelegate);
		}

		/// <summary>
		/// Adds a validation rule that validates a collection of dependent properties.
		/// </summary>
		/// <param name="properties">The collection of target property expressions. </param>
		/// <param name="validateDelegate">
		/// The validation delegate - a function that returns an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddRule(IEnumerable<Expression<Func<object>>> properties, Func<RuleValidationResult> validateDelegate)
		{
			Contract.Requires(properties != null);
			Contract.Requires(properties.Any());
			Contract.Requires(validateDelegate != null);

			IValidationTarget target = CreatePropertyValidationTarget(properties);

			AddRuleCore(target, validateDelegate, null);
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates the <paramref name="target"/> object.
		/// </summary>
		/// <param name="target">The validation target (object that is being validated by <paramref name="validateCallback"/>).</param>
		/// <param name="validateCallback">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddAsyncRule(object target, AsyncRuleValidateCallback validateCallback)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateCallback != null);

			AddRuleCore(new GenericValidationTarget(target), null, validateCallback);
		}

		/// <summary>
		/// Adds an asynchronious validation rule.
		/// </summary>
		/// <param name="validateCallback">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddAsyncRule(AsyncRuleValidateCallback validateCallback)
		{
			Contract.Requires(validateCallback != null);

			AddRuleCore(new UndefinedValidationTarget(), null, validateCallback);
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a property of an object. The target property is specified in the <paramref name="propertyExpression"/> parameter.
		/// </summary>
		/// <param name="propertyExpression">The target property expression.</param>
		/// <param name="validateCallback">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, 
		///			(onCompleted) => 
		///         {
		///				ValidationServiceFacade.ValidateFoo(Foo, result => onCompleted(RuleValidationResult.Assert(result.IsValid, "Foo must be greater than 10")));
		///			})
		/// </code>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddAsyncRule(Expression<Func<object>> propertyExpression, AsyncRuleValidateCallback validateCallback)
		{
			Contract.Requires(propertyExpression != null);
			Contract.Requires(validateCallback != null);

			AddAsyncRule(new[] {propertyExpression}, validateCallback);
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates two dependent properties.
		/// </summary>
		/// <param name="property1Expression">The first target property expression.</param>
		/// <param name="property2Expression">The second target property expression.</param>
		/// <param name="validateCallback">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, () => Bar
		///			(onCompleted) => 
		///         {
		///				ValidationServiceFacade.ValidateFooAndBar(Foo, Bar, result => onCompleted(RuleValidationResult.Assert(result.IsValid, "Foo must be greater than 10")));
		///			})
		/// </code>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddAsyncRule(Expression<Func<object>> property1Expression, Expression<Func<object>> property2Expression,
		                         AsyncRuleValidateCallback validateCallback)
		{
			Contract.Requires(property1Expression != null);
			Contract.Requires(property2Expression != null);
			Contract.Requires(validateCallback != null);

			AddAsyncRule(new[] {property1Expression, property2Expression}, validateCallback);
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a collection of dependent properties.
		/// </summary>
		/// <param name="properties">The collection of target property expressions. </param>
		/// <param name="validateCallback">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleValidationResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddAsyncRule(IEnumerable<Expression<Func<object>>> properties, AsyncRuleValidateCallback validateCallback)
		{
			Contract.Requires(properties != null);
			Contract.Requires(properties.Any());
			Contract.Requires(validateCallback != null);

			IValidationTarget target = CreatePropertyValidationTarget(properties);

			AddRuleCore(target, null, validateCallback);
		}

		private void AddRuleCore(IValidationTarget target, Func<RuleValidationResult> validateDelegate,
		                         AsyncRuleValidateCallback asyncValidateCallback)
		{
			var rule = new ValidationRule(target, validateDelegate, asyncValidateCallback);

			RegisterValidationRule(rule);
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
			ValidationRules.Add(rule);
		}

		#endregion

		#region Getting Validation Results

		/// <summary>
		/// Returns the current validation state (all errors tracked by this instance of <see cref="ValidationHelper"/>).
		/// </summary>
		/// <returns>An instance of <see cref="ValidationResult"/> that contains an indication whether the object is valid and a collection of errors if not.</returns>
		public ValidationResult GetResult()
		{
			return GetResult(null);
		}

		/// <summary>
		/// Returns the current validation state for the given <paramref name="target"/> (all errors tracked by this instance of <see cref="ValidationHelper"/>).
		/// </summary>
		/// <param name="target">The validation target for which to retrieve the validation state.</param>
		/// <returns>An instance of <see cref="ValidationResult"/> that contains an indication whether the object is valid and a collection of errors if not.</returns>
		public ValidationResult GetResult(object target)
		{
			Contract.Ensures(Contract.Result<ValidationResult>() != null);

			bool returnAllResults = target == null || (string.IsNullOrEmpty(target as string));

			ValidationResult result = returnAllResults ? GetResultInternal() : GetResultInternal(target);

			return result;
		}

		private ValidationResult GetResultInternal(object target)
		{
			ValidationResult result = ValidationResult.Valid;

			IDictionary<ValidationRule, ValidationResult> ruleResultMap;

			if (ruleValidationResultMap.TryGetValue(target, out ruleResultMap))
			{
				foreach (ValidationResult ruleValidationResult in ruleResultMap.Values)
				{
					result = result.MergeWith(ruleValidationResult);
				}
			}
			return result;
		}

		private ValidationResult GetResultInternal()
		{
			ValidationResult result = ValidationResult.Valid;

			foreach (var ruleResultsMap in ruleValidationResultMap.Values)
			{
				foreach (ValidationResult validationResult in ruleResultsMap.Values)
				{
					result = result.MergeWith(validationResult);
				}
			}
			return result;
		}

		#endregion

		#region Validation Execution

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ValidationResult Validate(Expression<Func<object>> propertyPathExpression)
		{
			Contract.Requires(propertyPathExpression != null);
			Contract.Ensures(Contract.Result<ValidationResult>() != null);

			return ValidateInternal(PropertyName.For(propertyPathExpression));
		}

		public ValidationResult Validate(object target)
		{
			Contract.Requires(target != null);

			return ValidateInternal(target);
		}

		public ValidationResult ValidateAll()
		{
			return ValidateInternal(null);
		}

		private ValidationResult ValidateInternal(object target)
		{
			Contract.Ensures(Contract.Result<ValidationResult>() != null);

			if (isValidationSuspanded)
			{
				return ValidationResult.Valid;
			}

			ValidationResult validationResult = ExecuteValidationRules(target);

			return validationResult;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void ValidateAsync(Expression<Func<object>> propertyPathExpression)
		{
			ValidateAsync(propertyPathExpression, null);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void ValidateAsync(Expression<Func<object>> propertyPathExpression, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(propertyPathExpression != null);

			ValidateAsync(PropertyName.For(propertyPathExpression), onCompleted);
		}

		public void ValidateAsync(object target)
		{
			ValidateAsync(target, null);
		}

		public void ValidateAsync(object target, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(target != null);

			ValidateInternalAsync(target, onCompleted);
		}

		public void ValidateAllAsync()
		{
			ValidateAllAsync(null);
		}

		public void ValidateAllAsync(Action<ValidationResult> onCompleted)
		{
			ValidateInternalAsync(null, onCompleted);
		}

		private void ValidateInternalAsync(object target, Action<ValidationResult> onCompleted)
		{
			onCompleted = onCompleted ?? (r => { });

			if (isValidationSuspanded)
			{
				onCompleted(ValidationResult.Valid);
				return;
			}

			ExecuteValidationRulesAsync(target, r => ThreadingUtils.RunOnUI(() => onCompleted(r)));
		}

		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ValidateAsync")]
		private ValidationResult ExecuteValidationRules(object target = null)
		{
			Func<ValidationRule, bool> ruleFilter = CreateRuleFilterFor(target);

			var result = new ValidationResult(target);

			ValidationRule[] rulesToExecute = ValidationRules.Where(ruleFilter).ToArray();

			// Check that all the rules support syncronious validation
			if (rulesToExecute.Any(r => !r.SupportsSyncValidation))
			{
				throw new InvalidOperationException(
					"There are asynchronous validation rules for this target that cannot be executed synchronously. Please call ValidateAsync instead.");
			}

			foreach (ValidationRule validationRule in rulesToExecute)
			{
				RuleValidationResult ruleResult = validationRule.Evaluate();

				SaveRuleValidationResultAndNotifyIfNeeded(validationRule, ruleResult);

				AddErrorsFromRuleResult(result, validationRule, ruleResult);
			}

			return result;
		}

		private void ExecuteValidationRulesAsync(object target, Action<ValidationResult> completed)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				Func<ValidationRule, bool> ruleFilter = CreateRuleFilterFor(target);

				var result = new ValidationResult(target);

				IEnumerable<ValidationRule> rulesToExecute =
					ValidationRules.Where(ruleFilter).Where(r => r.SupportsAsyncValidation);

				var events = new List<ManualResetEvent>();

				foreach (ValidationRule validationRule in rulesToExecute)
				{
					ValidationRule rule = validationRule;

					var completedEvent = new ManualResetEvent(false);
					events.Add(completedEvent);

					validationRule.EvaluateAsync(ruleResult =>
					{
						SaveRuleValidationResultAndNotifyIfNeeded(rule, ruleResult);

						AddErrorsFromRuleResult(result, rule, ruleResult);

						completedEvent.Set();
					});
				}

				if (events.Any())
				{
					WaitHandle.WaitAll(events.ToArray());
				}

				completed(result);
			});
		}

		private static void AddErrorsFromRuleResult(ValidationResult resultToAddTo, ValidationRule validationRule,
		                                            RuleValidationResult ruleResult)
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

		private void SaveRuleValidationResultAndNotifyIfNeeded(ValidationRule rule, RuleValidationResult ruleValidationResult)
		{
			lock (syncRoot)
			{
				IEnumerable<object> ruleTargets = rule.Target.UnwrapTargets();

				foreach (object ruleTarget in ruleTargets)
				{
					IDictionary<ValidationRule, ValidationResult> targetRuleMap = GetRuleMapForTarget(ruleTarget);

					ValidationResult currentRuleResult = GetCurrentValidationResultForRule(targetRuleMap, rule);

					if (currentRuleResult.IsValid != ruleValidationResult.IsValid)
					{
						targetRuleMap[rule] = ruleValidationResult.IsValid
						                      	? ValidationResult.Valid
						                      	: new ValidationResult(ruleTarget, ruleValidationResult.Errors);

						// Notify that validation result for the target has changed
						NotifyResultChanged(ruleTarget, GetResult(ruleTarget));
					}
				}
			}
		}

		private static ValidationResult GetCurrentValidationResultForRule(
			IDictionary<ValidationRule, ValidationResult> ruleMap, ValidationRule rule)
		{
			lock (ruleMap)
			{
				if (!ruleMap.ContainsKey(rule))
				{
					ruleMap.Add(rule, ValidationResult.Valid);
				}

				return ruleMap[rule];
			}
		}

		private IDictionary<ValidationRule, ValidationResult> GetRuleMapForTarget(object target)
		{
			lock (ruleValidationResultMap)
			{
				if (!ruleValidationResultMap.ContainsKey(target))
				{
					ruleValidationResultMap.Add(target, new Dictionary<ValidationRule, ValidationResult>());
				}

				IDictionary<ValidationRule, ValidationResult> ruleMap = ruleValidationResultMap[target];

				return ruleMap;
			}
		}

		#endregion

		#region ResultChanged

		public event EventHandler<ValidationResultChangedEventArgs> ResultChanged;

		private void NotifyResultChanged(object target, ValidationResult newResult)
		{
			ThreadingUtils.RunOnUI(() =>
			{
				EventHandler<ValidationResultChangedEventArgs> handler = ResultChanged;
				if (handler != null)
				{
					handler(this, new ValidationResultChangedEventArgs(target, newResult));
				}
			});
		}

		#endregion

		#region Misc

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IDisposable SuppressValidation()
		{
			Contract.Ensures(Contract.Result<IDisposable>() != null);

			isValidationSuspanded = true;

			return new DelegateDisposable(() => { isValidationSuspanded = false; });
		}

		#endregion
	}
}