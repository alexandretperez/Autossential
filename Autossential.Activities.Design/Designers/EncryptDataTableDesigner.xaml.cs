using Autossential.Enums;
using Autossential.Utils;

namespace Autossential.Activities.Design.Designers
{
    // Interaction logic for EncryptDataTableDesigner.xaml
    public partial class EncryptDataTableDesigner
    {
        public EncryptDataTableDesigner()
        {
            InitializeComponent();

            cbAlgorithms.ItemsSource = EnumUtil.EnumAsDictionary<SymmetricAlgorithms>();
            cbAlgorithms.DisplayMemberPath = "Key";
            cbAlgorithms.SelectedValuePath = "Value";
        }
    }
}