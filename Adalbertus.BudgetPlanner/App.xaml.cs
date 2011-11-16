using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace Adalbertus.BudgetPlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            EventManager.RegisterClassHandler(typeof(DatePicker),
                                              DatePicker.PreviewKeyDownEvent,
                                              new KeyEventHandler(DatePicker_PreviewKeyDown));
        }

        private void DatePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dp = sender as DatePicker;
            if (dp == null) return;

            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                dp.SetValue(DatePicker.SelectedDateProperty, DateTime.Today);
                return;
            }

            if (!dp.SelectedDate.HasValue) return;

            var date = dp.SelectedDate.Value;
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                dp.SetValue(DatePicker.SelectedDateProperty, date.AddDays(1));
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                dp.SetValue(DatePicker.SelectedDateProperty, date.AddDays(-1));
            }
        }
    }
}
