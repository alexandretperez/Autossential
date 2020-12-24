using Autossential.Activities.Constraints;
using System.Activities;
using System.Activities.Validation;

namespace Autossential.Activities
{
    public sealed class Exit : NativeActivity
    {
        protected override bool CanInduceIdle => true;

        public Exit()
        {
            var arg = new DelegateInArgument<Exit>("constraintArg");
            var ctx = new DelegateInArgument<ValidationContext>("validationContext");
            Constraints.Add(new Constraint<Exit>
            {
                Body = new ActivityAction<Exit, ValidationContext>
                {
                    Argument1 = arg,
                    Argument2 = ctx,
                    Handler = new ExitConstraint
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

        public const string BOOKMARK_NAME = "ExitBookmark";
    }
}