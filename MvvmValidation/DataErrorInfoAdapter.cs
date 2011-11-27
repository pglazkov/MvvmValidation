using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace MvvmValidation
{
	/// <summary>
	/// An adapter of the <see cref="ValidationHelper"/> class to the <see cref="IDataErrorInfo"/> interface.
	/// </summary>
	/// <remarks>
	/// This adapter is intended to be used whenever you need to implement the <see cref="IDataErrorInfo"/> interface using <see cref="ValidationHelper"/>.
	/// </remarks>
	public class DataErrorInfoAdapter : IDataErrorInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataErrorInfoAdapter"/> class.
		/// </summary>
		/// <param name="validator">The adaptee.</param>
		public DataErrorInfoAdapter(ValidationHelper validator)
		{
			Contract.Requires(validator != null);

			Validation = validator;
		}

		private ValidationHelper Validation { get; set; }

		#region IDataErrorInfo Members

		/// <summary>
		/// Gets the error message for the property with the given name.
		/// </summary>
		/// <returns>The error message for the property. The default is an empty string ("").</returns>
		public string this[string columnName]
		{
			get { return Validation.GetResult(columnName).ErrorList.ToString(); }
		}

		/// <summary>
		/// Gets an error message indicating what is wrong with this object.
		/// </summary>
		/// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
		public string Error
		{
			get { return Validation.GetResult().ErrorList.ToString(); }
		}

		#endregion
	}
}