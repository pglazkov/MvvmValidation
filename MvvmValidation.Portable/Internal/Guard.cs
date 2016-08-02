using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MvvmValidation.Internal
{
    [DebuggerStepThrough]
    internal static class Guard
    {
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
        [ContractAnnotation("value:null => halt")]
        public static void NotNull<T>(T value, string paramName,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (ReferenceEquals(value, null))
            {
                string argumentNullMessage = $"Value of argument \"{paramName}\" cannot be null.";
                string message = FormatMessageWithCollerInfo(argumentNullMessage, callerMemberName, callerFilePath,
                    callerLineNumber);

                BreakInDebuggerIfAttached();

                throw new ArgumentNullException(paramName, message);
            }
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
                string argumentNullMessage = $"Value of argument \"{argumentName}\" cannot be null or an empty string.";
                string message = FormatMessageWithCollerInfo(argumentNullMessage, callerMemberName, callerFilePath,
                    callerLineNumber);

                BreakInDebuggerIfAttached();

                throw new ArgumentException(message, argumentName);
            }
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

            string formattedMessage = FormatMessageWithCollerInfo(message, callerMemberName, callerFilePath,
                callerLineNumber);

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