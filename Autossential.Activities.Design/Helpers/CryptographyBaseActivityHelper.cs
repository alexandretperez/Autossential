using Autossential.Activities.Base;
using System.Activities.Presentation.Model;

namespace Autossential.Activities.Design.Helpers
{
    public static class CryptographyBaseActivityHelper
    {
        public static void NormalizeCryptoKeys(ModelItem modelItem)
        {
            var value = (bool)modelItem.Properties[nameof(CryptographyBaseActivity.UseSecureKey)].ComputedValue;
            modelItem.Properties[value ? nameof(CryptographyBaseActivity.Key) : nameof(CryptographyBaseActivity.SecureKey)].ClearValue();
        }
    }
}