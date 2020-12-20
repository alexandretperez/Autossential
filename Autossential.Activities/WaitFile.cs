using Autossential.Activities.Base;
using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.Activities.Validation;
using System.ComponentModel;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    [DisplayName("Wait File")]
    public class WaitFile : ContinuableAsyncTaskCodeActivity
    {
        [LocalCateg("")]
        public InArgument<int> Timeout { get; set; } = 30000;

        [LocalCateg(nameof(Resources.Options_Category))]
        public bool WaitForExists { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public int Interval { get; set; } = 500;

        public InArgument<string> FilePath { get; set; }
        public OutArgument<FileInfo> FileInfo { get; set; }

        private const int MINIMUM_INTERVAL = 100;
        private const int MAXIMUM_INTERVAL = 30000;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (FilePath == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(FilePath)));
            if (Interval < MINIMUM_INTERVAL || Interval > MAXIMUM_INTERVAL)
                metadata.AddValidationError(new ValidationError(Resources.WaitFile_ErrorMsg_IntervalRangeFormat(MINIMUM_INTERVAL, MAXIMUM_INTERVAL)));
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken token)
        {
            var path = FilePath.Get(context);
            var time = Timeout.Get(context);

            await ExecuteWithTimeoutAsync(context, token, ExecuteMainAsync(token, path), time, defaultHandler =>
            {
                if (_fileException == null)
                {
                    defaultHandler();
                    return;
                }

                ExceptionDispatchInfo.Capture(_fileException).Throw();
            }).ConfigureAwait(false);

            return ctx => FileInfo.Set(ctx, new FileInfo(path));
        }

        private Exception _fileException;

        private Task ExecuteMainAsync(CancellationToken token, string path)
        {
            var interval = GetInterval();

            return Task.Run(() =>
            {
                var done = false;
                if (!WaitForExists && !File.Exists(path))
                    throw new IOException(Resources.WaitFile_ErrorMsg_FilePathDoesNotExists);

                do
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();

                        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
                            done = true;
                    }
                    catch (Exception e)
                    {
                        done = e is OperationCanceledException || e is ObjectDisposedException;
                        _fileException = e;

                        if (!done)
                            Thread.Sleep(interval);
                    }
                } while (!done);
            }, token);
        }

        private int GetInterval()
        {
            return Interval < 100 ? 100 : Math.Min(Interval, 30000);
        }
    }
}