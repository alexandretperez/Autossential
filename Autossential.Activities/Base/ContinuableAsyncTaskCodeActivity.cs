using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Autossential.Activities.Base
{
    public abstract class ContinuableAsyncTaskCodeActivity : AsyncTaskCodeActivity
    {
        [LocalizedCategory(nameof(Resources.Common_Category))]
        public InArgument<bool> ContinueOnError { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            try
            {
                return base.BeginExecute(context, callback, state);
            }
            catch (Exception e)
            {
                if (ContinueOnError.Get(context))
                {
                    Trace.TraceError(e.ToString());
                    var tcs = new TaskCompletionSource<AsyncCodeActivityContext>(state);
                    tcs.TrySetResult(null);
                    callback?.Invoke(tcs.Task);
                    return tcs.Task;
                }

                throw;
            }
        }

        protected override Action<AsyncCodeActivityContext> EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            try
            {
                return base.EndExecute(context, result);
            }
            catch (Exception e)
            {
                if (ContinueOnError.Get(context))
                {
                    Trace.TraceError(e.ToString());
                    return Result.Get(context);
                }

                throw;
            }
        }
    }
}