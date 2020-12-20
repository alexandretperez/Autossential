﻿using Autossential.Activities.Base;
using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using Autossential.Security;
using Autossential.Utils;
using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Autossential.Activities
{
    public class EncryptDataTable : CryptographyBaseActivity
    {
        public InArgument<DataTable> InputDataTable { get; set; }
        public OutArgument<DataTable> OutputDataTable { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument Columns { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument<string> Sort { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public bool ParallelProcessing { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
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

            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            var inputDt = InputDataTable.Get(context);
            var sortBy = Sort.Get(context);
            var columns = DataTableUtil.IdentifyDataColumns(inputDt, Columns.Get(context));
            
            var outputDt = CreateCryptoDataTable(inputDt, new HashSet<int>(columns));

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
                    var values = ApplyEncryption(row.ItemArray, outDt.Columns, crypto, key);
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
                var values = ApplyEncryption(row.ItemArray, outDt.Columns, crypto, key);
                outDt.LoadDataRow(values, false);
            }
        }

        private object[] ApplyEncryption(object[] values, DataColumnCollection dataColumns, Crypto crypto, string key)
        {
            foreach (DataColumn col in dataColumns)
            {
                var content = values[col.Ordinal];
                if (content == null || content == DBNull.Value || Equals(content, ""))
                    continue;

                values[col.Ordinal] = crypto.Encrypt(content.ToString(), key);
            }

            return values;
        }
    }
}