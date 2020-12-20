using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;

namespace Autossential.Activities.Base
{
    public abstract class ScopeActivity : NativeActivity
    {
        [Browsable(false)]
        public ActivityAction Body { get; set; }

        protected override bool CanInduceIdle => true;

        protected ScopeActivity()
        {
            InitializeBody();
        }

        protected virtual void InitializeBody()
        {
            Body = new ActivityAction
            {
                Handler = new Sequence
                {
                    DisplayName = "Do"
                }
            };
        }
    }

    //public abstract class AsyncTaskCodeActivity<T, TState> : AsyncCodeActivity<T>
    //{
    //    protected sealed override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
    //    {
    //        TState activityState = PreExecute(context);
    //        context.UserState = activityState;
    //        var task = ExecuteAsync(activityState);
    //        return AsyncFactory<T>.ToBegin(task, callback, state);
    //    }

    //    protected sealed override T EndExecute(AsyncCodeActivityContext context, IAsyncResult asyncResult)
    //    {
    //        var result = AsyncFactory<T>.ToEnd(asyncResult);
    //        return PostExecute(context, (TState)context.UserState, result);
    //    }

    //    protected virtual TState PreExecute(AsyncCodeActivityContext context)
    //    {
    //        return default(TState);
    //    }
    //    protected abstract Task<T> ExecuteAsync(TState activityState);
    //    protected virtual T PostExecute(AsyncCodeActivityContext context, TState activityState, T result)
    //    {
    //        return result;
    //    }
    //}
    //public abstract class AsyncTaskCodeActivity<T> : AsyncTaskCodeActivity<T, object>
    //{
    //}
}