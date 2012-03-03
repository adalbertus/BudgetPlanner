using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public class ListBoxExtensions : DependencyObject
    {
        public static object GetScrollToItem(DependencyObject obj)
        {
            return (object)obj.GetValue(ScrollToItemProperty);
        }

        public static void SetScrollToItem(DependencyObject obj, object value)
        {
            obj.SetValue(ScrollToItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for ScrollToItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollToItemProperty =
            DependencyProperty.RegisterAttached("ScrollToItem", typeof(object), typeof(ListBoxExtensions), new FrameworkPropertyMetadata(null, OnScrollToItemPropertyChanged));

        private static void OnScrollToItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            if (listBox == null)
            {
                return;
            }
            listBox.ScrollIntoView(e.NewValue);
        }

    }
}
