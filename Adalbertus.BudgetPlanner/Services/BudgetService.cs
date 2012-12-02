using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;

namespace Adalbertus.BudgetPlanner.Services
{
    public class BudgetService : IBudgetService
    {
        public BudgetService(IDatabase database)
        {
            Database = database;
        }

        public IDatabase Database { get; private set; }

        public IEnumerable<DateTime> GetBudgetDates()
        {
            var sql = PetaPoco.Sql.Builder.Select("date(DateFrom)").From("Budget").OrderBy("DateFrom DESC");
            var dates = Database.Query<DateTime>(sql).ToList();
            return dates;
        }
    }
}
