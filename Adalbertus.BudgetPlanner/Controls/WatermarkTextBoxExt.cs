using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls;

namespace Adalbertus.BudgetPlanner.Controls
{
    public class WatermarkTextBoxExt : WatermarkTextBox
    {
        static WatermarkTextBoxExt()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkTextBoxExt), new FrameworkPropertyMetadata(typeof(WatermarkTextBoxExt)));
        }

        public bool IsClearButtonVisible
        {
            get { return (bool)GetValue(IsClearButtonVisibleProperty); }
            set { SetValue(IsClearButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsClearButtonVisibleProperty =
            DependencyProperty.Register("IsClearButtonVisible", typeof(bool), typeof(WatermarkTextBoxExt), new UIPropertyMetadata(false));

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(WatermarkTextBoxExt), new UIPropertyMetadata(string.Empty));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var imageBorder = GetTemplateChild("PART_ClearIconBorder") as Border;
            if (imageBorder != null)
            {
                imageBorder.MouseLeftButtonDown += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(Text))
                    {
                        Text = DefaultText;
                    }
                };
            }
        }
    }
}
