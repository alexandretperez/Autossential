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
    public class Aggregate : CodeActivity
    {
        public InArgument<DataTable> DataTable { get; set; }
        public OutArgument<DataRow> Result { get; set; }

        [LocalCateg(nameof(Resources.Output_Category))]
        public bool Detached { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public AggregateFunction Function { get; set; } = AggregateFunction.Sum;

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument Columns { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public bool Incorporate { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (DataTable == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(DataTable)));
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
            DataRow row = null;
            var dt = DataTable.Get(context);
            var columns = Columns?.Get(context);

            var columnIndexes = DataTableUtil.IdentifyDataColumns(dt, columns);

            if (dt.Rows.Count > 0)
            {
                row = Detached
                        ? dt.Clone().NewRow()
                        : dt.NewRow();

                Compute(
                    dt.AsEnumerable(),
                    GetConvertibleColumns(dt, columnIndexes),
                    row
                );
            }

            if (Incorporate)
            {
                dt.BeginLoadData();
                dt.LoadDataRow(row.ItemArray, true);
                dt.EndLoadData();
            }

            Result.Set(context, row);
        }

        private void Compute(IEnumerable<DataRow> rows, Dictionary<int, AggregateFunction[]> convertibles, DataRow result)
        {
            foreach (var c in convertibles)
            {
                if (!c.Value.Contains(Function))
                    continue;

                if (Function == AggregateFunction.DistinctCount)
                {
                    result[c.Key] = rows.Select(row => row[c.Key]).Distinct().Count();
                    continue;
                }

                var validRows = rows.Where(row => HasValue(row[c.Key])).ToArray();
                switch (Function)
                {
                    case AggregateFunction.Average:
                        result[c.Key] = Average(c.Key, validRows);
                        break;

                    case AggregateFunction.StandardDeviation:
                        result[c.Key] = StDev(c.Key, validRows);
                        break;

                    case AggregateFunction.Max:
                        result[c.Key] = validRows.Max(row => row[c.Key]);
                        break;

                    case AggregateFunction.Median:
                        result[c.Key] = Median(c.Key, validRows);
                        break;

                    case AggregateFunction.Min:
                        result[c.Key] = validRows.Min(row => row[c.Key]);
                        break;

                    case AggregateFunction.Sum:
                        result[c.Key] = Sum(c.Key, validRows);
                        break;

                    case AggregateFunction.Variance:
                        result[c.Key] = Variance(c.Key, validRows);
                        break;
                }
            }
        }

        private static bool HasValue(object value) => value != null && value != DBNull.Value && value.ToString()?.Length > 0;

        private Dictionary<int, AggregateFunction[]> GetConvertibleColumns(DataTable table, IEnumerable<int> columnIndexes)
        {
            var hasColumnIndexes = columnIndexes.Any();

            var convertibles = new Dictionary<int, AggregateFunction[]>();

            foreach (DataColumn col in table.Columns)
            {
                if (hasColumnIndexes && !columnIndexes.Contains(col.Ordinal))
                    continue;

                var dataType = col.DataType;
                if (dataType == typeof(object))
                {
                    // Determines the real column type based on the first value found
                    foreach (DataRow row in table.Rows)
                    {
                        var value = row[col.Ordinal];
                        if (!HasValue(value))
                            continue;

                        dataType = row[col.Ordinal].GetType();
                        break;
                    }
                }

                var functions = new[] { AggregateFunction.DistinctCount };

                if (DataTableUtil.IsNumericDataType(dataType))
                {
                    functions = new[] {
                         AggregateFunction.Sum,
                         AggregateFunction.Average,
                         AggregateFunction.Min,
                         AggregateFunction.Max,
                         AggregateFunction.Median,
                         AggregateFunction.DistinctCount,
                         AggregateFunction.StandardDeviation,
                         AggregateFunction.Variance
                     };
                }

                if (new[] {
                    typeof(bool),
                    typeof(char),
                    typeof(string),
                    typeof(DateTime),
                    typeof(Guid),
                    typeof(TimeSpan),
                }.Contains(dataType))
                {
                    functions = new[]
                    {
                        AggregateFunction.Min,
                        AggregateFunction.Max,
                        AggregateFunction.DistinctCount
                    };
                }

                convertibles.Add(col.Ordinal, functions);
            }

            return convertibles;
        }

        #region Math functions

        private static dynamic Sum(int columnIndex, DataRow[] rows)
        {
            dynamic total = 0;
            for (int i = 0; i < rows.Length; i++)
                total += (dynamic)rows[i][columnIndex];

            return total;
        }

        private static dynamic Average(int columnIndex, DataRow[] rows) => Sum(columnIndex, rows) / rows.Length;

        private static dynamic Median(int columnIndex, DataRow[] rows)
        {
            var values = rows.Select(row => row[columnIndex]).ToArray();
            Array.Sort(values);
            int index = values.Length / 2;
            if (values.Length % 2 == 0)
            {
                return ((dynamic)values[index] + (dynamic)values[index - 1]) / 2;
            }
            return values[index];
        }

        private static double StDev(int columnIndex, DataRow[] rows)
        {
            var values = rows.Select(row => (double)row[columnIndex]).ToArray();
            var avg = values.Average();
            var sum = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sum / (values.Length - 1));
        }

        public static double Variance(int columnIndex, DataRow[] rows)
        {
            double avg = Average(columnIndex, rows);
            double variance = 0;
            foreach (DataRow row in rows)
                variance += Math.Pow((dynamic)row[columnIndex] - avg, 2.0);

            return variance / (rows.Length - 1);
        }

        #endregion Math functions
    }
}