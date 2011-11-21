using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public class ImageExtension
    {
        public static ImageSource GetImageSource(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(SourceProperty);
        }

        public static void SetImageSource(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("ImageSource", typeof(ImageSource), typeof(ImageExtension), new FrameworkPropertyMetadata((ImageSource)null));
    }
}
