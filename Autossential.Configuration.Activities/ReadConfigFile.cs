using Autossential.Configuration.Activities.Properties;
using System.Activities;
using System.ComponentModel;

namespace Autossential.Configuration.Activities
{
    public sealed class ReadConfigFile : CodeActivity<ConfigSection>
    {
        public InArgument<string> FilePath { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (FilePath == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(FilePath)));
            if (Result == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Result)));
        }

        protected override ConfigSection Execute(CodeActivityContext context)
        {
            var filePath = FilePath.Get(context);
            return new ConfigSection(filePath);
        }
    }
}