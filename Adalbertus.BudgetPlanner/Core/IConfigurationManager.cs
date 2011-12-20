using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Core
{
    public interface IConfigurationManager : IDictionary<string, Configuration>
    {
        T GetValueOrDefault<T>(string key, T defaultValue = default(T));
        void SaveValue(string key, object value);
    }
}
