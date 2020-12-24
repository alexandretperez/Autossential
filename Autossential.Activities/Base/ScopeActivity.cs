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
}