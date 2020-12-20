using Autossential.Activities.Localization;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.ComponentModel;

namespace Autossential.Activities
{
    [DisplayName("Check Point")]
    public class CheckPoint : Activity
    {
        [LocalCateg("")]
        public InArgument<bool> Expression { get; set; }

        [LocalCateg("")]
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