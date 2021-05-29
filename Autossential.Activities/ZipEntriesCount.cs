﻿using Autossential.Activities.Base;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    [DisplayName("Zip Entries Count")]
    public sealed class ZipEntriesCount : ContinuableAsyncTaskCodeActivity
    {
        public InArgument<string> ZipFilePath { get; set; }

        public OutArgument<int> EntriesCount { get; set; }
        public OutArgument<int> FilesCount { get; set; }
        public OutArgument<int> FoldersCount { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (ZipFilePath == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ZipFilePath)));
            if (EntriesCount == null && FilesCount == null && FoldersCount == null) 
                metadata.AddValidationError(Resources.ZipEntriesCount_ErrorMsg_OutputMissing);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken token)
        {
            var entriesCount = 0;
            var foldersCount = 0;
            var filesCount = 0;
            var filePath = ZipFilePath.Get(context);

            await Task.Run(() =>
            {
                using (var zip = ZipFile.Open(filePath, ZipArchiveMode.Read))
                {
                    entriesCount = zip.Entries.Count;
                    foldersCount = zip.Entries.Count(entry => string.IsNullOrEmpty(entry.Name));
                    filesCount = entriesCount - foldersCount;
                }
            }).ConfigureAwait(false);

            return ctx =>
            {
                EntriesCount.Set(ctx, entriesCount);
                FilesCount.Set(ctx, filesCount);
                FoldersCount.Set(ctx, foldersCount);
            };
        }
    }
}