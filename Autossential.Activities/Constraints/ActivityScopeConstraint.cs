using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Autossential.Activities.Constraints
{
    public abstract class ActivityScopeConstraint : NativeActivity<bool>
    {
        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<IEnumerable<Activity>> ParentChain
        {
            get;
            set;
        }

        protected override void Execute(NativeActivityContext context)
        {
            foreach (var activity in ParentChain.Get(context))
            {
                if (IsInValidScope(activity))
                    return;
            }

            OnScopeValidationError(context);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var arg = new RuntimeArgument("ParentChain", typeof(IEnumerable<Activity>), ArgumentDirection.In, isRequired: true);
            metadata.Bind(ParentChain, arg);
            metadata.AddArgument(arg);
        }

        protected abstract bool IsInValidScope(Activity activity);

        protected abstract void OnScopeValidationError(NativeActivityContext context);
    }
}