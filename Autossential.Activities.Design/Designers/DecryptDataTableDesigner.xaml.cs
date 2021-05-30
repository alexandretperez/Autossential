using Autossential.Activities.Base;
using Autossential.Activities.Design.Helpers;
using System.Activities.Presentation.Model;

namespace Autossential.Activities.Design.Designers
{
    // Interaction logic for DecryptDataTableDesigner.xaml
    public partial class DecryptDataTableDesigner
    {
        public DecryptDataTableDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (newItem is ModelItem modelItem)
                modelItem.PropertyChanged += ModelItem_PropertyChanged;
        }

        private void ModelItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CryptographyBaseActivity.UseSecureKey))
                CryptographyBaseActivityHelper.NormalizeCryptoKeys(ModelItem);
        }
    }
}