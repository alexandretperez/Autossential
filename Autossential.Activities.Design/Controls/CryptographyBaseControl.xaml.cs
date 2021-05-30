using Autossential.Enums;
using Autossential.Utils;
using System.Windows;

namespace Autossential.Activities.Design.Controls
{
    // Interaction logic for CryptographyBaseControl.xaml
    public partial class CryptographyBaseControl
    {
        public CryptographyBaseControl()
        {
            InitializeComponent();

            cbAlgorithms.ItemsSource = EnumUtil.EnumAsDictionary<SymmetricAlgorithms>();
            cbAlgorithms.DisplayMemberPath = "Key";
            cbAlgorithms.SelectedValuePath = "Value";
        }

        #region DependencyProperty : KeyToolTipProperty
        public string KeyToolTip
        {
            get => (string)GetValue(KeyToolTipProperty);
            set => SetValue(KeyToolTipProperty, value);
        }
        public static readonly DependencyProperty KeyToolTipProperty
            = DependencyProperty.Register(nameof(KeyToolTip), typeof(string), typeof(CryptographyBaseControl), new PropertyMetadata(default(string)));

        #endregion
    }
}