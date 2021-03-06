﻿using Autossential.Activities.Properties;
using System.Activities;
using System.Activities.Validation;

namespace Autossential.Activities.Constraints
{
    internal class ExitConstraint : ActivityScopeConstraint
    {
        protected override bool IsInValidScope(Activity activity)
        {
            return activity != null && (activity is Container || activity is Iterate);
        }

        protected override void OnScopeValidationError(NativeActivityContext context)
        {
            Constraint.AddValidationError(context, new ValidationError(Resources.Validation_ScopeErrorFormat(nameof(Container))));
        }
    }
}