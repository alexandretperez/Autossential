using Autossential.Activities.Base;
using Autossential.Activities.Properties;
using Autossential.Security;
using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    public class DecryptDataTable : CryptographyBaseActivity
    {
        public InArgument<DataTable> InputDataTable { get; set; }
        public InArgument Columns { get; set; }
        public OutArgument<DataTable> OutputDataTable { get; set; }
        public bool ParallelProcessing { get; set; }
        public InArgument<string> Sort { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (InputDataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(InputDataTable)));
            if (OutputDataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(OutputDataTable)));

            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            var inputDt = InputDataTable.Get(context);
            var columns = new DataColumn[inputDt.Columns.Count];
            var sortBy = Sort.Get(context);

            inputDt.Columns.CopyTo(columns, 0);
            var outputDt = CreateCryptoDataTable(inputDt, new HashSet<DataColumn>(columns));

            ExecuteCrypto(context, (crypto, key) =>
            {
                outputDt.BeginLoadData();
                AddToDataTable(inputDt, outputDt, key, crypto);
                outputDt.AcceptChanges();
                outputDt.EndLoadData();
            });

            if (sortBy != null)
            {
                outputDt.DefaultView.Sort = sortBy;
                outputDt = outputDt.DefaultView.ToTable();
            }

            OutputDataTable.Set(context, outputDt);
        }

        private void AddToDataTable(DataTable inDt, DataTable outDt, string key, Crypto crypto)
        {
            if (ParallelProcessing)
            {
                var safeList = new ConcurrentBag<object[]>();
                Parallel.ForEach(inDt.AsEnumerable(), row =>
                {
                    var values = ApplyDecryption(row.ItemArray, outDt.Columns, crypto, key);
                    safeList.Add(values);
                });

                while (!safeList.IsEmpty)
                {
                    if (safeList.TryTake(out object[] values))
                        outDt.LoadDataRow(values, false);
                }

                return;
            }

            foreach (DataRow row in inDt.Rows)
            {
                var values = ApplyDecryption(row.ItemArray, outDt.Columns, crypto, key);
                outDt.LoadDataRow(values, false);
            }
        }

        private object[] ApplyDecryption(object[] values, DataColumnCollection dataColumns, Crypto crypto, string key)
        {
            foreach (DataColumn col in dataColumns)
            {
                var content = values[col.Ordinal];
                if (content == null || content == DBNull.Value || Equals(content, ""))
                    continue;

                values[col.Ordinal] = crypto.Decrypt(content.ToString(), key);
            }

            return values;
        }
    }
}