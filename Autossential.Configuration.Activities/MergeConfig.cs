using Autossential.Configuration.Activities.Properties;
using System.Activities;

namespace Autossential.Configuration.Activities
{
    public sealed class MergeConfig : CodeActivity
    {
        public InOutArgument<ConfigSection> Destination { get; set; }
        public InArgument<ConfigSection> Source { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (Destination == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Destination)));
            if (Source == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Source)));
        }

        protected override void Execute(CodeActivityContext context)
        {
            Destination.Get(context).Merge(Source.Get(context));
        }
    }
}