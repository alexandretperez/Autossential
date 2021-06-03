using Autossential.Activities.Base;
using Autossential.Activities.Properties;
using System.Activities;

namespace Autossential.Activities
{
    public sealed class EncryptText : CryptographyBaseActivity
    {
        public InArgument<string> Text { get; set; }

        public OutArgument<string> Result { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (Text == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Text)));
            if (Result == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Result)));
        }

        protected override void Execute(CodeActivityContext context)
        {
            string text = Text.Get(context);

            if (!string.IsNullOrEmpty(text))
                ExecuteCrypto(context, (crypto, key) => text = crypto.Encrypt(text, key));

            Result.Set(context, text);
        }
    }
}