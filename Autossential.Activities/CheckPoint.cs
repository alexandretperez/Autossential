using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.ExceptionServices;

namespace Autossential.Activities
{
    [DisplayName("Check Point")]
    public sealed class CheckPoint : CodeActivity
    {
        [LocalCateg("")]
        public InArgument<bool> Expression { get; set; }

        [LocalCateg("")]
        public InArgument<Exception> Exception { get; set; }

        [LocalCateg("")]
        [Browsable(true)]
        public Dictionary<string, InArgument> Data { get; } = new Dictionary<string, InArgument>();

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (Expression == null)
                metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Expression)));

            if (Exception == null)
                metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Exception)));
        }

        protected override void Execute(CodeActivityContext context)
        {
            if (Expression.Get(context))
                return;

            var ex = Exception.Get(context);

            foreach (var item in Data)
                ex.Data.Add(item.Key, item.Value.Get(context));

            ExceptionDispatchInfo.Capture(ex).Throw();
        }
    }
}