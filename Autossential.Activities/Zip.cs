using Autossential.Activities.Properties;
using Autossential.Shared;
using Autossential.Shared.Activities.Base;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    public sealed class Zip : ContinuableAsyncTaskCodeActivity
    {
        public InArgument ToCompress { get; set; }

        public InArgument<string> ZipFilePath { get; set; }

        // [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument<Encoding> TextEncoding { get; set; }

        // [LocalCateg(nameof(Resources.Options_Category))]
        public bool AutoRenaming { get; set; } = true;

        // [LocalCateg(nameof(Resources.Options_Category))]
        public CompressionLevel CompressionLevel { get; set; }

        public OutArgument<int> FilesCount { get; set; }

        public Zip()
        {
            TextEncoding = ExpressionServiceLanguage.Current.CreateExpression<Encoding>($"{typeof(Encoding).FullName}.{nameof(Encoding.UTF8)}");
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (ZipFilePath == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ZipFilePath)));
            if (ToCompress == null)
            {
                metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(ToCompress)));
            }
            else if (ToCompress.ArgumentType == typeof(string) || typeof(IEnumerable<string>).IsAssignableFrom(ToCompress.ArgumentType))
            {
                var arg = new RuntimeArgument(nameof(ToCompress), ToCompress.ArgumentType, ArgumentDirection.In, true);
                metadata.Bind(ToCompress, arg);
                metadata.AddArgument(arg);
            }
            else
            {
                metadata.AddValidationError(Resources.Validation_TypeErrorFormat("IEnumerable<string> or IEnumerable<int>", nameof(ToCompress)));
            }
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken token)
        {
            var toCompress = ToCompress.Get(context);
            var zipFilePath = ZipFilePath.Get(context);
            var encoding = TextEncoding.Get(context);
            var counter = 0;

            await Task.Run(() =>
            {
                if (toCompress is string)
                    toCompress = new string[] { toCompress.ToString() };

                var paths = (IEnumerable<string>)toCompress;
                var directories = paths.Where(Directory.Exists);
                var files = paths.Except(directories)
                    .Concat(directories.SelectMany(path => Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)))
                    .Select(path => new FileInfo(path))
                    .Where(fi => fi.FullName != Path.GetFullPath(zipFilePath));

                var mode = File.Exists(zipFilePath) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

                var allInRoot = files.Select(fi => fi.Directory.FullName).Distinct().Count() == 1;
                using (var zip = ZipFile.Open(zipFilePath, mode, encoding))
                {
                    counter = CreateZip(files, allInRoot, zip, mode, token);
                }
            }, token).ConfigureAwait(false);

            return ctx => FilesCount.Set(ctx, counter);
        }

        private int CreateZip(IEnumerable<FileInfo> files, bool allInRoot, ZipArchive zip, ZipArchiveMode mode, CancellationToken token)
        {
            var dic = new Dictionary<string, int>();
            string Rename(string keyName, string entryName)
            {
                var entry = zip.GetEntry(entryName);
                if (entry == null)
                    return entryName;

                if (dic.ContainsKey(keyName))
                    dic[keyName]++;
                else
                    dic.Add(keyName, 1);

                return Rename(keyName, $"{Path.ChangeExtension(keyName, $"{dic[keyName]}{Path.GetExtension(keyName)}")}");
            }

            var renaming = AutoRenaming && mode == ZipArchiveMode.Update;
            var counter = 0;
            if (allInRoot)
            {
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();

                    zip.CreateEntryFromFile(file.FullName, renaming ? Rename(file.Name, file.Name) : file.Name, CompressionLevel);
                    counter++;
                }
            }
            else
            {
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();

                    var name = NormalizeName(file);
                    zip.CreateEntryFromFile(file.FullName, renaming ? Rename(name, name) : name, CompressionLevel);
                    counter++;
                }
            }

            return counter;
        }

        private static string NormalizeName(FileInfo file)
        {
            return file.FullName.Substring(file.Directory.Root.FullName.Length).Replace('\\', '/');
        }
    }
}