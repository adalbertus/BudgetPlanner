using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;

namespace Adalbertus.BudgetPlanner.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class IsEmptyConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return true;
            }

            var collection = value as IEnumerable<object>;
            if (collection != null)
            {
                return collection.Any();
            }

            throw new NotImplementedException("Only IEnumerable<> is implemented");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
