using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MvvmValidation.Internal
{
	[DebuggerStepThrough]
	internal static class Guard
	{
		/// <summary>
		///     Ensures that the given expression results in a non-null value.
		/// </summary>
		/// <typeparam name="TResult">
		///     Type of the result, typically omitted as it can
		///     be inferred by the compiler from the lambda expression.
		/// </typeparam>
		/// <param name="value">Value to check.</param>
		/// <param name="argumentNameExpression">The expression for getting the argument name.</param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		/// <exception cref="ArgumentNullException">Expression resulted in a null value.</exception>
		/// <example>
		///     The following example shows how to validate that a
		///     constructor argument is not null:
		///     <code>
		/// public Presenter(IRepository repository, IMailSender mailer)
		/// {
		///   Guard.NotNull(repository, () => repository);
		///   Guard.NotNull(mailer, () => mailer);
		///   
		///     this.repository = repository;
		///     this.mailer = mailer;
		/// }
		/// </code>
		/// </example>
		[ContractArgumentValidator]
		[ContractAnnotation("value:null => halt")]
		public static void NotNull<TResult>(TResult value, Expression<Func<TResult>> argumentNameExpression,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			if (ReferenceEquals(value, null))
			{
				string argName = GetArgName(argumentNameExpression);
				string argumentNullMessage = string.Format("Value of argument \"{0}\" cannot be null.", argName);
				string message = FormatMessageWithCollerInfo(argumentNullMessage, callerMemberName, callerFilePath, callerLineNumber);

				BreakInDebuggerIfAttached();

				throw new ArgumentNullException(argName, message);
			}
			Contract.EndContractBlock();
		}

		/// <summary>
		///     Ensures that the given value is a non-null value.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <param name="paramName">The argument name.</param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="value" /> is a null value.
		/// </exception>
		[ContractArgumentValidator]
		[ContractAnnotation("value:null => halt")]
		public static void NotNull<T>(T value, string paramName,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			if (ReferenceEquals(value, null))
			{
				string argumentNullMessage = string.Format("Value of argument \"{0}\" cannot be null.", paramName);
				string message = FormatMessageWithCollerInfo(argumentNullMessage, callerMemberName, callerFilePath, callerLineNumber);

				BreakInDebuggerIfAttached();

				throw new ArgumentNullException(paramName, message);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		///     Ensures that the given expression results in not null
		///     or an empty string.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <param name="argumentNameExpression">The expression for getting the argument name.</param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		/// <exception cref="ArgumentNullException">Expression resulted in a null value.</exception>
		/// <exception cref="ArgumentException">Expression resulted in an empty string value.</exception>
		/// <example>
		///     The following example shows how to validate that a
		///     constructor argument is not a null or empty string:
		///     <code>
		/// public Presenter(string senderAddress)
		/// {
		///   Guard.NotNullOrEmpty(senderAddress, () => senderAddress);
		///   
		///     this.sender = senderAddress;
		/// }
		/// </code>
		/// </example>
		[ContractArgumentValidator]
		[ContractAnnotation("value:null => halt")]
		public static void NotNullOrEmpty(string value, Expression<Func<string>> argumentNameExpression,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			// ReSharper disable ExplicitCallerInfoArgument
			NotNull(value, argumentNameExpression, callerMemberName, callerFilePath, callerLineNumber);
			// ReSharper restore ExplicitCallerInfoArgument

			if (value.Length == 0)
			{
				string argName = GetArgName(argumentNameExpression);

				string argumentNullMessage = string.Format("Value of argument \"{0}\" cannot be null or an empty string.", argName);
				string message = FormatMessageWithCollerInfo(argumentNullMessage, callerMemberName, callerFilePath, callerLineNumber);

				BreakInDebuggerIfAttached();

				throw new ArgumentException(message, argName);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		///     Ensures that the given value is not null
		///     or an empty string.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="argumentName">The name of the argument that is being checked.</param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		/// <exception cref="ArgumentNullException">Value is a null value.</exception>
		/// <exception cref="ArgumentException">Value is an empty string value.</exception>
		[ContractArgumentValidator]
		[ContractAnnotation("value:null => halt")]
		public static void NotNullOrEmpty(string value, string argumentName,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			// ReSharper disable ExplicitCallerInfoArgument
			NotNull(value, argumentName, callerMemberName, callerFilePath, callerLineNumber);
			// ReSharper restore ExplicitCallerInfoArgument

			if (value.Length == 0)
			{
				string argumentNullMessage = string.Format("Value of argument \"{0}\" cannot be null or an empty string.", argumentName);
				string message = FormatMessageWithCollerInfo(argumentNullMessage, callerMemberName, callerFilePath, callerLineNumber);

				BreakInDebuggerIfAttached();

				throw new ArgumentException(message, argumentName);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		///     Ensures that the given condition is <c>true</c>.
		/// </summary>
		/// <param name="condition">Condition to check.</param>
		/// <param name="argumentNameExpression">The expression for getting the argument name.</param>
		/// <param name="message">
		///     Message to include in the exception of the condition is <c>false</c>.
		/// </param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		/// <exception cref="ArgumentNullException">Expression resulted in a null value.</exception>
		[ContractArgumentValidator]
		public static void Requires<TResult>(bool condition, Expression<Func<TResult>> argumentNameExpression, string message,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			if (!condition)
			{
				string argName = GetArgName(argumentNameExpression);

				string messageWithCallerInfo = FormatMessageWithCollerInfo(message, callerMemberName, callerFilePath,
																		   callerLineNumber);

				BreakInDebuggerIfAttached();

				throw new ArgumentException(messageWithCallerInfo, argName);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		///     Checks the given condition and if it is <c>False</c>, throws <see cref="ArgumentOutOfRangeException" />.
		/// </summary>
		/// <param name="condition">Condition to check.</param>
		/// <param name="argumentNameExpression">The expression for getting the argument name.</param>
		/// <param name="message">
		///     Message to include in the exception of the condition is <c>false</c>.
		/// </param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="condition" /> is <c>false</c>.
		/// </exception>
		[ContractArgumentValidator]
		public static void NotOutOfRange<TResult>(bool condition, Expression<Func<TResult>> argumentNameExpression,
			string message = null,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			if (!condition)
			{
				string argName = GetArgName(argumentNameExpression);

				if (string.IsNullOrEmpty(message))
				{
					message = string.Format("Value of the \"{0}\" argument is out of range.", argName);
				}

				string messageWithCallerInfo = FormatMessageWithCollerInfo(message, callerMemberName, callerFilePath,
																		   callerLineNumber);

				BreakInDebuggerIfAttached();

				throw new ArgumentOutOfRangeException(argName, messageWithCallerInfo);
			}

			Contract.EndContractBlock();
		}

		private static string GetArgName<TResult>(Expression<Func<TResult>> argumentNameExpression)
		{
			//var printer = new PrettyPrinter();
			//var argName = printer.Print(argumentNameExpression.Body);
			//return argName;

			string argName = ((MemberExpression)argumentNameExpression.Body).Member.Name;
			return argName;
		}

		//#endregion

		/// <summary>
		///     Asserts the specified condition.
		/// </summary>
		/// <param name="condition">The condition to assert.</param>
		/// <param name="message">The message to show when the assertion fails.</param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		[ContractAnnotation("condition:false => halt")]
		[Conditional("DEBUG")]
		public static void DebugAssert(bool condition, [NotNull] string message,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			Assert(condition, message, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		///     Asserts the specified condition.
		/// </summary>
		/// <param name="condition">The condition to assert.</param>
		/// <param name="message">The message to show when the assertion fails.</param>
		/// <param name="callerMemberName">To be populated by the compiler.</param>
		/// <param name="callerFilePath">To be populated by the compiler.</param>
		/// <param name="callerLineNumber">To be populated by the compiler.</param>
		[ContractAnnotation("condition:false => halt")]
		public static void Assert(bool condition, [NotNull] string message,
			[CallerMemberName] string callerMemberName = null,
			[CallerFilePath] string callerFilePath = null,
			[CallerLineNumber] int callerLineNumber = 0)
		{
			NotNullOrEmpty(message, "message");

			if (condition)
			{
				return;
			}

			BreakInDebuggerIfAttached();

			string formattedMessage = FormatMessageWithCollerInfo(message, callerMemberName, callerFilePath, callerLineNumber);

			throw new InvalidOperationException(formattedMessage);
		}

		private static string FormatMessageWithCollerInfo(string message, string callerMemberName, string callerFilePath,
			int callerLineNumber)
		{
			if (string.IsNullOrEmpty(callerMemberName) && string.IsNullOrEmpty(callerFilePath))
			{
				return message;
			}

			string result = message;

			result = result + " [Member Name: ";

			if (!string.IsNullOrEmpty(callerMemberName))
			{
				result = result + callerMemberName;
			}

			if (!string.IsNullOrEmpty(callerFilePath))
			{
				result = result + " at " + callerFilePath + " Line: " + callerLineNumber;
			}

			result = result + "]";

			return result;
		}

		[Conditional("DEBUG")]
		private static void BreakInDebuggerIfAttached()
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
		}
	}
}