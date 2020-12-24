using Autossential.Enums;
using Autossential.Utils;

namespace Autossential.Activities.Design.Designers
{
    // Interaction logic for EncryptTextDesigner.xaml
    public partial class EncryptTextDesigner
    {
        public EncryptTextDesigner()
        {
            InitializeComponent();

            cbAlgorithms.ItemsSource = EnumUtil.EnumAsDictionary<SymmetricAlgorithms>();
            cbAlgorithms.DisplayMemberPath = "Key";
            cbAlgorithms.SelectedValuePath = "Value";
        }
    }
}