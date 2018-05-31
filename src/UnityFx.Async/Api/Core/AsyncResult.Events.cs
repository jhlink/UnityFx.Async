﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	partial class AsyncResult
	{
		#region data

		private static readonly object _callbackCompletionSentinel = new object();

		private volatile object _callback;

		#endregion

		#region internals

		/// <summary>
		/// Adds a completion callback for <c>await</c> implementation.
		/// </summary>
		internal void SetContinuationForAwait(Action continuation, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (!TryAddCallback(continuation, syncContext, true))
			{
				continuation();
			}
		}

		#endregion

		#region IAsyncOperationEvents

		/// <inheritdoc/>
		public event ProgressChangedEventHandler ProgressChanged
		{
			add
			{
				ThrowIfDisposed();

				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				var syncContext = SynchronizationContext.Current;

				if (!TryAddCallback(value, syncContext, false))
				{
					AsyncCallbackCollection.InvokeProgressCallback(this, value, syncContext);
				}
			}
			remove
			{
				if (value != null)
				{
					TryRemoveCallback(value);
				}
			}
		}

		/// <inheritdoc/>
		public event AsyncCompletedEventHandler Completed
		{
			add
			{
				ThrowIfDisposed();

				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				var syncContext = SynchronizationContext.Current;

				if (!TryAddCallback(value, syncContext, true))
				{
					InvokeCompletionCallback(value, syncContext);
				}
			}
			remove
			{
				if (value != null)
				{
					TryRemoveCallback(value);
				}
			}
		}

		/// <inheritdoc/>
		public void AddCompletionCallback(AsyncOperationCallback action)
		{
			if (!TryAddCompletionCallback(action))
			{
				InvokeCompletionCallback(action, SynchronizationContext.Current);
			}
		}

		/// <inheritdoc/>
		public bool TryAddCompletionCallback(AsyncOperationCallback action)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddCallback(action, SynchronizationContext.Current, true);
		}

		/// <inheritdoc/>
		public void AddCompletionCallback(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			if (!TryAddCompletionCallback(action, syncContext))
			{
				InvokeCompletionCallback(action, syncContext);
			}
		}

		/// <inheritdoc/>
		public bool TryAddCompletionCallback(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddCallback(action, syncContext, true);
		}

		/// <inheritdoc/>
		public bool RemoveCompletionCallback(AsyncOperationCallback action)
		{
			if (action != null)
			{
				return TryRemoveCallback(action);
			}

			return false;
		}

		/// <inheritdoc/>
		public void AddCompletionCallback(IAsyncContinuation continuation)
		{
			if (!TryAddCompletionCallback(continuation))
			{
				InvokeCompletionCallback(continuation, SynchronizationContext.Current);
			}
		}

		/// <inheritdoc/>
		public bool TryAddCompletionCallback(IAsyncContinuation continuation)
		{
			ThrowIfDisposed();

			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			return TryAddCallback(continuation, SynchronizationContext.Current, true);
		}

		/// <inheritdoc/>
		public void AddCompletionCallback(IAsyncContinuation continuation, SynchronizationContext syncContext)
		{
			if (!TryAddCompletionCallback(continuation, syncContext))
			{
				InvokeCompletionCallback(continuation, syncContext);
			}
		}

		/// <inheritdoc/>
		public bool TryAddCompletionCallback(IAsyncContinuation continuation, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			return TryAddCallback(continuation, syncContext, true);
		}

		/// <inheritdoc/>
		public bool RemoveCompletionCallback(IAsyncContinuation continuation)
		{
			if (continuation != null)
			{
				return TryRemoveCallback(continuation);
			}

			return false;
		}

		/// <inheritdoc/>
		public void AddProgressCallback(AsyncOperationCallback action)
		{
			if (!TryAddProgressCallback(action))
			{
				AsyncCallbackCollection.InvokeProgressCallback(this, action, SynchronizationContext.Current);
			}
		}

		/// <inheritdoc/>
		public bool TryAddProgressCallback(AsyncOperationCallback action)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddCallback(action, SynchronizationContext.Current, false);
		}

		/// <inheritdoc/>
		public void AddProgressCallback(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			if (!TryAddProgressCallback(action, syncContext))
			{
				AsyncCallbackCollection.InvokeProgressCallback(this, action, syncContext);
			}
		}

		/// <inheritdoc/>
		public bool TryAddProgressCallback(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddCallback(action, syncContext, false);
		}

		/// <inheritdoc/>
		public bool RemoveProgressCallback(AsyncOperationCallback action)
		{
			if (action != null)
			{
				return TryRemoveCallback(action);
			}

			return false;
		}

#if !NET35

		/// <inheritdoc/>
		public void AddProgressCallback(IProgress<float> callback)
		{
			if (!TryAddProgressCallback(callback))
			{
				AsyncCallbackCollection.InvokeProgressCallback(this, callback, SynchronizationContext.Current);
			}
		}

		/// <inheritdoc/>
		public bool TryAddProgressCallback(IProgress<float> callback)
		{
			ThrowIfDisposed();

			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			return TryAddCallback(callback, SynchronizationContext.Current, false);
		}

		/// <inheritdoc/>
		public void AddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext)
		{
			if (!TryAddProgressCallback(callback, syncContext))
			{
				AsyncCallbackCollection.InvokeProgressCallback(this, callback, syncContext);
			}
		}

		/// <inheritdoc/>
		public bool TryAddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			return TryAddCallback(callback, syncContext, false);
		}

		/// <inheritdoc/>
		public bool RemoveProgressCallback(IProgress<float> callback)
		{
			if (callback != null)
			{
				return TryRemoveCallback(callback);
			}

			return false;
		}

