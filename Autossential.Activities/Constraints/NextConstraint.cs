using System.Activities;

namespace Autossential.Activities.Constraints
{
    internal class NextConstraint : ActivityScopeConstraint
    {
        protected override bool IsInValidScope(Activity activity)
        {
            return activity != null/* && activity is Iterate*/;
        }

        protected override void OnScopeValidationError(NativeActivityContext context)
        {
            // Constraint.AddValidationError(context, new ValidationError(string.Format(Resources.ValidationScope_Error, nameof(Iterate))));
        }
    }
}