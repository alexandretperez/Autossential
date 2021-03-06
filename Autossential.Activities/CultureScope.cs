﻿using Autossential.Activities.Properties;
using Autossential.Shared.Activities.Base;
using System;
using System.Activities;
using System.Globalization;
using System.Threading;

namespace Autossential.Activities
{
    public sealed class CultureScope : ScopeActivity
    {
        public InArgument<string> CultureName { get; set; }

        private readonly CultureInfo _originalCulture;

        public CultureScope()
        {
            _originalCulture = Thread.CurrentThread.CurrentCulture;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (CultureName == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(CultureName)));
        }

        protected override void Execute(NativeActivityContext context)
        {
            var culture = CultureName.Get(context);
            SetCulture(CultureInfo.CreateSpecificCulture(culture));
            context.ScheduleAction(Body, OnCompleted, OnFaulted);
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            SetCulture(_originalCulture);
        }

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            faultContext.CancelChildren();
            SetCulture(_originalCulture);
        }

        private void SetCulture(CultureInfo info)
        {
            Thread.CurrentThread.CurrentCulture = info;
        }
    }
}