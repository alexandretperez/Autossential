using Autossential.Activities.Localization;
using Autossential.Activities.Properties;
using Autossential.Enums;
using Autossential.Security;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Security;
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

        public InArgument Key { get; set; }

        protected CryptographyBaseActivity()
        {
            TextEncoding = ExpressionServiceLanguage.Current.CreateExpression<Encoding>($"{typeof(Encoding).FullName}.{nameof(Encoding.UTF8)}");
            Iterations = new InArgument<int>(1000);
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (TextEncoding == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(TextEncoding)));
            if (Key == null || (Key.ArgumentType != typeof(string) && Key.ArgumentType != typeof(SecureString)))
            {
                metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Key)));
            }
            else
            {
                var arg = new RuntimeArgument(nameof(Key), Key.ArgumentType, ArgumentDirection.In, true);
                metadata.Bind(Key, arg);
                metadata.AddArgument(arg);
            }
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

            string cryptoKey;
            if (key is SecureString ssKey)
            {
                cryptoKey = new NetworkCredential(null, ssKey).Password;
            }
            else
            {
                cryptoKey = (string)key;
            }

            using (var crypto = new Crypto(Algorithm, encoding, iterations))
                action(crypto, cryptoKey);
        }
    }
}