using Autossential.Activities.Base;
using Autossential.Activities.Properties;
using Autossential.Security;
using System.Activities;

namespace Autossential.Activities
{
    public class DecryptText : CryptographyBaseActivity
    {
        public InArgument<string> Text { get; set; }

        public OutArgument<string> Result { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Text == null) metadata.AddValidationError(Resources.Validation_ValueErrorFormat(nameof(Text)));

            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            var text = Text.Get(context);
            var password = Key.Get(context);
            var encoding = TextEncoding.Get(context);
            var iterations = Iterations.Get(context);

            string result = text;
            if (!string.IsNullOrEmpty(text))
            {
                using (var crypto = new Crypto(Algorithm, encoding, iterations))
                    result = crypto.Decrypt(text, password);
            }

            Result.Set(context, result);
        }
    }
}