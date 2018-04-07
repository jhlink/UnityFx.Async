﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class CatchResult<T, TException> : ContinuationResult<T>, IAsyncContinuation where TException : Exception
	{
		#region data

		private readonly Action<TException> _errorCallback;

		#endregion

		#region interface

		public CatchResult(IAsyncOperation op, Action<TException> errorCallback)
			: base(op)
		{
			_errorCallback = errorCallback;

			// NOTE: Cannot move this to base class because this call might trigger virtual Invoke
			if (!op.TryAddContinuation(this))
			{
				InvokeInternal(op, true);
			}
		}

		#endregion

		#region PromiseResult

		protected override void InvokeUnsafe(IAsyncOperation op, bool completedSynchronously)
		{
			_errorCallback.Invoke(op.Exception.InnerException as TException);
			TrySetCompleted(completedSynchronously);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			InvokeInternal(op, false);
		}

		#endregion

		#region implementation

		private void InvokeInternal(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully || !(op.Exception.InnerException is TException))
			{
				TrySetCompleted(completedSynchronously);
			}
			else
			{
				InvokeOnSyncContext(op, completedSynchronously);
			}
		}

		#endregion
	}
}
