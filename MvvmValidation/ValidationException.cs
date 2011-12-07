using System;
using System.Runtime.Serialization;

namespace MvvmValidation
{
	/// <summary>
	/// Represents an exception that occurs during validation
	/// </summary>
#if !SILVERLIGHT
	[Serializable]
#endif
	public class ValidationException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		public ValidationException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ValidationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public ValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if !SILVERLIGHT
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
		///   
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
		protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}