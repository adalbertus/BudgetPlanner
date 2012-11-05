using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using ILCalc;
using System.Globalization;
using Adalbertus.BudgetPlanner.Core;
using System.Windows.Controls;
using System.Windows.Input;

namespace Adalbertus.BudgetPlanner.Controls
{
    public class CalculatorTextBox : NumericTextBox
    {
        static CalculatorTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalculatorTextBox), new FrameworkPropertyMetadata(typeof(CalculatorTextBox)));
        }

        public decimal? Result
        {
            get { return (decimal?)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(decimal?), typeof(CalculatorTextBox), new UIPropertyMetadata(default(decimal?), OnResultChanged));

        private static void OnResultChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var calculatorTextBox = o as CalculatorTextBox;
            if (calculatorTextBox == null)
            {
                return;
            }

            calculatorTextBox.ShowHideResultPopup();
        }

        public string ShowResultPrefix
        {
            get { return (string)GetValue(ShowResultPrefixProperty); }
            set { SetValue(ShowResultPrefixProperty, value); }
        }

        public static readonly DependencyProperty ShowResultPrefixProperty =
            DependencyProperty.Register("ShowResultPrefix", typeof(string), typeof(CalculatorTextBox), new UIPropertyMetadata(default(string)));

        private CalcContext<decimal> _calculator;
        private bool _suppressEvent;
        public Popup ResultPopup { get; private set; }
        public CalculatorTextBox()
        {
            _calculator = new CalcContext<decimal>()
            {
                Culture = CultureInfo.InvariantCulture,
            };
            _suppressEvent = false;
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnWindowMouseDown));
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseWheelEvent, new MouseWheelEventHandler(OnMouseWheel));
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            var calculatorTextBox = e.OriginalSource as CalculatorTextBox;
            if (calculatorTextBox == null)
            {
                if (ResultPopup != null)
                {
                    ResultPopup.IsOpen = false;
                }
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var calculatorTextBox = e.OriginalSource as CalculatorTextBox;
            if (calculatorTextBox == null)
            {
                if (ResultPopup != null)
                {
                    ResultPopup.IsOpen = false;
                }
            }
        }

        private void ShowHideResultPopup()
        {
            if (ResultPopup != null)
            {
                ResultPopup.IsOpen = IsFocused && Result.HasValue;
            }
        }

        protected override void OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            if (ResultPopup != null)
            {
                ResultPopup.IsOpen = false;
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Escape:
                case System.Windows.Input.Key.Enter:
                    if (ResultPopup != null)
                    {
                        ResultPopup.IsOpen = false;
                        base.FormatValue();
                        return;
                    }
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void ParseText()
        {
            var expression = Text.Trim().Replace(",", ".");
            if (string.IsNullOrWhiteSpace(expression))
            {
                Value = DefaultValue;
                Result = DefaultValue;
            }
            else
            {
                try
                {
                    _suppressEvent = true;
                    Result = _calculator.Evaluate(expression);
                    Value = Result;
                }
                catch (DivideByZeroException)
                {
                    // ignore it
                }
                catch (SyntaxException)
                {
                    // ignore it
                }
                finally
                {
                    _suppressEvent = false;
                }
            }
        }

        protected override void FormatValue()
        {
            if (_suppressEvent)
            {
                return;
            }

            base.FormatValue();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ResultPopup = GetTemplateChild("PART_ResultHost") as Popup;
            this.SelectAll();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            SelectAll();
        }
    }
}
