﻿using Autossential.Activities.Properties;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;

namespace Autossential.Activities
{
    public sealed class Increment : CodeActivity
    {
        public InArgument<int> Value { get; set; }

        public InOutArgument<int> Variable { get; set; }

        public Increment()
        {
            Value = new VisualBasicValue<int>("1");
        }

        protected override void Execute(CodeActivityContext context)
        {
            var value = Value.Get(context);
            if (value < 1)
                throw new InvalidOperationException(Resources.Increment_ErrorMsg_MinValue);

            Variable.Set(context, Variable.Get(context) + value);
        }
    }
}