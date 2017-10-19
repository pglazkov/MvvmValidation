[![Build status](https://ci.appveyor.com/api/projects/status/cxp4fdhrhqhrq127?svg=true)](https://ci.appveyor.com/project/pglazkov/mvvmvalidation)

# MVVM Validation Helpers
MVVM Validation Helpers is a lightweight library that makes it easier for developers to implement validation in MVVM applications. 

It allows you to:
* Define and keep all your validation rules conveniently in one place.
* Reduce boilerplate of maintaining error list for each of the validation targets (properties).
* Validate a target and get the validation result back without worrying what rules need to be checked.
* Easily implement `INotifyDataErrorInfo` interface in your view models to integrate with XAML binding engine for displaying validation errors in your views (see `NotifyDataErrorAdapter` below).

## Getting Started
Install the [NuGet package](https://www.nuget.org/packages/MvvmValidation).

OR

Download the binaries from [Releases](https://github.com/pglazkov/MvvmValidation/releases).

## Examples
**Creating an instance of the helper**

In order to use the library you need to create an instance of the `ValidationHelper` helper class and store it in an instance field of the class where you want to implement validation. 
If you do a lot of validation, it is probably the best to do it in your `ViewModelBase` class.
```cs
public class ViewModelBase
{
    public ViewModelBase()
    {
        Validator = new ValidationHelper();
    }
    
    protected ValidationHelper Validator { get; private set; }
}
```    
**Adding a simple validation rule**
```cs
Validator.AddRule(nameof(FirstName),
                  () => RuleResult.Assert(!string.IsNullOrEmpty(FirstName), "First Name is required"));
```
OR
```cs
Validator.AddRequiredRule(() => FirstName, "First Name is required");
```
**Adding a rule that depends on two different properties**

Such a rule will be executed whenever you validate either of those properties.
```cs
Validator.AddRule(nameof(RangeStart),
                  nameof(RangeEnd),
                  () => RuleResult.Assert(RangeEnd > RangeStart, 
                                          "RangeEnd must be grater than RangeStart");
```
**Adding an asynchronous rule**

Such rule can perform more complex validation that may take long time or cannot be executed synchronously, for example, a call to a web service.
```cs
Validator.AddAsyncRule(nameof(UserName),
    async () =>
    {
        var isAvailable = await UserRegistrationService.IsUserNameAvailable(UserName).ToTask();

        return RuleResult.Assert(isAvailable, 
            string.Format("User Name {0} is taken. Please choose a different one.", UserName));
    });
```
**Executing validation**
```cs
// Validate all (execute all validation rules)
ValidationResult validationResult = Validator.ValidateAll();

// Validate a specific target
ValidationResult validationResult = Validator.Validate(nameof(FirstName));
```
**Executing validation asynchronously**
```cs
// Validate all (execute all validation rules)
ValidationResult validationResult = await Validator.ValidateAllAsync();

// Validate a specific target
ValidationResult validationResult = await Validator.ValidateAsync(nameof(FirstName));
```
**Getting current validation state at any point of time**

Any time you can request current validation state for the entire object or for specific validation targets.
```cs
// Get validation result for the entire object
var validationResult = Validator.GetResult();

// Get validation result for a target
var firstNameValidationResult = Validator.GetResult(nameof(FirstName));
```
**Receive notifications when validation result changes**
```cs
Validator.ResultChanged += OnValidationResultChanged;

private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
{
    // Get current state of the validation
    ValidationResult validationResult = Validator.GetResult();

    UpdateValidationSummary(validationResult);
}
```
**Implement `INotifyDataErrorInfo` interface**

The library includes `NotifyDataErrorAdapter` class that makes the implementation of `INotifyDataErrorInfo` interface in your view models trivial.

```cs
public class ValidatableViewModelBase : INotifyDataErrorInfo
{
    protected ValidationHelper Validator { get; private set; }
    private NotifyDataErrorInfoAdapter NotifyDataErrorInfoAdapter { get; set; }

    public ValidatableViewModelBase()
    {
        Validator = new ValidationHelper();

        NotifyDataErrorInfoAdapter = new NotifyDataErrorInfoAdapter(Validator);
    }

    public IEnumerable GetErrors(string propertyName)
    {
        return NotifyDataErrorInfoAdapter.GetErrors(propertyName);
    }

    public bool HasErrors
    {
        get { return NotifyDataErrorInfoAdapter.HasErrors; }
    }

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
    {
        add { NotifyDataErrorInfoAdapter.ErrorsChanged += value; }
        remove { NotifyDataErrorInfoAdapter.ErrorsChanged -= value; }
    }
}
```

**For more examples download the source code and check out the example project.**

![Sample UI Screenshot](/Examples/screenshot.png)

### Advanced Use Cases

**Execute rules even if the target is already invalid**

By default, if the first rule for the target failed, the remaining rules for that target are skipped. For example, if the "Email" field is required it doesn't make sense to check that the email address is in valid format until user has entered something, so first we execute the "required" rule and only if it succeds we execute the next rule to verify the format of the email. 

This default behavior can be overriden either globally or per rule:

Globally:

```cs
Validator = new ValidationHelper(new ValidationSettings
{
    DefaultRuleSettings = new ValidationRuleSettings
    {
        ExecuteOnAlreadyInvalidTarget = true
    }
});
```

Per Rule:

```cs
Validator.AddAsyncRule(/* Rule */)
         .WithSettings(s => s.ExecuteOnAlreadyInvalidTarget = false);
```
