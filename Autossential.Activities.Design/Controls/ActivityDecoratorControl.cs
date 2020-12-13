using System.Windows;
using System.Windows.Controls;

namespace Autossential.Activities.Design.Controls
{
    public class ActivityDecoratorControl : ContentControl
    {
        static ActivityDecoratorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ActivityDecoratorControl),
                new FrameworkPropertyMetadata(typeof(ActivityDecoratorControl))
            );
        }
    }
}