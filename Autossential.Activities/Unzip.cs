using Autossential.Activities.Properties;
using Autossential.Shared.Activities.Base;
using System;
using System.Activities;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    public sealed class Unzip : ContinuableAsyncTaskCodeActivity
    {
        public InArgument<string> ZipFilePath { get; set; }

        public InArgument<string> ExtractTo { get; set; }

        // [LocalCateg(nameof(Resources.Options_Category))]
        public bool Overwrite { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (ZipFilePath == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ZipFilePath)));
            if (ExtractTo == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ExtractTo)));
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken token)
        {
            var zipFilePath = ZipFilePath.Get(context);
            var extractTo = ExtractTo.Get(context);

            await Task.Run(() =>
            {
                using (var zip = ZipFile.OpenRead(zipFilePath))
                {
                    var dir = Directory.CreateDirectory(extractTo);
                    var dirPath = dir.FullName;

                    foreach (var entry in zip.Entries)
                    {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();

                        var fullPath = Path.GetFullPath(Path.Combine(dirPath, entry.FullName));

                        if (!fullPath.StartsWith(dirPath, StringComparison.OrdinalIgnoreCase))
                            throw new IOException(Resources.Unzip_ErrorMsg_OutsideDir);

                        if (Path.GetFileName(fullPath).Length == 0)
                        {
                            if (entry.Length != 0L)
                                throw new IOException(Resources.Unzip_ErrorMsg_DirNameWithData);

                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                            entry.ExtractToFile(fullPath, Overwrite);
                        }
                    }
                }
            }, token).ConfigureAwait(false);

            return _ => { };
        }
    }
}