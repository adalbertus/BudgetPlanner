using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Adalbertus.BudgetPlanner.Converters
{
    public enum CompareType
    {
        Equal = 0,
        GraterThan,
        GraterThanEqual,
        LessThan,
        LessThanEqual,
    }

    [ValueConversion(typeof(decimal), typeof(bool))]
    public class CompareConverter : IValueConverter
    {
        public CompareType CompareType { get; set; }
        public decimal? CompareValue { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                decimal compareValue = CompareValue.GetValueOrDefault(0);
                if (parameter != null)
                {
                    compareValue = System.Convert.ToDecimal(parameter);
                }
                decimal decimalValue = System.Convert.ToDecimal(value);

                switch(CompareType)
                {
                    case Converters.CompareType.Equal:
                        return decimalValue == compareValue;
                    case Converters.CompareType.GraterThan:
                        return decimalValue > compareValue;
                    case Converters.CompareType.GraterThanEqual:
                        return decimalValue >= compareValue;
                    case Converters.CompareType.LessThan:
                        return decimalValue < compareValue;
                    case Converters.CompareType.LessThanEqual:
                        return decimalValue <= compareValue;
                    default:
                        return DependencyProperty.UnsetValue;
                }
            }
            catch (InvalidCastException)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
