using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MvvmValidation.Internal
{
    internal static class ExceptionUtils
    {
        public static IEnumerable<Exception> UnwrapExceptions(Exception exception)
        {
            var loadException = exception as ReflectionTypeLoadException;
            if (loadException != null)
            {
                return loadException.LoaderExceptions.SelectMany(UnwrapExceptions);
            }

            if (exception is TargetInvocationException)
            {
                return UnwrapExceptions(exception.InnerException);
            }

            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                return aggregateException.InnerExceptions.SelectMany(UnwrapExceptions);
            }

            var validationException = exception as ValidationException;
            if (validationException != null)
            {
                return UnwrapExceptions(validationException.InnerException);
            }

            if (exception.GetType() == typeof(Exception) && exception.InnerException != null)
            {
                return UnwrapExceptions(exception.InnerException);
            }

            return new[] {exception};
        }

        public static Exception UnwrapException(Exception exception)
        {
            var unwrappedExceptions = UnwrapExceptions(exception).ToArray();

            if (unwrappedExceptions.Length > 1)
            {
                return new AggregateException(unwrappedExceptions);
            }

            return unwrappedExceptions[0];
        }
    }
}