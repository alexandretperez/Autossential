using Autossential.Activities.Properties;
using System;
using System.ComponentModel;

namespace Autossential.Activities.Localization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class LocalDescriptionAttribute : DescriptionAttribute
    {
        public LocalDescriptionAttribute(string name) : base(name)
        {
        }

        public override string Description => Resources.ResourceManager.GetString(DescriptionValue) ?? base.Description;
    }
}