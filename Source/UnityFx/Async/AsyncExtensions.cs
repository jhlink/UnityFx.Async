﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

#if NET46
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Defines extension methods realted to <see cref="IAsyncOperation"/>.
	/// </summary>
	public static class AsyncExtensions
	{
		#region IAsyncOperation

#if NET46

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		public static AsyncResultAwaiter GetAwaiter(this IAsyncOperation op)
		{
			return new AsyncResultAwaiter(op);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		public static AsyncResultAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> op)
		{
			return new AsyncResultAwaiter<T>(op);
		}

		/// <summary>
		/// Created a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		public static Task ToTask(this IAsyncOperation op)
		{
			if (op is IAsyncContinuationContainer c)
			{
				var result = new TaskCompletionSource<object>(op);

				c.AddContinuation(() =>
				{
					if (op.IsCompletedSuccessfully)
					{
						result.SetResult(null);
					}
					else if (op.IsCanceled)
					{
						result.SetCanceled();
					}
					else
					{
						result.SetException(op.Exception);
					}
				});

				return result.Task;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Created a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		public static Task<T> ToTask<T>(this IAsyncOperation<T> op)
		{
			if (op is IAsyncContinuationContainer c)
			{
				var result = new TaskCompletionSource<T>(op);

				c.AddContinuation(() =>
				{
					if (op.IsCompletedSuccessfully)
					{
						result.SetResult(op.Result);
					}
					else if (op.IsCanceled)
					{
						result.SetCanceled();
					}
					else
					{
						result.SetException(op.Exception);
					}
				});

				return result.Task;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

#endif

		/// <summary>
		/// Transforms the caller instance of <see cref="IAsyncOperation{T}"/> to another one that differs only by the result value.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="transformer"/> is <c>null</c>.</exception>
		public static IAsyncOperation<T> Transform<T, TFrom>(this IAsyncOperation<TFrom> op, Func<TFrom, T> transformer)
		{
			if (transformer == null)
			{
				throw new ArgumentNullException(nameof(transformer));
			}

			return new AsyncResultTransformer<T, TFrom>(op, transformer);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation ContinueWith<T>(this T op, Func<T, IAsyncResult> continuationFactory) where T : class, IAsyncOperation
		{
			return AsyncResult.Factory.ContinueWhen(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation ContinueWith<T>(this T op, Func<T, IAsyncResult> continuationFactory, CancellationToken cancellationToken) where T : class, IAsyncOperation
		{
			return AsyncResult.Factory.ContinueWhen(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation ContinueWith<T>(this T op, Func<T, IAsyncResult> continuationFactory, MonoBehaviour b) where T : class, IAsyncOperation
		{
			return new AsyncFactory(b).ContinueWhen(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation ContinueWith<T>(this T op, Func<T, IAsyncResult> continuationFactory, CancellationToken cancellationToken, MonoBehaviour b) where T : class, IAsyncOperation
		{
			return new AsyncFactory(b).ContinueWhen(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation ContinueWith<T>(this T op, Func<T, IAsyncResult> continuationFactory, AsyncScheduler scheduler) where T : class, IAsyncOperation
		{
			return new AsyncFactory(scheduler).ContinueWhen(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation ContinueWith<T>(this T op, Func<T, IAsyncResult> continuationFactory, CancellationToken cancellationToken, AsyncScheduler scheduler) where T : class, IAsyncOperation
		{
			return new AsyncFactory(scheduler).ContinueWhen(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, IAsyncOperation<TResult>> continuationFactory) where T : class, IAsyncOperation
		{
			return AsyncResult.Factory.ContinueWhen(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, IAsyncOperation<TResult>> continuationFactory, CancellationToken cancellationToken) where T : class, IAsyncOperation
		{
			return AsyncResult.Factory.ContinueWhen(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, IAsyncOperation<TResult>> continuationFactory, MonoBehaviour b) where T : class, IAsyncOperation
		{
			return new AsyncFactory(b).ContinueWhen(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, IAsyncOperation<TResult>> continuationFactory, CancellationToken cancellationToken, MonoBehaviour b) where T : class, IAsyncOperation
		{
			return new AsyncFactory(b).ContinueWhen(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, IAsyncOperation<TResult>> continuationFactory, AsyncScheduler scheduler) where T : class, IAsyncOperation
		{
			return new AsyncFactory(scheduler).ContinueWhen(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, IAsyncOperation<TResult>> continuationFactory, CancellationToken cancellationToken, AsyncScheduler scheduler) where T : class, IAsyncOperation
		{
			return new AsyncFactory(scheduler).ContinueWhen(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<UnityEngine.Object> ContinueWith<T>(this T op, Func<T, AsyncOperation> continuationFactory) where T : class, IAsyncOperation
		{
			return AsyncResult.Factory.ContinueWhen<T, UnityEngine.Object>(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<UnityEngine.Object> ContinueWith<T>(this T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken) where T : class, IAsyncOperation
		{
			return AsyncResult.Factory.ContinueWhen<T, UnityEngine.Object>(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<UnityEngine.Object> ContinueWith<T>(this T op, Func<T, AsyncOperation> continuationFactory, MonoBehaviour b) where T : class, IAsyncOperation
		{
			return new AsyncFactory(b).ContinueWhen<T, UnityEngine.Object>(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<UnityEngine.Object> ContinueWith<T>(this T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken, MonoBehaviour b) where T : class, IAsyncOperation
		{
			return new AsyncFactory(b).ContinueWhen<T, UnityEngine.Object>(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<UnityEngine.Object> ContinueWith<T>(this T op, Func<T, AsyncOperation> continuationFactory, AsyncScheduler scheduler) where T : class, IAsyncOperation
		{
			return new AsyncFactory(scheduler).ContinueWhen<T, UnityEngine.Object>(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<UnityEngine.Object> ContinueWith<T>(this T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken, AsyncScheduler scheduler) where T : class, IAsyncOperation
		{
			return new AsyncFactory(scheduler).ContinueWhen<T, UnityEngine.Object>(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, AsyncOperation> continuationFactory) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			return AsyncResult.Factory.ContinueWhen<T, TResult>(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			return AsyncResult.Factory.ContinueWhen<T, TResult>(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, AsyncOperation> continuationFactory, MonoBehaviour b) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			return new AsyncFactory(b).ContinueWhen<T, TResult>(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken, MonoBehaviour b) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			return new AsyncFactory(b).ContinueWhen<T, TResult>(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, AsyncOperation> continuationFactory, AsyncScheduler scheduler) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			return new AsyncFactory(scheduler).ContinueWhen<T, TResult>(op, continuationFactory);
		}

#if NET46
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="AsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public static IAsyncOperation<TResult> ContinueWith<T, TResult>(this T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken, AsyncScheduler scheduler) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			return new AsyncFactory(scheduler).ContinueWhen<T, TResult>(op, continuationFactory, cancellationToken);
		}
#endif

		/// <summary>
		/// Causes the calling thread to wait until the <see cref="IAsyncResult"/> instance has completed.
		/// </summary>
		/// <seealso cref="Wait(IAsyncResult, int)"/>
		/// <seealso cref="Wait(IAsyncResult, TimeSpan)"/>
		public static void Wait(this IAsyncResult op)
		{
			while (!op.IsCompleted)
			{
				Thread.Sleep(AsyncResult.WaitSleepTimeout);
			}
		}

		/// <summary>
		/// Causes the calling thread to wait until the <see cref="IAsyncResult"/> instance has completed or until the specified <paramref name="timeout"/>.
		/// </summary>
		/// <seealso cref="Wait(IAsyncResult)"/>
		/// <seealso cref="Wait(IAsyncResult, int)"/>
		public static void Wait(this IAsyncResult op, TimeSpan timeout)
		{
			var startTime = DateTime.UtcNow;

			while (!op.IsCompleted)
			{
				var span = DateTime.UtcNow.Subtract(startTime);

				if (span > timeout)
				{
					break;
				}

				Thread.Sleep(AsyncResult.WaitSleepTimeout);
			}
		}

		/// <summary>
		/// Causes the calling thread to wait until the <see cref="IAsyncResult"/> instance has completed or until the specified number of milliseconds.
		/// </summary>
		/// <seealso cref="Wait(IAsyncResult)"/>
		/// <seealso cref="Wait(IAsyncResult, TimeSpan)"/>
		public static void Wait(this IAsyncResult op, int millisecondsTimeout)
		{
			Wait(op, TimeSpan.FromMilliseconds(millisecondsTimeout));
		}

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Returns a <see cref="AsyncFactory"/> instance for this <see cref="MonoBehaviour"/>.
		/// </summary>
		public static AsyncFactory GetAsyncFactory(this MonoBehaviour b)
		{
			return new AsyncFactory(b);
		}

		/// <summary>
		/// Starts an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public static IAsyncOperation StartAsyncOperation(this MonoBehaviour b, IEnumerator op) => GetAsyncFactory(b).FromEnumerator(op);

#if NET46
		/// <summary>
		/// Starts an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public static IAsyncOperation StartAsyncOperation(this MonoBehaviour b, IEnumerator op, CancellationToken cancellationToken) => GetAsyncFactory(b).FromEnumerator(op, cancellationToken);
#endif

		/// <summary>
		/// Starts an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="IAsyncResult"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public static IAsyncOperation StartAsyncOperation(this MonoBehaviour b, IAsyncResult op) => GetAsyncFactory(b).FromAsyncResult(op);

		/// <summary>
		/// Starts an instance of <see cref="IAsyncOperation"/> from the supplied update callback.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		public static IAsyncOperation StartAsyncOperation(this MonoBehaviour b, Action<IAsyncOperationController> updateCallback) => GetAsyncFactory(b).FromUpdateCallback(updateCallback);

#if NET46
		/// <summary>
		/// Starts an instance of <see cref="IAsyncOperation"/> from the supplied update callback.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		public static IAsyncOperation StartAsyncOperation(this MonoBehaviour b, Action<IAsyncOperationController> updateCallback, CancellationToken cancellationToken) => GetAsyncFactory(b).FromUpdateCallback(updateCallback, cancellationToken);
#endif

		/// <summary>
		/// Starts an instance of <see cref="IAsyncOperation{T}"/> from the supplied update callback.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		public static IAsyncOperation<T> StartAsyncOperation<T>(this MonoBehaviour b, Action<IAsyncOperationController<T>> updateCallback) => GetAsyncFactory(b).FromUpdateCallback(updateCallback);

#if NET46
		/// <summary>
		/// Starts an instance of <see cref="IAsyncOperation{T}"/> from the supplied update callback.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		public static IAsyncOperation<T> StartAsyncOperation<T>(this MonoBehaviour b, Action<IAsyncOperationController<T>> updateCallback, CancellationToken cancellationToken) => GetAsyncFactory(b).FromUpdateCallback(updateCallback, cancellationToken);
#endif

		#endregion
	}
}