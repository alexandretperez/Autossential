using Autossential.Enums;
using Autossential.Utils;

namespace Autossential.Activities.Design.Designers
{
    // Interaction logic for DecryptTextDesigner.xaml
    public partial class DecryptTextDesigner
    {
        public DecryptTextDesigner()
        {
            InitializeComponent();

            cbAlgorithms.ItemsSource = EnumUtil.EnumAsDictionary<SymmetricAlgorithms>();
            cbAlgorithms.DisplayMemberPath = "Key";
            cbAlgorithms.SelectedValuePath = "Value";
        }
    }
}