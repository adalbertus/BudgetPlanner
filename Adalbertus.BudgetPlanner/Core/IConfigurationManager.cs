using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Core
{
    public interface IConfigurationManager : IDictionary<string, string>
    {
        T GetGetValueOrDefault<T>(string key, T defaultValue = default(T));
    }
}
