using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Database
{
    public static class Commands
    {
        public static bool AreBudgetTemplatesApplied(this IDatabase database, Budget budget)
        {
            if (database == null)
            {
                throw new NullReferenceException("database");
            }

            if(budget == null)
            {
                throw new NullReferenceException("budget");
            }

            return database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                    .Select("COUNT(*)")
                    .From("[BudgetTemplateHistory]")
                    .Where("BudgetId = @0", budget.Id)) > 0;
        }


    }
}