#endif

		#endregion

		#region implementation

		private bool TryAddCallback(object callbackToAdd, SynchronizationContext syncContext, bool completionCallback)
		{
			// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
			var oldValue = _callback;

			// Quick return if the operation is completed.
			if (oldValue != _callbackCompletionSentinel)
			{
				// If no callback is stored yet, try to store it as _callback.
				if (oldValue == null)
				{
					var newValue = callbackToAdd;

					if (completionCallback)
					{
						if (syncContext != null)
						{
							newValue = new AsyncCallbackCollection(this, callbackToAdd, syncContext);
						}
					}
					else
					{
						var newList = new AsyncCallbackCollection(this);
						newList.AddProgressCallback(callbackToAdd, syncContext);
						newValue = newList;
					}

					oldValue = Interlocked.CompareExchange(ref _callback, newValue, null);

					// Quick return if exchange succeeded.
					if (oldValue == null)
					{
						return true;
					}
				}

				// Logic for the case where we were previously storing a single callback.
				if (oldValue != _callbackCompletionSentinel && !(oldValue is AsyncCallbackCollection))
				{
					var newList = new AsyncCallbackCollection(this, oldValue, null);
					Interlocked.CompareExchange(ref _callback, newList, oldValue);

					// We might be racing against another thread converting the single into a list,
					// or we might be racing against operation completion, so resample "list" below.
				}

				// If list is null, it can only mean that _callbackCompletionSentinel has been exchanged
				// into _callback. Thus, the task has completed and we should return false from this method,
				// as we will not be queuing up the callback.
				if (_callback is AsyncCallbackCollection list)
				{
					lock (list)
					{
						// It is possible for the operation to complete right after we snap the copy of the list.
						// If so, then fall through and return false without queuing the callback.
						if (_callback != _callbackCompletionSentinel)
						{
							if (completionCallback)
							{
								list.AddCompletionCallback(callbackToAdd, syncContext);
							}
							else
							{
								list.AddProgressCallback(callbackToAdd, syncContext);
							}

							return true;
						}
					}
				}
			}

			return false;
		}

		private bool TryRemoveCallback(object callbackToRemove)
		{
			// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
			var value = _callback;

			if (value != _callbackCompletionSentinel)
			{
				var list = value as AsyncCallbackCollection;

				if (list == null)
				{
					// This is not a list. If we have a single object (the one we want to remove) we try to replace it with an empty list.
					// Note we cannot go back to a null state, since it will mess up the TryAddCallback() logic.
					if (Interlocked.CompareExchange(ref _callback, new AsyncCallbackCollection(this), callbackToRemove) == callbackToRemove)
					{
						return true;
					}
					else
					{
						// If we fail it means that either TryAddContinuation won the race condition and _callback is now a List
						// that contains the element we want to remove. Or it set the _callbackCompletionSentinel.
						// So we should try to get a list one more time.
						list = _callback as AsyncCallbackCollection;
					}
				}

				// If list is null it means _callbackCompletionSentinel has been set already and there is nothing else to do.
				if (list != null)
				{
					lock (list)
					{
						// There is a small chance that the operation completed since we took a local snapshot into
						// list. In that case, just return; we don't want to be manipulating the callback list as it is being processed.
						if (_callback != _callbackCompletionSentinel)
						{
							return list.Remove(callbackToRemove);
						}
					}
				}
			}

			return false;
		}

		private void InvokeProgressCallbacks()
		{
			var value = _callback;

			if (value != null)
			{
				if (value is AsyncCallbackCollection callbackList)
				{
					lock (callbackList)
					{
						callbackList.InvokeProgressCallbacks();
					}
				}
				else
				{
					AsyncCallbackCollection.InvokeProgressCallback(this, value);
				}
			}
		}

		private void InvokeCallbacks()
		{
			var value = Interlocked.Exchange(ref _callback, _callbackCompletionSentinel);

			if (value != null)
			{
				if (value is AsyncCallbackCollection callbackList)
				{
					lock (callbackList)
					{
						callbackList.Invoke();
					}
				}
				else
				{
					InvokeCallback(value);
				}
			}
		}

		private void InvokeCallback(object value)
		{
			if ((_flags & _flagRunContinuationsAsynchronously) != 0)
			{
				if (value is AsyncCallbackCollection callbackList)
				{
					// NOTE: This is more effective than InvokeContinuationAsync().
					lock (callbackList)
					{
						callbackList.InvokeAsync();
					}
				}
				else
				{
					InvokeCallbackAsync(value, SynchronizationContext.Current, false);
				}
			}
			else
			{
				if (value is AsyncCallbackCollection callbackList)
				{
					lock (callbackList)
					{
						callbackList.Invoke();
					}
				}
				else
				{
					InvokeContinuationInline(value, false);
				}
			}
		}

		private void InvokeCompletionCallback(object continuation, SynchronizationContext syncContext)
		{
			if ((_flags & _flagRunContinuationsAsynchronously) != 0)
			{
				InvokeCallbackAsync(continuation, syncContext, true);
			}
			else if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeContinuationInline(continuation, true);
			}
			else
			{
				syncContext.Post(args => InvokeContinuationInline(args, true), continuation);
			}
		}

		private void InvokeCallbackAsync(object continuation, SynchronizationContext syncContext, bool inline)
		{
			if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
			{
				syncContext.Post(args => InvokeContinuationInline(args, inline), continuation);
			}
			else
			{
				ThreadPool.QueueUserWorkItem(args => InvokeContinuationInline(args, inline), continuation);
			}
		}

		private void InvokeContinuationInline(object continuation, bool inline)
		{
			AsyncCallbackCollection.InvokeCompletionCallback(this, continuation, inline);
		}

		#endregion
	}
}
