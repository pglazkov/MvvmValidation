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
			var tcs = new TaskCompletionSource<TResult[]>();

			var remainingTasks = tasks.ToList();
			int count = remainingTasks.Count;
			var exceptions = new List<Exception>();
			var results = new List<TResult>();

			foreach (var task in remainingTasks)
			{
				task.ContinueWith(t =>
				{
					if (Interlocked.Decrement(ref count) == 0)
					{
						foreach (var task1 in remainingTasks)
						{
							if (task1.Exception != null)
							{
								exceptions.Add(task1.Exception);
							}
							else
							{
								lock (results)
								{
									results.Add(task1.Result);
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
	}
}