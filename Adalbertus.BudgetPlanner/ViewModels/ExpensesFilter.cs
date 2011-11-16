using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesFilter
    {
        public IEnumerable<CashFlow> CashFlows { get; set; }
    }
}
