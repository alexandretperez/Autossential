using Autossential.Activities.Properties;
using System;
using System.ComponentModel;

namespace Autossential.Activities.Localization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class LocalDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalDisplayNameAttribute(string name) : base(name)
        {
        }

        public override string DisplayName => Resources.ResourceManager.GetString(DisplayNameValue) ?? base.DisplayName;
    }
}