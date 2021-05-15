using Autossential.Activities.Properties;
using Autossential.Utils;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace Autossential.Activities
{
    [DisplayName("Remove Data Columns")]
    public class RemoveDataColumns : CodeActivity
    {
        public InOutArgument<DataTable> DataTable { get; set; }

        public InArgument Columns { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (DataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(metadata)));

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
            var dt = DataTable.Get(context);
            var columnIndexes = DataTableUtil.IdentifyDataColumns(dt, Columns.Get(context)).OrderByDescending(v => v).ToArray();
            foreach (var colIndex in columnIndexes)
                dt.Columns.RemoveAt(colIndex);

            DataTable.Set(context, dt);
        }
    }
}