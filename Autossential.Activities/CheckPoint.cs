using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;

namespace Autossential.Activities
{
    public class CheckPoint : Activity
    {
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<bool> Expression { get; set; }

        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<Exception> Exception { get; set; }

        public CheckPoint()
        {
            Implementation = (() => new If
            {
                Condition = new ArgumentValue<bool>(nameof(Expression)),
                Else = new Throw
                {
                    DisplayName = DisplayName,
                    Exception = new LambdaValue<Exception>(ctx => Exception.Get(ctx))
                }
            });
        }
    }
}