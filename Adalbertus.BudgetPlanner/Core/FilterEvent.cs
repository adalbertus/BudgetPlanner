using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.ViewModels;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Core
{
    public class FilterEvent
    {
        public IEnumerable<CashFlow> CashFlows { get; set; }
    }
}
