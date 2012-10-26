using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Services
{
    public interface IExportService
    {
        void ExportExpenses(string fileName, string workbookName, IEnumerable<Expense> expenses);

        void ExportBudget(string fileName, IEnumerable<Budget> budgets);
    }
}
