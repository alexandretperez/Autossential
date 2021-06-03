using Autossential.Activities.Properties;
using System.Activities;
using System.Collections.Generic;
using System.Data;

namespace Autossential.Activities
{
    public sealed class DataRowToDictionary : CodeActivity
    {
        public InArgument<DataRow> InputDataRow { get; set; }

        public OutArgument<Dictionary<string, object>> OutputDictionary { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (InputDataRow == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(InputDataRow)));
            if (OutputDictionary == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(OutputDictionary)));
        }

        protected override void Execute(CodeActivityContext context)
        {
            var dataRow = InputDataRow.Get(context);
            var dictionary = new Dictionary<string, object>();

            foreach (DataColumn col in dataRow.Table.Columns)
                dictionary.Add(col.ColumnName, dataRow[col.ColumnName]);

            OutputDictionary.Set(context, dictionary);
        }
    }
}