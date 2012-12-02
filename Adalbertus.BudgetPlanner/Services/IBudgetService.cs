using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Services
{
    public interface IBudgetService
    {
        IEnumerable<DateTime> GetBudgetDates();
    }
}
