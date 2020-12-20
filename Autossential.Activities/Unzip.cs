﻿using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.IO;
using System.IO.Compression;

namespace Autossential.Activities
{
    public class Unzip : CodeActivity
    {
        public InArgument<string> ZipFilePath { get; set; }

        public InArgument<string> ExtractTo { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public bool Overwrite { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (ZipFilePath == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ZipFilePath)));
            if (ExtractTo == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ExtractTo)));

            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            var zipFilePath = ZipFilePath.Get(context);
            var extractTo = ExtractTo.Get(context);

            using (var zip = ZipFile.OpenRead(zipFilePath))
            {
                var dir = Directory.CreateDirectory(extractTo);
                var dirPath = dir.FullName;

                foreach (var entry in zip.Entries)
                {
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
        }
    }
}