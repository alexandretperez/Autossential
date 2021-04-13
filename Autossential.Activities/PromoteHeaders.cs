﻿using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace Autossential.Activities
{
    [DisplayName("Promote Headers")]
    public sealed class PromoteHeaders : CodeActivity
    {
        public InArgument<DataTable> InputDataTable { get; set; }
        public OutArgument<DataTable> OutputDataTable { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public bool AutoRename { get; set; } = true;

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument<string> EmptyColumnName { get; set; } = new InArgument<string>("Empty");

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (InputDataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(InputDataTable)));
            if (OutputDataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(OutputDataTable)));
            if (EmptyColumnName == null)
            {
                metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(EmptyColumnName)));
            }
            else
            {
                if (EmptyColumnName.Expression is Literal<string> prop && string.IsNullOrEmpty(prop.Value))
                {
                    metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(EmptyColumnName)));
                }
            }
            
            
            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            var names = new Dictionary<string, int>();
            var emptyName = EmptyColumnName.Get(context);

            var inputDT = InputDataTable.Get(context);
            if (inputDT.Rows.Count == 0)
                throw new InvalidOperationException("There is no rows in input data table to promote");

            var outputDT = inputDT.Copy();

            var row = outputDT.Rows[0];

            string getName(string firstRowValue)
            {
                return string.IsNullOrEmpty(firstRowValue) ? emptyName : firstRowValue;
            }

            if (AutoRename)
            {
                foreach (DataColumn col in outputDT.Columns)
                {
                    var name = getName(row[col.ColumnName].ToString());
                    if (names.ContainsKey(name))
                    {
                        names[name]++;
                        name += names[name].ToString();
                    }
                    else
                    {
                        names.Add(name, 0);
                    }

                    col.ColumnName = name;
                }
            }
            else
            {
                foreach (DataColumn col in outputDT.Columns)
                    col.ColumnName = getName(row[col.ColumnName].ToString());
            }

            outputDT.Rows.Remove(row);
            OutputDataTable.Set(context, outputDT);
        }
    }
}