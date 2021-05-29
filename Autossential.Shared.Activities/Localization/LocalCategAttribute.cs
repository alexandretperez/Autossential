using Autossential.Activities.Properties;
using System;
using System.ComponentModel;

namespace Autossential.Shared.Activities.Localization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LocalCategAttribute : CategoryAttribute
    {
        public LocalCategAttribute(string name) : base(name)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return Resources.ResourceManager.GetString(value) ?? base.GetLocalizedString(value);
        }
    }
}