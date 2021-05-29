using Autossential.Shared.Activities.Localization;
using Autossential.Activities.Properties;
using Autossential.Extensions;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

namespace Autossential.Activities
{
    [DisplayName("Enumerate Files")]
    public sealed class EnumerateFiles : CodeActivity
    {
        public InArgument Path { get; set; }
        public InArgument SearchPattern { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public SearchOption SearchOption { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public FileAttributes Exclusions { get; set; } = FileAttributes.Hidden
                                                        | FileAttributes.System
                                                        | FileAttributes.Temporary
                                                        | FileAttributes.Device
                                                        | FileAttributes.Offline;

        public OutArgument<IEnumerable<string>> Result { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var directories = Path.GetAsArray<string>(context);
            var patterns = SearchPattern?.GetAsArray<string>(context) ?? new[] { "*.*" };

            IEnumerable<string> result = new string[] { };
            foreach (var directory in directories)
            {
                foreach (var pattern in patterns)
                {
                    result = result.Union(Directory.EnumerateFiles(directory, pattern, SearchOption));
                }
            }

            if (Exclusions > 0)
                result = result.Where(filePath => (new FileInfo(filePath).Attributes & Exclusions) == 0);

            // Outputs
            Result.Set(context, result);
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (Path == null)
            {
                metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Path)));
            }
            else if (Path.IsStringOrCollectionOfString())
            {
                var argument = new RuntimeArgument(nameof(Path), Path.ArgumentType, ArgumentDirection.In, true);
                metadata.Bind(Path, argument);
                metadata.AddArgument(argument);
            }
            else
            {
                metadata.AddValidationError("Invalid format");
            }

            if (SearchPattern != null)
            {
                if (SearchPattern.IsStringOrCollectionOfString())
                {
                    var argument = new RuntimeArgument(nameof(SearchPattern), SearchPattern.ArgumentType, ArgumentDirection.In, true);
                    metadata.Bind(SearchPattern, argument);
                    metadata.AddArgument(argument);
                }
                else
                {
                    metadata.AddValidationError("Invalid format");
                }
            }
        }
    }
}