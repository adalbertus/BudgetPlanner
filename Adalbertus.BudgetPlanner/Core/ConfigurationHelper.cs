using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Core
{
    public static class ConfigurationHelper
    {
        public static string DefaultConnectionString { 
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["default"].ConnectionString;
            }
        }
    }
}
