using Autossential.Activities.Properties;
using Autossential.Shared.Utils;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Autossential.Activities
{
    public sealed class RemoveDuplicateRows : CodeActivity
    {
        public InArgument<DataTable> InputDataTable { get; set; }

        public OutArgument<DataTable> OutputDataTable { get; set; }

        public InArgument Columns { get; set; }

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
            var inputDt = InputDataTable.Get(context) ?? throw new ArgumentException(nameof(InputDataTable));
            var columns = DataTableUtil.IdentifyDataColumns(inputDt, Columns?.Get(context));

            if (OutputDataTable == null)
                return;

            DataTable outputDt;
            if (columns.Any())
            {
                outputDt = inputDt.Clone();
                var colIndexes = columns.ToArray();
                foreach (DataRow inRow in inputDt.Rows)
                {
                    var skip = false;
                    foreach (DataRow outRow in outputDt.Rows)
                    {
                        if (RowExist(inRow.ItemArray, outRow.ItemArray, colIndexes))
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (skip) continue;

                    outputDt.ImportRow(inRow);
                }
            }
            else
            {
                outputDt = inputDt.AsDataView().ToTable(true);
            }

            OutputDataTable.Set(context, outputDt);
        }

        private bool RowExist(object[] inputValues, object[] outputValues, int[] columns)
        {
            bool flag = true;
            foreach (var colIndex in columns)
                flag &= Equals(inputValues[colIndex], outputValues[colIndex]);

            return flag;
        }
    }
}