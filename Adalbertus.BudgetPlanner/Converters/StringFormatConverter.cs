using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Adalbertus.BudgetPlanner.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : IValueConverter
    {
        public string StringFormat { get; set; }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(StringFormat))
            {
                return string.Empty;
            }
            return string.Format(StringFormat, value);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
