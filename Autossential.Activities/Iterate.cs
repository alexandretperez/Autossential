using Autossential.Activities.Base;
using System.Activities;

namespace Autossential.Activities
{
    public class Iterate : ScopeActivity
    {
        public InArgument<int> Iterations { get; set; }

        public OutArgument<int> Index { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var exitBookmark = context.CreateBookmark(OnExit, BookmarkOptions.NonBlocking);
            context.Properties.Add(Exit.BOOKMARK_NAME, exitBookmark);

            var nextBookmark = context.CreateBookmark(OnNext, BookmarkOptions.MultipleResume | BookmarkOptions.NonBlocking);
            context.Properties.Add(Next.BOOKMARK_NAME, nextBookmark);

            _iterations = Iterations.Get(context);

            ExecuteNext(context);
        }

        private void ExecuteNext(NativeActivityContext context)
        {
            Index.Set(context, _index);
            context.ScheduleAction(Body, OnIterateCompleted);
        }

        private void OnIterateCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
                _break = true;
            }

            if (!_break && ++_index < _iterations)
                ExecuteNext(context);
        }

        private void OnNext(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.CancelChildren();
            if (value is Bookmark b)
                context.ResumeBookmark(b, value);
        }

        private void OnExit(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.CancelChildren();
            _break = true;
            if (value is Bookmark b)
                context.ResumeBookmark(b, value);
        }

        private int _iterations = 0;
        private int _index = 0;
        private bool _break;
    }
}