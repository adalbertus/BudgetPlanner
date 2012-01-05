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



        public static bool GetIsColumnAutoSizeEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsColumnAutoSizeEnabledProperty);
        }

        public static void SetIsColumnAutoSizeEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsColumnAutoSizeEnabledProperty, value);
        }

        
        public static readonly DependencyProperty IsColumnAutoSizeEnabledProperty =
            DependencyProperty.RegisterAttached("IsColumnAutoSizeEnabled", typeof(bool), typeof(DataGridExtensions), new UIPropertyMetadata(false, OnIsColumnAutoSizeEnabledChanged));

        private static void OnIsColumnAutoSizeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FrameworkElement)d;

            if ((bool)e.NewValue == true)
            {
                control.TargetUpdated += OnControlTargetUpdated;
            }
            else
            {
                control.TargetUpdated -= OnControlTargetUpdated;
            }
        }

        /// <remarks>
        /// To fire this event you must add in column binding: NotifyOnTargetUpdated=True
        /// </remarks>
        private static void OnControlTargetUpdated(object sender, DataTransferEventArgs e)
        {
            var listView = sender as ListView;
            
            var dataGrid = sender as DataGrid;
            if (listView != null && listView.View is GridView)
            {
                var gridView = listView.View as GridView;
                gridView.Columns.ForEach(x =>
                {
                    if (double.IsNaN(x.Width))
                    {
                        x.Width = x.ActualWidth;
                    }
                    x.Width = double.NaN;
                });
            }
        }
    }
}
