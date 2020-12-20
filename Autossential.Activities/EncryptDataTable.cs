using Autossential.Activities.Base;
using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using Autossential.Security;
using Autossential.Utils;
using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    [DisplayName("Encrypt DataTable")]
    public class EncryptDataTable : CryptographyBaseActivity
    {
        public InArgument<DataTable> InputDataTable { get; set; }
        public OutArgument<DataTable> OutputDataTable { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument Columns { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument<string> Sort { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public bool ParallelProcessing { get; set; } = true;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (InputDataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(InputDataTable)));
            if (OutputDataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(OutputDataTable)));
            if (Columns != null)
            {
                var argType = Columns.ArgumentType;
                if (typeof(IEnumerable<int>).IsAssignableFrom(argType) || typeof(IEnumerable<string>).IsAssignableFrom(argType))
                {
                    var arg = new RuntimeArgument(nameof(Columns), argType, ArgumentDirection.In, false);
                    metadata.Bind(Columns, arg);
                    metadata.AddArgument(arg);
                }
                else
                {
                    metadata.AddValidationError(Resources.Validation_TypeErrorFormat("IEnumerable<string> or IEnumerable<int>", nameof(Columns)));
                }
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            var inputDt = InputDataTable.Get(context);
            var sortBy = Sort.Get(context);
            var columns = DataTableUtil.IdentifyDataColumns(inputDt, Columns?.Get(context), Enumerable.Range(0, inputDt.Columns.Count));

            var outputDt = CreateCryptoDataTable(inputDt, new HashSet<int>(columns));

            ExecuteCrypto(context, (crypto, key) =>
            {
                outputDt.BeginLoadData();
                AddToDataTable(inputDt, outputDt, key, crypto, columns);
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

        private void AddToDataTable(DataTable inDt, DataTable outDt, string key, Crypto crypto, IEnumerable<int> dataColumnsIndex)
        {
            if (ParallelProcessing)
            {
                var safeList = new ConcurrentBag<object[]>();
                Parallel.ForEach(inDt.AsEnumerable(), row =>
                {
                    var values = ApplyEncryption(row.ItemArray, dataColumnsIndex, crypto, key);
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
                var values = ApplyEncryption(row.ItemArray, dataColumnsIndex, crypto, key);
                outDt.LoadDataRow(values, false);
            }
        }

        private object[] ApplyEncryption(object[] values, IEnumerable<int> dataColumnsIndex, Crypto crypto, string key)
        {
            foreach (var colIndex in dataColumnsIndex)
            {
                var content = values[colIndex];
                if (content == null || content == DBNull.Value || Equals(content, ""))
                    continue;

                values[colIndex] = crypto.Encrypt(content.ToString(), key);
            }

            return values;
        }
    }
}