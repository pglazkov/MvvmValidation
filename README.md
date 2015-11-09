# MVVM Validation Helpers
MVVM Validation Helpers is a little library that makes it easier for developers to implement validation in their WPF/Silverlight MVVM applications. You'll no longer have to implement IDataErrorInfo and INotifyDataErrorInfo interfaces manually in your view models. 

With this lightweight library you can define and keep all your validation rules conveniently in one place. It saves you from all the boilerplate of maintaining error list for each of the validation targets (properties). You just define a set of validation rules that need to be checked for each of the targets and later, when appropriate, it is easy to just validate a target and get the validation result back without worrying what rules need to be checked.

## Getting Started
Install the [NuGet package](https://www.nuget.org/packages/MvvmValidation).

OR

Download the binaries from [Releases](https://github.com/pglazkov/MvvmValidation/releases).

## Examples
**Creating an instance of the helper**

In order to use the library you need to create an instance of the _ValidationHelper_ helper class and store it in an instance field of the class where you want to implement validation. 
If you do a lot of validation, it is probably the best to do it in your _ViewModelBase_ class.
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
Validator.AddRule(() => FirstName,
                  () => RuleResult.Assert(!string.IsNullOrEmpty(FirstName), "First Name is required"));
```
OR
```cs
Validator.AddRequiredRule(() => FirstName, "First Name is required");
```
**Adding a rule that depends on two different properties**

Such a rule will be executed whenever you validate either of those properties.
```cs
Validator.AddRule(() => RangeStart,
                  () => RangeEnd,
                  () => RuleResult.Assert(RangeEnd > RangeStart, "RangeEnd must be grater than RangeStart");
```
**Adding an asynchronous rule**

Such rule can perform more complex validation that may take long time or cannot be executed synchronously, for example, a call to a web service.
```cs
Validator.AddAsyncRule(() => UserName,
                       (Action<RuleResult> onCompleted) =>
                       {
                           var asyncOperation = UserRegistrationService.IsUserNameAvailable(UserName);
                               asyncOperation.Completed += (o, e) => 
                           {
                               var isAvailable = e.Result;

                               var ruleResult = RuleResult.Assert(isAvailable, string.Format("User Name {0} is taken. Please choose a different one.", UserName));

                              onCompleted(ruleResult);
                           }
                       });
```
**Executing validation**
```cs
// Validate all (execute all validation rules)
ValidationResult validationResult = Validator.ValidateAll();

// Validate a specific target
ValidationResult validationResult = Validator.Validate(() => FirstName);
```
OR
```cs
ValidationResult validationResult = Validator.Validate("FirstName");
```
**Executing validation asynchronously**
```cs
// Validate all (execute all validation rules)
Validator.ValidateAllAsync(result => {
    // Analyze the result
});

// Validate a specific target
Validator.ValidateAsync(() => FirstName, result => {
    // Analyze the result
});
```
**Getting current validation state at any point of time**

Any time you can request current validation state for the entire object or for specific validation targets.
```cs
// Get validation result for the entire object
var validationResult = Validator.GetResult();

// Get validation result for a target
var firstNameValidationResult = Validator.GetResult(() => FirstName);
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
**Implement _IDataErrorInfo_ and _INotifyDataErrorInfo_ interfaces**

The library includes _DataErrorInfoAdapter_ and _NotifyDataErrorAdapter_ classes that make the implementation of _IDataErrorInfo_ and _INotifyDataErrorInfo_ interfaces in your view models trivial.

_IDataErrorInfo_:
```cs
public class ValidatableViewModelBase : IDataErrorInfo
{
        protected ValidationHelper Validator { get; private set; }

        private DataErrorInfoAdapter DataErrorInfoAdapter { get; set; }

        public ValidatableViewModelBase()
        {
            Validator = new ValidationHelper();

            DataErrorInfoAdapter = new DataErrorInfoAdapter(Validator);
        }

        public string this[string columnName]
        {
            get { return DataErrorInfoAdapter[columnName]; }
        }

        public string Error
        {
            get { return DataErrorInfoAdapter.Error; }
        }
}
```
_INotifyDataErrorInfo_:
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
