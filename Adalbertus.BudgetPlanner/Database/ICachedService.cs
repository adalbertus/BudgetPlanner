using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Database
{
    public interface ICachedService
    {
        void Clear(string key = null);
        IEnumerable<CashFlow> GetAllCashFlows();
        IEnumerable<CashFlowGroup> GetAllCashFlowGroups();
    }
}
