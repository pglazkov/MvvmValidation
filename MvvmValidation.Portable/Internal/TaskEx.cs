using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmValidation.Internal
{
    internal static class TaskEx
    {
        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            var tcs = new TaskCompletionSource<TResult[]>();

            var remainingTasks = tasks.ToList();

            if (remainingTasks.Count == 0)
            {
                throw new ArgumentException("There must be at least one task.", nameof(tasks));
            }

            int count = remainingTasks.Count;
            var exceptions = new List<Exception>();
            var results = new List<TResult>();

            foreach (var task in remainingTasks)
            {
                task.ContinueWith(t =>
                {
                    if (Interlocked.Decrement(ref count) == 0)
                    {
                        foreach (var remainingTask in remainingTasks)
                        {
                            if (remainingTask.Exception != null)
                            {
                                exceptions.Add(remainingTask.Exception);
                            }
                            else
                            {
                                lock (results)
                                {
                                    results.Add(remainingTask.Result);
                                }
                            }
                        }

                        if (exceptions.Any())
                        {
                            tcs.SetException(new AggregateException(exceptions));
                        }
                        else
                        {
                            tcs.SetResult(results.ToArray());
                        }
                    }
                });
            }

            return tcs.Task;
        }

        public static Task<T> FromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();

            tcs.TrySetResult(result);

            return tcs.Task;
        }
    }
}