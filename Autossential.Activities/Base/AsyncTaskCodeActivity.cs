﻿using System;
using System.Activities;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Autossential.Activities.Base
{

    public abstract class AsyncTaskCodeActivity : AsyncCodeActivity<Action<AsyncCodeActivityContext>>, IDisposable
    {
        private CancellationTokenSource _tokenSource;
        private bool _disposed;
        private bool _tokenDisposed;

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            if (_tokenSource != null && !_tokenDisposed)
                _tokenSource.Cancel();

            base.Cancel(context);
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var taskCompletionSource = new TaskCompletionSource<Action<AsyncCodeActivityContext>>(state);
            _tokenSource = new CancellationTokenSource();

            ExecuteAsync(context, _tokenSource.Token)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        taskCompletionSource.TrySetException(task.Exception.InnerException);
                    }
                    else if (task.IsCanceled || _tokenSource.IsCancellationRequested)
                    {
                        taskCompletionSource.TrySetCanceled();
                    }
                    else
                    {
                        taskCompletionSource.TrySetResult(task.Result);
                    }

                    callback?.Invoke(taskCompletionSource.Task);
                });

            return taskCompletionSource.Task;
        }

        protected override Action<AsyncCodeActivityContext> EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var task = (Task<Action<AsyncCodeActivityContext>>)result;
            try
            {
                task.Result?.Invoke(context);
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }

            return Result.Get(context);
        }

        protected abstract Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken token);

        protected async Task ExecuteWithTimeoutAsync(AsyncCodeActivityContext context, CancellationToken token, Task task, int timeout, Action<Action> timeoutHandler = null)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout, token)).ConfigureAwait(false) != task)
            {
                if (token.CanBeCanceled)
                    Cancel(context);

                void handler() => throw new TimeoutException();

                if (timeoutHandler == null)
                    handler();

                timeoutHandler.Invoke(handler);
            }
            await task.ConfigureAwait(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (!_tokenDisposed)
                    _tokenSource?.Dispose();
            }

            _tokenDisposed = true;
            _disposed = true;
        }

        public void Dispose() => Dispose(true);
    }
}