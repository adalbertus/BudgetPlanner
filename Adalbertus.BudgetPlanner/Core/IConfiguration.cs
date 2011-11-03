using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Core
{
    public interface IConfiguration : IDictionary<string, string>
    {
        void AddRange(IEnumerable<Models.Dictionary> items);
    }
}
