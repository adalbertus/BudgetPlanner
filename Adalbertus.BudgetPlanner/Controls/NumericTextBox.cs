using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Adalbertus.BudgetPlanner.Controls
{
    public class NumericTextBox : WatermarkTextBox
    {
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
            numericControl.FormatValue();
        }
        
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


        public NumericTextBox()
        {
            TextAlignment = System.Windows.TextAlignment.Right;
            SelectAllOnGotFocus = true;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            FormatValue();            
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (!IsReadOnly)
            {
                Text = Value.ToString();
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (IsFocused)
            {
                decimal value;
                if (decimal.TryParse(Text, out value))
                {
                    Value = value;
                }
            }
            base.OnTextChanged(e);
        }

        private void FormatValue()
        {
            if (!IsFocused)
            {
                if (Value.HasValue)
                {
                    Text = Value.Value.ToString(FormatString);
                }
                else
                {
                    Text = string.Empty;
                }
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
