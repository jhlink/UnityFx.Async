﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A lightweight net35-compatible asynchronous operation that can return a value.
	/// </summary>
	/// <typeparam name="TResult">Type of the operation result value.</typeparam>
	/// <seealso cref="AsyncCompletionSource{T}"/>
	/// <seealso cref="AsyncResult"/>
	/// <seealso cref="IAsyncResult"/>
#if NET35
	public class AsyncResult<TResult> : AsyncResult, IAsyncOperation<TResult>
#else
	public class AsyncResult<TResult> : AsyncResult, IAsyncOperation<TResult>, IObservable<TResult>
#endif
	{
		#region data

		private TResult _result;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		public AsyncResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncCallback asyncCallback, object asyncState)
			: base(asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="status">Status value of the operation.</param>
		public AsyncResult(AsyncOperationStatus status)
			: base(status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="status">Status value of the operation.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncOperationStatus status, object asyncState)
			: base(status, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="status">Status value of the operation.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: base(status, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class. For internal use only.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		internal AsyncResult(Exception exception, object asyncState)
			: base(exception, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class. For internal use only.
		/// </summary>
		/// <param name="exceptions">Exceptions to complete the operation with.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		internal AsyncResult(IEnumerable<Exception> exceptions, object asyncState)
			: base(exceptions, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class. For internal use only.
		/// </summary>
		/// <param name="result">Result value.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		internal AsyncResult(TResult result, object asyncState)
			: base(AsyncOperationStatus.RanToCompletion, asyncState)
		{
			_result = result;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TrySetResult(TResult result, bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (TryReserveCompletion())
			{
				_result = result;
				SetCompleted(StatusRanToCompletion, completedSynchronously);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Copies state of the specified operation.
		/// </summary>
		internal void CopyCompletionState(IAsyncOperation<TResult> patternOp, bool completedSynchronously)
		{
			if (!TryCopyCompletionState(patternOp, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attemts to copy state of the specified operation.
		/// </summary>
		internal bool TryCopyCompletionState(IAsyncOperation<TResult> patternOp, bool completedSynchronously)
		{
			if (patternOp.IsCompletedSuccessfully)
			{
				return TrySetResult(patternOp.Result, completedSynchronously);
			}
			else if (patternOp.IsFaulted || patternOp.IsCanceled)
			{
				return TrySetException(patternOp.Exception, completedSynchronously);
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public TResult Result
		{
			get
			{
				if (!IsCompleted)
				{
					throw new InvalidOperationException(Constants.ErrorResultNotAvailable);
				}

				ThrowIfNonSuccess(true);
				return _result;
			}
		}

		#endregion

		#region IObservable

#if !NET35

		/// <inheritdoc/>
		public IDisposable Subscribe(IObserver<TResult> observer)
		{
			ThrowIfDisposed();

			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			AsyncOperationCallback completionCallback = op =>
			{
				if (IsCompletedSuccessfully)
				{
					observer.OnNext(_result);
					observer.OnCompleted();
				}
				else if (IsFaulted)
				{
					observer.OnError(Exception.InnerException);
				}
				else
				{
					observer.OnCompleted();
				}
			};

			if (TryAddCompletionCallback(completionCallback, null))
			{
				return new AsyncObservableSubscription(this, completionCallback);
			}
			else
			{
				completionCallback(this);
				return Disposable.Empty;
			}
		}

#endif

		#endregion
	}
}
