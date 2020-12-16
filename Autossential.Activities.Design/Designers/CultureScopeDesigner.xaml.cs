using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Autossential.Activities.Design.Designers
{
    // Interaction logic for CultureScopeDesigner.xaml
    public partial class CultureScopeDesigner
    {
        public CultureScopeDesigner()
        {
            InitializeComponent();


        }

        private void ExpressionTextBox_EditorLostLogicalFocus(object sender, RoutedEventArgs e)
        {
            DisplayCultureName();
        }

        private void DisplayCultureName()
        {
            if (ModelItem.Properties[nameof(CultureScope.CultureName)].Value?.GetCurrentValue() is InArgument<string> arg)
            {
                try
                {
                    if (arg.Expression is Literal<string>)
                    {
                        var info = CultureInfo.CreateSpecificCulture(arg.Expression.ToString());
                        CultureLabel.Content = info.EnglishName;
                    }
                    else
                    {
                        CultureLabel.Content = "Dynamic";
                    }
                }
                catch
                {
                    CultureLabel.Content = "Invalid Content";
                }
            }
            else
            {
                CultureLabel.Content = Thread.CurrentThread.CurrentCulture.EnglishName;
            }
        }

        private void CultureLabel_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayCultureName();
        }
    }
}
