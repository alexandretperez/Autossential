using Autossential.Enums;
using Autossential.Utils;

namespace Autossential.Activities.Design.Designers
{
    // Interaction logic for StopwatchDesigner.xaml
    public partial class StopwatchDesigner
    {
        public StopwatchDesigner()
        {
            InitializeComponent();

            cbMethods.ItemsSource = EnumUtil.EnumAsDictionary<StopwatchMethods>();
            cbMethods.DisplayMemberPath = "Key";
            cbMethods.SelectedValuePath = "Value";
        }
    }
}