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

		private readonly IDictionary<object, IDictionary<ValidationRule, RuleResult>> ruleValidationResultMap =
			new Dictionary<object, IDictionary<ValidationRule, RuleResult>>();

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
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddRule(object target, Func<RuleResult> validateDelegate)
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
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddRule(Func<RuleResult> validateDelegate)
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
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, , () => RuleResult.Assert(Foo > 10, "Foo must be greater than 10"))
		/// </code>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddRule(Expression<Func<object>> propertyExpression, Func<RuleResult> validateDelegate)
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
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		/// <example>
		/// <code>
		/// AddRule(() => Foo, () => Bar, () => RuleResult.Assert(Foo > Bar, "Foo must be greater than bar"))
		/// </code>
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddRule(Expression<Func<object>> property1Expression, Expression<Func<object>> property2Expression,
		                    Func<RuleResult> validateDelegate)
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
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddRule(IEnumerable<Expression<Func<object>>> properties, Func<RuleResult> validateDelegate)
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
		/// <param name="target">The validation target (object that is being validated by <paramref name="validateAction"/>).</param>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddAsyncRule(object target, AsyncRuleValidateAction validateAction)
		{
			Contract.Requires(target != null);
			Contract.Requires(validateAction != null);

			AddRuleCore(new GenericValidationTarget(target), null, validateAction);
		}

		/// <summary>
		/// Adds an asynchronious validation rule.
		/// </summary>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		public void AddAsyncRule(AsyncRuleValidateAction validateAction)
		{
			Contract.Requires(validateAction != null);

			AddRuleCore(new UndefinedValidationTarget(), null, validateAction);
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a property of an object. The target property is specified in the <paramref name="propertyExpression"/> parameter.
		/// </summary>
		/// <param name="propertyExpression">The target property expression.</param>
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
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddAsyncRule(Expression<Func<object>> propertyExpression, AsyncRuleValidateAction validateAction)
		{
			Contract.Requires(propertyExpression != null);
			Contract.Requires(validateAction != null);

			AddAsyncRule(new[] {propertyExpression}, validateAction);
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates two dependent properties.
		/// </summary>
		/// <param name="property1Expression">The first target property expression.</param>
		/// <param name="property2Expression">The second target property expression.</param>
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
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddAsyncRule(Expression<Func<object>> property1Expression, Expression<Func<object>> property2Expression,
		                         AsyncRuleValidateAction validateAction)
		{
			Contract.Requires(property1Expression != null);
			Contract.Requires(property2Expression != null);
			Contract.Requires(validateAction != null);

			AddAsyncRule(new[] {property1Expression, property2Expression}, validateAction);
		}

		/// <summary>
		/// Adds an asynchronious validation rule that validates a collection of dependent properties.
		/// </summary>
		/// <param name="properties">The collection of target property expressions. </param>
		/// <param name="validateAction">
		/// The validation delegate - a function that performs asyncrhonious validation and calls a continuation callback with an instance 
		/// of <see cref="RuleResult"/> that indicated whether the rule has passed and 
		/// a collection of errors (in not passed).
		/// </param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddAsyncRule(IEnumerable<Expression<Func<object>>> properties, AsyncRuleValidateAction validateAction)
		{
			Contract.Requires(properties != null);
			Contract.Requires(properties.Any());
			Contract.Requires(validateAction != null);

			IValidationTarget target = CreatePropertyValidationTarget(properties);

			AddRuleCore(target, null, validateAction);
		}

		private void AddRuleCore(IValidationTarget target, Func<RuleResult> validateDelegate,
		                         AsyncRuleValidateAction asyncValidateAction)
		{
			var rule = new ValidationRule(target, validateDelegate, asyncValidateAction);

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
			return GetResultInternal();
		}

		/// <summary>
		/// Returns the current validation state for the given <paramref name="target"/> (all errors tracked by this instance of <see cref="ValidationHelper"/>).
		/// </summary>
		/// <param name="target">The validation target for which to retrieve the validation state.</param>
		/// <returns>An instance of <see cref="ValidationResult"/> that contains an indication whether the object is valid and a collection of errors if not.</returns>
		public ValidationResult GetResult(object target)
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
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ValidationResult GetResult(Expression<Func<object>> propertyExpression)
		{
			Contract.Requires(propertyExpression != null);

			return GetResult(PropertyName.For(propertyExpression));
		}

		private ValidationResult GetResultInternal(object target)
		{
			ValidationResult result = ValidationResult.Valid;

			IDictionary<ValidationRule, RuleResult> ruleResultMap;

			if (ruleValidationResultMap.TryGetValue(target, out ruleResultMap))
			{
				foreach (var ruleValidationResult in ruleResultMap.Values)
				{
					result = result.Combine(new ValidationResult(target, ruleValidationResult.Errors));
				}
			}
			return result;
		}

		private ValidationResult GetResultInternal()
		{
			ValidationResult result = ValidationResult.Valid;

			foreach (var ruleResultsMapPair in ruleValidationResultMap)
			{
				var ruleTarget = ruleResultsMapPair.Key;
				var ruleResultsMap = ruleResultsMapPair.Value;

				foreach (var validationResult in ruleResultsMap.Values)
				{
					result = result.Combine(new ValidationResult(ruleTarget, validationResult.Errors));
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
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ValidationResult Validate(Expression<Func<object>> propertyPathExpression)
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
		public ValidationResult Validate(object target)
		{
			Contract.Requires(target != null);

			return ValidateInternal(target);
		}

		/// <summary>
		/// Executes validation using all validation rules. 
		/// </summary>
		/// <returns>Result that indicates whether the validation was succesfull and a collection of errors, if it wasn't.</returns>
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

		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty).</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void ValidateAsync(Expression<Func<object>> propertyPathExpression)
		{
			ValidateAsync(propertyPathExpression, null);
		}

		/// <summary>
		/// Executes validation for the given property asynchronously. 
		/// Executes all (normal and async) validation rules for the property specified in the <paramref name="propertyPathExpression"/>.
		/// </summary>
		/// <param name="propertyPathExpression">Expression for the property to validate. Example: ValidateAsync(() => MyProperty, ...).</param>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void ValidateAsync(Expression<Func<object>> propertyPathExpression, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(propertyPathExpression != null);

			ValidateAsync(PropertyName.For(propertyPathExpression), onCompleted);
		}

		/// <summary>
		/// Executes validation for the given target asynchronously. 
		/// Executes all (normal and async) validation rules for the target object specified in the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		public void ValidateAsync(object target)
		{
			ValidateAsync(target, null);
		}

		/// <summary>
		/// Executes validation for the given target asynchronously. 
		/// Executes all (normal and async) validation rules for the target object specified in the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target object to validate.</param>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
		public void ValidateAsync(object target, Action<ValidationResult> onCompleted)
		{
			Contract.Requires(target != null);

			ValidateInternalAsync(target, onCompleted);
		}

		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		public void ValidateAllAsync()
		{
			ValidateAllAsync(null);
		}

		/// <summary>
		/// Executes validation using all validation rules asynchronously.
		/// </summary>
		/// <param name="onCompleted">Callback to execute when the asynchronous validation is completed. The callback will be executed on the UI thread.</param>
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

		private ValidationResult ExecuteValidationRules(object target = null)
		{
			Func<ValidationRule, bool> ruleFilter = CreateRuleFilterFor(target);

			ValidationRule[] rulesToExecute = ValidationRules.Where(ruleFilter).ToArray();

			ValidationResult result = ExecuteValidationRulesCore(rulesToExecute);

			return result;
		}

		private void ExecuteValidationRulesAsync(object target, Action<ValidationResult> completed)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				ValidationResult result = ExecuteValidationRules(target);

				completed(result);
			});
		}

		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ValidateAsync")]
		private ValidationResult ExecuteValidationRulesCore(IEnumerable<ValidationRule> rulesToExecute)
		{
			var result = new ValidationResult();

			var failedTargets = new HashSet<object>();

			foreach (ValidationRule validationRule in rulesToExecute)
			{
				// Skip rule if the target is already invalid
				if (failedTargets.Contains(validationRule.Target))
				{
					// Assume that the rule is valid at this point because we are not interested in this error until
					// previous rule is fixed.
					SaveRuleValidationResultAndNotifyIfNeeded(validationRule, RuleResult.Valid());

					continue;
				}

				RuleResult ruleResult = ExecuteRuleCore(validationRule);

				SaveRuleValidationResultAndNotifyIfNeeded(validationRule, ruleResult);

				AddErrorsFromRuleResult(result, validationRule, ruleResult);

				if (!ruleResult.IsValid)
				{
					failedTargets.Add(validationRule.Target);
				}
			}

			return result;
		}

		private RuleResult ExecuteRuleCore(ValidationRule rule)
		{
			RuleResult result = RuleResult.Valid();

			if (!rule.SupportsSyncValidation)
			{
				var completedEvent = new ManualResetEvent(false);

				rule.EvaluateAsync(r =>
				{
					result = r;
					completedEvent.Set();
				});

				completedEvent.WaitOne();
			}
			else
			{
				result = rule.Evaluate();
			}

			return result;
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

		private void SaveRuleValidationResultAndNotifyIfNeeded(ValidationRule rule, RuleResult ruleResult)
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
						targetRuleMap[rule] = ruleResult;

						// Notify that validation result for the target has changed
						NotifyResultChanged(ruleTarget, GetResult(ruleTarget));
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

		#endregion

		#region ResultChanged

		/// <summary>
		/// Occurs when the validation result have changed for a property or for the entire entity (the result that is returned by the <see cref="GetResult()"/> method).
		/// </summary>
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