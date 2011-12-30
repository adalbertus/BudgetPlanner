using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public class ListViewExtensions : DependencyObject
    {
        public static bool GetIsSortingEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSortingEnabledProperty);
        }

        public static void SetIsSortingEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSortingEnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsSortingEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSortingEnabledProperty =
            DependencyProperty.RegisterAttached("IsSortingEnabled", typeof(bool), typeof(ListViewExtensions), new UIPropertyMetadata(default(bool), null, OnCoerceIsSortingEnabled));

        public static object OnCoerceIsSortingEnabled(DependencyObject source, object value)
        {
            ListView lv = (source as ListView);

            //Ensure we dont have an invalid dependancy object of type ListView.
            if (lv == null)
            {
                throw new ArgumentException("This property may only be used on ListViews");
            }
            lv.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
            lv.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));

            return value;
        }

        public static string GetSortBy(DependencyObject obj)
        {
            return (string)obj.GetValue(SortByProperty);
        }

        public static void SetSortBy(DependencyObject obj, string value)
        {
            obj.SetValue(SortByProperty, value);
        }

        // Using a DependencyProperty as the backing store for SortBy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortByProperty =
            DependencyProperty.RegisterAttached("SortBy", typeof(string), typeof(ListViewExtensions), new UIPropertyMetadata(default(string)));

        public static GridViewColumnHeader GetLastHeaderClicked(DependencyObject obj)
        {
            return (GridViewColumnHeader)obj.GetValue(LastHeaderClickedProperty);
        }

        public static void SetLastHeaderClicked(DependencyObject obj, GridViewColumnHeader value)
        {
            obj.SetValue(LastHeaderClickedProperty, value);
        }

        // Using a DependencyProperty as the backing store for LastHeaderClicked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastHeaderClickedProperty =
            DependencyProperty.RegisterAttached("LastHeaderClicked", typeof(GridViewColumnHeader), typeof(ListViewExtensions), new UIPropertyMetadata(default(GridViewColumnHeader)));

        public static ListSortDirection GetLastDirection(DependencyObject obj)
        {
            return (ListSortDirection)obj.GetValue(LastDirectionProperty);
        }

        public static void SetLastDirection(DependencyObject obj, ListSortDirection value)
        {
            obj.SetValue(LastDirectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for LastDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastDirectionProperty =
            DependencyProperty.RegisterAttached("LastDirection", typeof(ListSortDirection), typeof(ListViewExtensions), new UIPropertyMetadata(ListSortDirection.Ascending));

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;
            if (headerClicked == null)
            {
                return;
            }
            ListView listView = GetAncestor<ListView>(headerClicked);
            if (listView == null)
            {
                return;
            }
            var _lastHeaderClicked = GetLastHeaderClicked(listView);
            var _lastDirection = GetLastDirection(listView);
            if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }

                var sortBy = GetSortBy(headerClicked.Column);
                if (string.IsNullOrWhiteSpace(sortBy))
                {
                    sortBy = headerClicked.Column.Header as string;
                }                
                Sort(listView, sortBy, direction);

                if (direction == ListSortDirection.Ascending)
                {
                    headerClicked.Column.HeaderTemplate = listView.Resources["HeaderTemplateArrowUp"] as DataTemplate;
                }
                else
                {
                    headerClicked.Column.HeaderTemplate = listView.Resources["HeaderTemplateArrowDown"] as DataTemplate;
                }

                // Remove arrow from previously sorted header
                if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                {
                    _lastHeaderClicked.Column.HeaderTemplate = null;
                    
                }


                SetLastHeaderClicked(listView, headerClicked);
                SetLastDirection(listView, direction);
                //_lastHeaderClicked = headerClicked;
                //_lastDirection = direction;

            }
        }

        private static void Sort(ListView lv, string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent != null)
                return (T)parent;
            else
                return null;

        }
    }
}
