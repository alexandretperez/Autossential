using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using Autossential.Enums;
using Autossential.Utils;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Autossential.Activities
{
    public class RemoveEmptyRows : CodeActivity
    {
        public InArgument<DataTable> InputDataTable { get; set; }

        public OutArgument<DataTable> OutputDataTable { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public DataRowValuesMode Mode { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument Columns { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public ConditionOperator Operator { get; set; }

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

            bool predicate(object value) => value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());

            // default handler
            Func<DataRow, bool> handler = dr => dr.ItemArray.Any(predicate);

            if (Mode == DataRowValuesMode.Any)
            {
                handler = dr => dr.ItemArray.All(predicate);
            }
            else if (Mode == DataRowValuesMode.Custom)
            {
                handler = GetCustomModeHandler(context, inputDt, predicate);
            }

            var rows = inputDt.AsEnumerable().Where(handler);
            var dtResult = rows.Any() ? rows.CopyToDataTable() : inputDt.Clone();

            OutputDataTable.Set(context, dtResult);
        }

        private Func<DataRow, bool> GetCustomModeHandler(CodeActivityContext context, DataTable dt, Func<object, bool> predicate)
        {
            var indexes = DataTableUtil.IdentifyDataColumns(dt, Columns?.Get(context));

            IEnumerable<object> filter(DataRow dr) => dr.ItemArray.Where((_, index) => indexes.Contains(index));

            if (Operator == ConditionOperator.And)
                return (DataRow dr) => filter(dr).Any(predicate);

            return (DataRow dr) => filter(dr).All(predicate);
        }
    }
}