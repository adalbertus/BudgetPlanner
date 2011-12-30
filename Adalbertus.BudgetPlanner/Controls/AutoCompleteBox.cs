using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Adalbertus.BudgetPlanner.Extensions;
using System.Collections;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Windows.Controls;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.Controls
{
    /// <summary>
    /// Based on http://www.codeproject.com/KB/WPF/autocomplete_textbox.aspx
    /// </summary>

    [TemplatePart(Name = "PART_SearchBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_SearchResults", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_SearchResultsHost", Type = typeof(Popup))]
    public class AutoCompleteBox : TextBox
    {
        #region Watermark

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(object), typeof(AutoCompleteBox), new UIPropertyMetadata(null));
        public object Watermark
        {
            get { return (object)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        #endregion //Watermark

        #region WatermarkTemplate

        public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register("WatermarkTemplate", typeof(DataTemplate), typeof(AutoCompleteBox), new UIPropertyMetadata(null));
        public DataTemplate WatermarkTemplate
        {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        #endregion //WatermarkTemplate

        #region SelectAllOnGotFocus

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false));
        public bool SelectAllOnGotFocus
        {
            get { return (bool)GetValue(SelectAllOnGotFocusProperty); }
            set { SetValue(SelectAllOnGotFocusProperty, value); }
        }

        public bool IsClearButtonVisible
        {
            get { return (bool)GetValue(IsClearButtonVisibleProperty); }
            set { SetValue(IsClearButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsClearButtonVisibleProperty =
            DependencyProperty.Register("IsClearButtonVisible", typeof(bool), typeof(AutoCompleteBox), new UIPropertyMetadata(false));



        #endregion //SelectAllOnGotFocus

        static AutoCompleteBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        private bool _suppressEvent = false;
        private bool _bypassSelectAll;

        public bool CanBeEmpty
        {
            get { return (bool)GetValue(CanBeEmptyProperty); }
            set { SetValue(CanBeEmptyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanBeEmpty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanBeEmptyProperty =
            DependencyProperty.Register("CanBeEmpty", typeof(bool), typeof(AutoCompleteBox), new UIPropertyMetadata(true));

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteBox), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));
        private static void OnSelectedItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = o as AutoCompleteBox;
            if (autoCompleteBox == null)
            {
                return;
            }

            autoCompleteBox.Text = GetObjectValue(autoCompleteBox.SelectedItem, autoCompleteBox.FilterBy);
        }

        public object SelectedValue
        {
            get { return (object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(AutoCompleteBox), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));
        private static void OnSelectedValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = o as AutoCompleteBox;
            if (autoCompleteBox == null)
            {
                return;
            }
            if (autoCompleteBox.SearchResults != null)
            {
                autoCompleteBox.SearchResults.SelectedValue = autoCompleteBox.SelectedValue;
            }
        }

        
        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValuePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(AutoCompleteBox), new UIPropertyMetadata(default(string), OnSelectedValuePathChanged));
        private static void OnSelectedValuePathChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = o as AutoCompleteBox;
            if (autoCompleteBox != null && autoCompleteBox.SearchResults != null)
            {
                autoCompleteBox.SearchResults.SelectedValuePath = autoCompleteBox.SelectedValuePath;
            }
        }


        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMemberPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(AutoCompleteBox), new UIPropertyMetadata(default(string), OnDisplayMemberPathChanged));
        private static void OnDisplayMemberPathChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = o as AutoCompleteBox;
            if (autoCompleteBox != null && autoCompleteBox.SearchResults != null)
            {
                autoCompleteBox.SearchResults.DisplayMemberPath = autoCompleteBox.DisplayMemberPath;
            }
        }

        public string FilterBy
        {
            get { return (string)GetValue(FilterByProperty); }
            set { SetValue(FilterByProperty, value); }
        }

        public static readonly DependencyProperty FilterByProperty =
            DependencyProperty.Register("FilterBy", typeof(string), typeof(AutoCompleteBox), new UIPropertyMetadata(default(string)));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoCompleteBox), new UIPropertyMetadata(default(IEnumerable), OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = o as AutoCompleteBox;
            if (autoCompleteBox != null && autoCompleteBox.SearchResults != null)
            {
                autoCompleteBox.SearchResults.ItemsSource = autoCompleteBox.ItemsSource;
            }
        }

        public bool IsSearchEverywhereEnabled
        {
            get { return (bool)GetValue(IsSearchEverywhereEnabledProperty); }
            set { SetValue(IsSearchEverywhereEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSearchEverywhereEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSearchEverywhereEnabledProperty =
            DependencyProperty.Register("IsSearchEverywhereEnabled", typeof(bool), typeof(AutoCompleteBox), new UIPropertyMetadata(false));

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (_suppressEvent)
            {
                return;
            }

            if (!IsFocused)
            {
                return;
            }

            if (SearchResults != null)
            {
                SearchResults.Items.Filter = (item) =>
                {
                    if (Text.IsNullOrWhiteSpace())
                    {
                        return false;
                    }
                    var itemValue = GetObjectValue(item, FilterBy);
                    if (IsSearchEverywhereEnabled)
                    {
                        return itemValue.ToLower().Contains(Text.ToLower());
                    }
                    else
                    {
                        return itemValue.ToLower().StartsWith(Text.ToLower());
                    }
                };

                if (SearchResults.Items.IsEmpty)
                {
                    HideResultHost();
                }
                else
                {
                    ShowResultHost();
                }
            }
        }

        public static string GetObjectValue(object o, string propertyName)
        {
            if (o == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return o.ToString();
            }

            var pi = o.GetType().GetProperty(propertyName);
            if (pi == null)
            {
                return string.Empty;
            }

            var value = pi.GetValue(o, null);
            if (value == null)
            {
                return string.Empty;
            }
            return value.ToString();
        }

        private ListBox SearchResults { get; set; }
        private Popup SearchResultHost { get; set; }

        public AutoCompleteBox()
        {
            SelectAllOnGotFocus = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            var fs = FocusManager.GetFocusScope(this);
            var o = FocusManager.GetFocusedElement(fs);
            if (e.Key == Key.Escape)
            {
                HideResultHost();
                Focus();
            }
            else if ((e.Key == Key.Down) && (o == this))
            {
                if (SearchResults != null)
                {
                    _suppressEvent = true;
                    SearchResults.Focus();
                    _suppressEvent = false;
                }
            }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            if (_bypassSelectAll)
            {
                return;
            }
            if (SelectAllOnGotFocus)
            {
                SelectAll();
            }
            else
            {
                SelectionLength = 0;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _bypassSelectAll = true;
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var parent = frameworkElement.Parent as FrameworkElement;

                if ((parent != null) && parent.Name == "PART_ContentHost")
                {
                    _bypassSelectAll = false;
                }
            }

            if (_bypassSelectAll)
            {

                if (e.OriginalSource is DependencyObject)
                {
                    var button = UI.FindVisualParent<Button>(e.OriginalSource as DependencyObject);
                    if (button != null && button.Name == "PART_ClearButtonHost")
                    {
                        SelectedItem = null;
                        e.Handled = true;
                    }
                }

                base.OnPreviewMouseLeftButtonDown(e);
                return;
            }

            if (!IsKeyboardFocused && SelectAllOnGotFocus)
            {
                e.Handled = true;
                Focus();
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewLostKeyboardFocus(e);

            var oldFocusAsButton = e.OldFocus as Button;
            var newFocusAsButton = e.NewFocus as Button;
            var oldFocusAsListBox = e.OldFocus as ListBox;
            var newFocusAsListBox = e.NewFocus as ListBox;
            var oldFocusAsListBoxItem = e.OldFocus as ListBoxItem;
            var newFocusAsListBoxItem = e.NewFocus as ListBoxItem;

            bool isToListButton = (newFocusAsButton != null) && (newFocusAsButton.Name == "PART_OpenListButtonHost");
            bool isFromListButton = (oldFocusAsButton != null) && (oldFocusAsButton.Name == "PART_OpenListButtonHost");
            bool isToListBox = (newFocusAsListBox != null) && (newFocusAsListBox.Name == "PART_SearchResults");
            bool isFromListBox = (oldFocusAsListBox != null) && (oldFocusAsListBox.Name == "PART_SearchResults");

            if ((e.OldFocus == this) && (isToListButton || isToListBox))
            {
                return;
            }

            if ((e.NewFocus == this) && (isFromListButton || isFromListBox))
            {
                return;
            }

            if ((oldFocusAsListBoxItem != null) && (newFocusAsListBoxItem != null))
            {
                return;
            }

            if (isFromListBox && (newFocusAsListBoxItem != null))
            {
                return;
            }

            if (_suppressEvent)
            {
                return;
            }

            HideResultHost();

            if (!CanBeEmpty)
            {
                _suppressEvent = true;
                Text = GetObjectValue(SelectedItem, FilterBy);
                _suppressEvent = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SearchResultHost = base.GetTemplateChild("PART_SearchResultsHost") as Popup;
            SearchResults = base.GetTemplateChild("PART_SearchResults") as ListBox;
            if (SearchResults != null)
            {
                SearchResults.ItemsSource = ItemsSource;
                SearchResults.DisplayMemberPath = DisplayMemberPath;
                SearchResults.SelectedValuePath = SelectedValuePath;
                SearchResults.SelectedItem = SelectedItem;
                SearchResults.KeyDown += SearchResults_KeyDown;
                SearchResults.PreviewMouseLeftButtonDown += SearchResults_PreviewMouseLeftButtonDown;
            }
            var openListButton = base.GetTemplateChild("PART_OpenListButtonHost") as Button;
            if (openListButton != null)
            {
                openListButton.PreviewMouseLeftButtonDown += delegate
                {
                    ShowHideResultHost();
                    SearchResults.Items.Filter = (item) => { return true; };
                };
            }
        }

        private void SearchResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SearchResults == null)
            {
                return;
            }
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null)
            {
                return;
            }

            var item = SearchResults.ItemContainerGenerator.ItemFromContainer(dep);
            if (item == null)
            {
                return;
            }

            SearchResults.SelectedItem = item;
            UpdateSelection(true);
        }

        private void UpdateSelection(bool focus)
        {
            if (SearchResults == null)
            {
                return;
            }
            SelectedItem = SearchResults.SelectedItem;
            SelectedValue = SearchResults.SelectedValue;
            Text = GetObjectValue(SelectedItem, FilterBy);
            HideResultHost();
            if (focus)
            {
                Focus();
            }
        }

        private void SearchResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                UpdateSelection(true);
            }
            else if (e.Key == Key.Tab)
            {
                UpdateSelection(false);
            }
        }

        private void ShowHideResultHost()
        {
            if (SearchResultHost != null)
            {
                _suppressEvent = true;
                if (SearchResultHost.IsOpen)
                {
                    HideResultHost();
                }
                else
                {
                    ShowResultHost();
                }
                _suppressEvent = false;
            }
        }

        private void ShowResultHost()
        {
            if (SearchResultHost != null)
            {
                SearchResultHost.IsOpen = true;
                if (SearchResults != null)
                {
                    SearchResults.SelectedIndex = -1;
                }
            }
        }

        private void HideResultHost()
        {
            _suppressEvent = true;
            if (SearchResultHost != null)
            {
                SearchResultHost.IsOpen = false;
                if (SearchResults != null)
                {
                    SearchResults.SelectedIndex = -1;
                }
            }
            _suppressEvent = false;
        }
    }
}
