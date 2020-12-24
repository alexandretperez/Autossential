using Autossential.Enums;
using Autossential.Utils;

namespace Autossential.Activities.Design.Designers
{
    // Interaction logic for DecryptDataTableDesigner.xaml
    public partial class DecryptDataTableDesigner
    {
        public DecryptDataTableDesigner()
        {
            InitializeComponent();

            cbAlgorithms.ItemsSource = EnumUtil.EnumAsDictionary<SymmetricAlgorithms>();
            cbAlgorithms.DisplayMemberPath = "Key";
            cbAlgorithms.SelectedValuePath = "Value";
        }
    }
}