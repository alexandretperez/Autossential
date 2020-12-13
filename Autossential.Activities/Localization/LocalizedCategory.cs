using Autossential.Activities.Properties;
using System;
using System.ComponentModel;

namespace Autossential.Activities.Localization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LocalizedCategory : CategoryAttribute
    {
        public LocalizedCategory(string name) : base(name)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return Resources.ResourceManager.GetString(value) ?? base.GetLocalizedString(value);
        }
    }
}