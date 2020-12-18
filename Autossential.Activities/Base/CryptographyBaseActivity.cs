using Autossential.Activities.Properties;
using Autossential.Enums;
using Autossential.Security;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Autossential.Activities.Base
{
    public abstract class CryptographyBaseActivity : CodeActivity
    {
        public SymmetricAlgorithms Algorithm { get; set; }
        public InArgument<Encoding> TextEncoding { get; set; }
        public InArgument<string> Key { get; set; }
        public InArgument<int> Iterations { get; set; }

        protected CryptographyBaseActivity()
        {
            TextEncoding = new VisualBasicValue<Encoding>($"{typeof(Encoding).FullName}.{nameof(Encoding.UTF8)}");
            Iterations = new VisualBasicValue<int>("1000");
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Key == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Key)));
            if (TextEncoding == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(TextEncoding)));

            base.CacheMetadata(metadata);
        }

        protected virtual DataTable CreateCryptoDataTable(DataTable sourceDataTable, HashSet<DataColumn> cryptoColumns)
        {
            var result = new DataTable();
            foreach (DataColumn col in sourceDataTable.Columns)
            {
                result.Columns.Add(col.ColumnName,
                    cryptoColumns.Contains(col) ? typeof(string) : col.DataType
                );
            }
            return result;
        }

        protected void ExecuteCrypto(CodeActivityContext context, Action<Crypto, string> action)
        {
            var key = Key.Get(context);
            var iterations = Iterations.Get(context);
            var encoding = TextEncoding.Get(context);

            using (var crypto = new Crypto(Algorithm, encoding, iterations))
                action(crypto, key);
        }
    }
}