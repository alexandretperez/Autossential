using Autossential.Activities.Properties;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Autossential.Activities
{
    public sealed class DictionaryToDataTable : CodeActivity
    {
        public InArgument<Dictionary<string, object>> InputDictionary { get; set; }

        public OutArgument<DataTable> OutputDataTable { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (InputDictionary == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(InputDictionary)));
            if (OutputDataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(OutputDataTable)));
        }

        protected override void Execute(CodeActivityContext context)
        {
            var dictionary = InputDictionary.Get(context);
            var table = new DataTable();

            if (dictionary.Count > 0)
            {
                foreach (var item in dictionary)
                    table.Columns.Add(item.Key, item.Value?.GetType() ?? typeof(object));

                table.BeginLoadData();
                table.LoadDataRow(dictionary.Values.ToArray(), true);
                table.EndLoadData();
            }

            OutputDataTable.Set(context, table);
        }
    }
}