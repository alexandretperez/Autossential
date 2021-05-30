using Autossential.Activities.Properties;
using Autossential.Enums;
using Autossential.Security;
using Autossential.Shared;
using Autossential.Shared.Activities.Localization;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        [Browsable(false)]
        public InArgument<string> Key { get; set; }

        [Browsable(false)]
        public InArgument<SecureString> SecureKey { get; set; }

        [Browsable(false)]
        public bool UseSecureKey { get; set; }

        protected CryptographyBaseActivity()
        {
            TextEncoding = ExpressionServiceLanguage.Current.CreateExpression<Encoding>($"{typeof(Encoding).FullName}.{nameof(Encoding.UTF8)}");
            Iterations = new InArgument<int>(1000);
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (TextEncoding == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(TextEncoding)));

            if (UseSecureKey && SecureKey == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(SecureKey)));
            if (!UseSecureKey && Key == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Key)));
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
            var iterations = Iterations.Get(context);
            var encoding = TextEncoding.Get(context);
            var key = UseSecureKey
                ? new System.Net.NetworkCredential(null, SecureKey.Get(context)).Password
                : Key.Get(context);

            using (var crypto = new Crypto(Algorithm, encoding, iterations))
                action(crypto, key);
        }
    }
}