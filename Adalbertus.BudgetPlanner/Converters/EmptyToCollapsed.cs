using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Adalbertus.BudgetPlanner.Converters
{
    public class EmptyToCollapsed : IValueConverter
    {
        public bool IsResultInverted { get; set; }

        public EmptyToCollapsed()
        {
            IsResultInverted = false;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility result = Visibility.Visible;
            if (value == null)
            {
                if (IsResultInverted)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            var collection = value as IEnumerable<object>;
            if (collection != null)
            {
                if (collection.Any())
                {
                    result = Visibility.Visible;
                }
                else
                {
                    result = Visibility.Collapsed;
                }
                if (IsResultInverted)
                {
                    return result == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
                return result;
            }

            var stringValue = value as string;
            if (value is string)
            {
                if (string.IsNullOrWhiteSpace(value as string))
                {
                    result = Visibility.Collapsed;
                }
                else
                {
                    result = Visibility.Visible;
                }
            }

            if (value is bool)
            {
                if ((bool)value)
                {
                    result = Visibility.Visible;
                }
                else
                {
                    result = Visibility.Collapsed;
                }
            }
            if (IsResultInverted)
            {
                return result == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }

            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
