﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace UnityFx.Async
{
	public class CompletionCallbackTests
	{
		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			op.SetCanceled();

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompletedSynchronously()
		{
			// Arrange
			var op = AsyncResult.CompletedOperation;

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task TryAddCompletionCallback_ExecutesWhenOperationCompletes()
		{
			// Arrange
			var op = AsyncResult.Delay(1);
			var callbackCalled = false;

			op.TryAddCompletionCallback(_ => callbackCalled = true, null);

			// Act
			await op;

			// Assert
			Assert.True(callbackCalled);
		}

		[Fact]
		public async Task TryAddCompletionCallback_IsThreadSafe()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var counter = 0;

			void CompletionCallback(IAsyncOperation o)
			{
				++counter;
			}

			void TestMethod()
			{
				for (var i = 0; i < 1000; ++i)
				{
					op.TryAddCompletionCallback(CompletionCallback);
				}
			}

			// Act
			await Task.WhenAll(Task.Run(new Action(TestMethod)), Task.Run(new Action(TestMethod)), Task.Run(new Action(TestMethod)));

			// Assert
			op.SetCompleted();
			Assert.Equal(3000, counter);
		}

		[Fact]
		public void TryAddContinuation_ExecutesWhenOperationCompletes()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var continuation = Substitute.For<IAsyncContinuation>();
			op.TryAddContinuation(continuation);

			// Act
			op.SetCompleted();

			// Assert
			continuation.Received(1).Invoke(op, false);
		}
	}
}
