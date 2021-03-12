using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using MvvmValidation.Internal;

namespace MvvmValidation
{
    /// <summary>
    /// Adapts an instance of <see cref="ValidationHelper"/> to the <see cref="INotifyDataErrorInfo"/> interface.
    /// </summary>
    public class NotifyDataErrorInfoAdapter : INotifyDataErrorInfo, IDisposable
    {
        private bool isDisposed;
        private readonly SynchronizationContext errorsChangedNotificationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyDataErrorInfoAdapter"/> class.
        /// </summary>
        /// <param name="validator">The adaptee.</param>
        public NotifyDataErrorInfoAdapter([NotNull] ValidationHelper validator)
            : this(validator, SynchronizationContext.Current)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyDataErrorInfoAdapter"/> class.
        /// </summary>
        /// <param name="validator">The adaptee.</param>
        /// <param name="errorsChangedNotificationContext">Synchronization context that should be used to raise the <see cref="ErrorsChanged"/> event on.</param>
        public NotifyDataErrorInfoAdapter([NotNull] ValidationHelper validator,
            [CanBeNull] SynchronizationContext errorsChangedNotificationContext)
        {
            Guard.NotNull(validator, nameof(validator));

            Validator = validator;
            this.errorsChangedNotificationContext = errorsChangedNotificationContext;
            Validator.ResultChanged += OnValidatorResultChanged;
        }

        private void OnValidatorResultChanged(object sender, ValidationResultChangedEventArgs e)
        {
            if (errorsChangedNotificationContext != null)
            {
                errorsChangedNotificationContext.Post(_ => {
                    OnErrorsChanged(e.Target as string);
                }, null);

                return;
            }

            OnErrorsChanged(e.Target as string);
        }

        private ValidationHelper Validator { get; }

        #region INotifyDataErrorInfo Members

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve validation errors for, or null or <see cref="F:System.String.Empty"/> to retrieve errors for the entire object.</param>
        /// <returns>
        /// The validation errors for the property or object.
        /// </returns>
        public IEnumerable GetErrors(string propertyName)
        {
            var validationResult = Validator.GetResult(propertyName);

            // Return all the errors as a single string because most UI implementations display only first error
            return validationResult.IsValid ? Enumerable.Empty<string>() : new[] {validationResult.ToString()};
        }

        /// <summary>
        /// Gets a value that indicates whether the object has validation errors.
        /// </summary>
        /// <returns>true if the object currently has validation errors; otherwise, false.</returns>
        public bool HasErrors => !Validator.GetResult().IsValid;

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire object.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #endregion

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void Dispose(bool disposing)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
          if (!isDisposed)
          {
            if (disposing)
            {
                Validator.ResultChanged -= OnValidatorResultChanged;
            }

            isDisposed = true;
          }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void Dispose()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
          // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
          Dispose(disposing: true);
          GC.SuppressFinalize(this);
        }
  }
}