using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Database
{
    public interface ICachedService
    {
        IEnumerable<CashFlow> GetAllCashFlows();
    }
}
