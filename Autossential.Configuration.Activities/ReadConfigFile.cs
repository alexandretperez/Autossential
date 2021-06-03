using System.Activities;
using System.ComponentModel;

namespace Autossential.Configuration.Activities
{
    public sealed class ReadConfigFile : CodeActivity<ConfigSection>
    {
        public InArgument<string> FilePath { get; set; }

        protected override ConfigSection Execute(CodeActivityContext context)
        {
            var filePath = FilePath.Get(context);
            return new ConfigSection(filePath);
        }
    }
}