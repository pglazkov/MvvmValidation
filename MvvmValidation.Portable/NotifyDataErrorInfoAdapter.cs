using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace MvvmValidation
{
	/// <summary>
	/// Adapts an instance of <see cref="ValidationHelper"/> to the <see cref="INotifyDataErrorInfo"/> interface.
	/// </summary>
	public class NotifyDataErrorInfoAdapter : INotifyDataErrorInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NotifyDataErrorInfoAdapter"/> class.
		/// </summary>
		/// <param name="validator">The adaptee.</param>
		public NotifyDataErrorInfoAdapter(ValidationHelper validator)
			: this(validator, SynchronizationContext.Current)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NotifyDataErrorInfoAdapter"/> class.
		/// </summary>
		/// <param name="validator">The adaptee.</param>
		/// <param name="errorsChangedNotificationContext">Synchronization context that should be used to raise the <see cref="ErrorsChanged"/> event on.</param>
		public NotifyDataErrorInfoAdapter(ValidationHelper validator, SynchronizationContext errorsChangedNotificationContext)
		{
			Contract.Requires(validator != null);

			Validator = validator;

			Validator.ResultChanged += (o,e) => OnValidatorResultChanged(o,e, errorsChangedNotificationContext);

		}

		private void OnValidatorResultChanged(object sender, ValidationResultChangedEventArgs e, SynchronizationContext syncContext)
		{
			if (syncContext != null)
			{
				syncContext.Post(_ => OnValidatorResultChanged(sender, e, null), null);

				return;
			}

			OnErrorsChanged(e.Target as string);
		}

		private ValidationHelper Validator { get; set; }

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
			return validationResult.IsValid ? Enumerable.Empty<string>() : new[] { validationResult.ToString() };
		}

		/// <summary>
		/// Gets a value that indicates whether the object has validation errors.
		/// </summary>
		/// <returns>true if the object currently has validation errors; otherwise, false.</returns>
		public bool HasErrors
		{
			get { return !Validator.GetResult().IsValid; }
		}

		/// <summary>
		/// Occurs when the validation errors have changed for a property or for the entire object.
		/// </summary>
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		#endregion

		private void OnErrorsChanged(string propertyName)
		{
			EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
			if (handler != null)
			{
				handler(this, new DataErrorsChangedEventArgs(propertyName));
			}
		}
	}
}