using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace Adalbertus.BudgetPlanner.Extensions
{
    /// <summary>
    /// i.e <TextBlock Text="{ext:CultureBinding ExpenseTotalValue, StringFormat='Łącznie: {0:c}'}" /> 
    /// </summary>
    public class CultureBinding : Binding
    {
        public CultureBinding()
        {
            ConverterCulture = CultureInfo.CurrentCulture;
        }

        public CultureBinding(string path)
            : base(path)
        {
            ConverterCulture = CultureInfo.CurrentCulture;
        }
    }
}
