using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Adalbertus.BudgetPlanner.Core
{
    public static class ConfigurationHelper
    {
        public static string DefaultConnectionString { 
            get
            {
                return ConfigurationManager.ConnectionStrings["default"].ConnectionString;
            }
        }
    }
}
