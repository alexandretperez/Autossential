using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;

namespace Autossential.Activities
{
    public class Container : NativeActivity
    {
        [Browsable(false)]
        public ActivityAction Body { get; set; }

        protected override bool CanInduceIdle => true;

        public Container()
        {
            Body = new ActivityAction
            {
                Handler = new Sequence
                {
                    DisplayName = "Do"
                }
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            var exit = context.CreateBookmark(OnExit, BookmarkOptions.NonBlocking);
            context.Properties.Add(Exit.BOOKMARK_NAME, exit);
            context.ScheduleAction(Body, onFaulted: OnFaulted);
        }

        private void OnExit(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.CancelChildren();
        }

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            faultContext.CancelChildren();
        }
    }
}