using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Adalbertus.BudgetPlanner.Controls
{
    public class NumericTextBox : WatermarkTextBoxExt
    {
        public string FormattedValue
        {
            get { return (string)GetValue(FormattedValueProperty); }
            set { SetValue(FormattedValueProperty, value); }
        }

        public static readonly DependencyProperty FormattedValueProperty =
            DependencyProperty.Register("FormattedValue", typeof(string), typeof(NumericTextBox), new UIPropertyMetadata(default(string)));


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal?), typeof(NumericTextBox), new FrameworkPropertyMetadata(default(decimal?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
        public decimal? Value
        {
            get { return (decimal?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericControl = o as NumericTextBox;
            if (numericControl == null)
            {
                return;
            }
            numericControl.UpdateFormattedValue();
            numericControl.FormatValue();
        }

        public decimal? MinValue
        {
            get { return (decimal?)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(decimal?), typeof(NumericTextBox), new UIPropertyMetadata(default(decimal?), OnMinValueChanged));

        private static void OnMinValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericControl = o as NumericTextBox;
            if (numericControl == null)
            {
                return;
            }
            numericControl.UpdateValueConstraints();
            numericControl.FormatValue();
        }

        public decimal? MaxValue
        {
            get { return (decimal?)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(decimal?), typeof(NumericTextBox), new UIPropertyMetadata(default(decimal?), OnMaxValueChanged));

        private static void OnMaxValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericControl = o as NumericTextBox;
            if (numericControl == null)
            {
                return;
            }
            numericControl.UpdateValueConstraints();
            numericControl.FormatValue();
        }

        public bool IsIntegerOnly
        {
            get { return (bool)GetValue(IsIntegerOnlyProperty); }
            set { SetValue(IsIntegerOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsIntegerOnlyProperty = DependencyProperty.Register("IsIntegerOnly", typeof(bool), typeof(NumericTextBox), new UIPropertyMetadata(false));

        

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(NumericTextBox), new UIPropertyMetadata(String.Empty, OnFormatStringChanged));
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var numericControl = o as NumericTextBox;
            if (numericControl == null)
            {
                return;
            }
            numericControl.FormatValue();
        }

        public decimal? DefaultValue
        {
            get { return (decimal?)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(decimal?), typeof(NumericTextBox), new UIPropertyMetadata(default(decimal?)));

        public NumericTextBox()
        {
            TextAlignment = System.Windows.TextAlignment.Right;
            SelectAllOnGotFocus = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Text = DefaultText;
                Value = DefaultValue;                
            }
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                if (Value.HasValue)
                {
                    Value++;
                }
                else
                {
                    Value = 0;
                }
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                if (Value.HasValue)
                {
                    Value--;
                }
                else
                {
                    Value = 0;
                }

            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            FormatValue();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (!IsReadOnly)
            {
                Text = Value.ToString();
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (IsFocused)
            {
                ParseText();
            }

            base.OnTextChanged(e);
        }

        protected virtual void ParseText()
        {
            decimal value;
            if (string.IsNullOrWhiteSpace(Text))
            {
                Value = DefaultValue;
            }
            else if (decimal.TryParse(Text, out value))
            {
                Value = value;
            }
            UpdateValueConstraints();
        }

        private void UpdateFormattedValue()
        {
            if (Value.HasValue)
            {
                FormattedValue = Value.Value.ToString(FormatString);
            }
            else
            {
                FormattedValue = string.Empty;
            }
        }

        private void UpdateValueConstraints()
        {
            if (MinValue.HasValue)
            {
                if (Value < MinValue)
                {
                    Value = MinValue;
                }
            }

            if (MaxValue.HasValue)
            {
                if (Value > MaxValue)
                {
                    Value = MaxValue;
                }
            }

            if (IsIntegerOnly && Value.HasValue)
            {
                Value = (int) Value.Value;
            }
        }


        protected virtual void FormatValue()
        {
            if (!IsFocused)
            {
                Text = FormattedValue;
            }
            else
            {
                if (Value.HasValue)
                {
                    if (Text != Value.ToString())
                    {
                        Text = Value.ToString();
                        SelectAll();
                    }
                }
                else
                {
                    Text = string.Empty;
                }
            }
        }
    }
}
