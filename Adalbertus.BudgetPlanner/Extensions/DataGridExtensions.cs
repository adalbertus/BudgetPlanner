using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Media;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public class DataGridExtensions : DependencyObject
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
            DependencyProperty.RegisterAttached("ScrollToItem", typeof(object), typeof(DataGridExtensions), new FrameworkPropertyMetadata(null, OnScrollToItemPropertyChanged));

        private static void OnScrollToItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;
            if (dataGrid == null)
            {
                return;
            }
            dataGrid.ScrollIntoView(e.NewValue);
        }
    }
}
