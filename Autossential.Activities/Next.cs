using Autossential.Activities.Constraints;
using System.Activities;
using System.Activities.Validation;

namespace Autossential.Activities
{
    public sealed class Next : NativeActivity
    {
        protected override bool CanInduceIdle => true;

        public Next()
        {
            var arg = new DelegateInArgument<Next>("constraintArg");
            var ctx = new DelegateInArgument<ValidationContext>("validationContext");
            Constraints.Add(new Constraint<Next>
            {
                Body = new ActivityAction<Next, ValidationContext>
                {
                    Argument1 = arg,
                    Argument2 = ctx,
                    Handler = new NextConstraint
                    {
                        ParentChain = new GetParentChain { ValidationContext = ctx }
                    }
                }
            });
        }

        protected override void Execute(NativeActivityContext context)
        {
            var bookmark = (Bookmark)context.Properties.Find(BOOKMARK_NAME);
            if (bookmark != null)
            {
                var value = context.CreateBookmark();
                context.ResumeBookmark(bookmark, value);
            }
        }

        public const string BOOKMARK_NAME = "NextBookmark";
    }
}