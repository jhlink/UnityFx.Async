﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class FinallyResult<T> : PromiseResult<T>, IAsyncContinuation
	{
		#region data

		private readonly Action _continuation;

		#endregion

		#region interface

		public FinallyResult(IAsyncOperation op, Action action)
			: base(op)
		{
			_continuation = action;
		}

		#endregion

		#region PromiseResult

		protected override void InvokeCallbacks(IAsyncOperation op, bool completedSynchronously)
		{
			_continuation();
			TrySetCompleted(completedSynchronously);
		}

		#endregion
	}
}