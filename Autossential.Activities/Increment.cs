﻿using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;

namespace Autossential.Activities
{
    public class Increment : CodeActivity
    {
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<int> Value { get; set; }

        [LocalizedCategory(nameof(Resources.Reference_Category))]
        public InOutArgument<int> Variable { get; set; }

        public Increment()
        {
            Value = new VisualBasicValue<int>("1");
        }

        protected override void Execute(CodeActivityContext context)
        {
            var value = Value.Get(context);
            if (value < 1)
                throw new InvalidOperationException(Resources.Increment_Value_Error);

            Variable.Set(context, Variable.Get(context) + value);
        }
    }

    public class Decrement : CodeActivity
    {
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<int> Value { get; set; }

        [LocalizedCategory(nameof(Resources.Reference_Category))]
        public InOutArgument<int> Variable { get; set; }

        public Decrement()
        {
            Value = new VisualBasicValue<int>("1");
        }

        protected override void Execute(CodeActivityContext context)
        {
            var value = Value.Get(context);
            if (value < 1)
                throw new InvalidOperationException(Resources.Decrement_Value_Error);

            Variable.Set(context, Variable.Get(context) - value);
        }
    }
}