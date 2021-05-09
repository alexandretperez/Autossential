using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using Autossential.Enums;
using Autossential.Security;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Autossential.Activities.Base
{
    public abstract class CryptographyBaseActivity : CodeActivity
    {
        [LocalCateg(nameof(Resources.Options_Category))]
        public SymmetricAlgorithms Algorithm { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        [LocalDisplayName("Encoding")]
        public InArgument<Encoding> TextEncoding { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public InArgument<int> Iterations { get; set; }

        public InArgument<string> Key { get; set; }

        protected CryptographyBaseActivity()
        {
            TextEncoding = ExpressionServiceLanguage.Current.CreateExpression<Encoding>($"{typeof(Encoding).FullName}.{nameof(Encoding.UTF8)}");
            Iterations = new InArgument<int>(1000);
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Key == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Key)));
            if (TextEncoding == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(TextEncoding)));

            base.CacheMetadata(metadata);
        }

        protected virtual DataTable CreateCryptoDataTable(DataTable sourceDataTable, HashSet<int> cryptoColumns)
        {
            var allColumns = cryptoColumns.Count == 0;
            var result = new DataTable();
            foreach (DataColumn col in sourceDataTable.Columns)
            {
                result.Columns.Add(col.ColumnName,
                   allColumns || cryptoColumns.Contains(col.Ordinal) ? typeof(string) : col.DataType
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