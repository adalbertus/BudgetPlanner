using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Adalbertus.BudgetPlanner.Converters
{
    [ValueConversion(typeof(decimal), typeof(bool))]
    public class NumericToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return (System.Convert.ToDecimal(value) > 0);
            }
            catch (InvalidCastException)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? 1 : 0;
        }

        #endregion
    }
}
