using Autossential.Activities.Base;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.ComponentModel;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    [DisplayName("Zip Entries Count")]
    public sealed class ZipEntriesCount : ContinuableAsyncTaskCodeActivity
    {
        public InArgument<string> ZipFilePath { get; set; }

        public OutArgument<int> EntriesCount { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (ZipFilePath == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ZipFilePath)));
            if (EntriesCount == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(EntriesCount)));
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken token)
        {
            var entriesCount = 0;
            var filePath = ZipFilePath.Get(context);

            await Task.Run(() =>
            {
                using (var zip = ZipFile.Open(filePath, ZipArchiveMode.Read))
                {
                    entriesCount = zip.Entries.Count;
                }
            }).ConfigureAwait(false);

            return ctx => EntriesCount.Set(ctx, entriesCount);
        }
    }
}