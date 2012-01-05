using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Core;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using Adalbertus.BudgetPlanner.Properties;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Database
{
    public static class SQLiteHelper
    {
        public static string DatabaseFile
        {
            get
            {
                var connectionBuilder = new SQLiteConnectionStringBuilder(AppConfigurationHelper.DefaultConnectionString);
                return connectionBuilder.DataSource;
            }
        }

        public static Database CreateDatabaseInstance()
        {
            return new Database();
        }

        public static void CreateDefaultDatabase()
        {
            if (File.Exists(DatabaseFile))
            {
                //CreateSampleData();
                return;
            }

            SQLiteConnection.CreateFile(DatabaseFile);

            using (var db = CreateDatabaseInstance())
            {
                db.Execute(Resources.db_schema);
                db.Execute(Resources.db_init);
            };            
        }

        private static void AddOrNothingCashFlowByName(Database db, CashFlowGroup defautGroup, string cashFowName, string cashFlowDescription = null)
        {
            var sql = PetaPoco.Sql.Builder.Where("NAME = @0", cashFowName);
            if (!string.IsNullOrWhiteSpace(cashFlowDescription))
            {
                sql.Append(" AND Description = @0", cashFlowDescription);
            }
            var cashFlow = db.FirstOrDefault<CashFlow>(sql);
            if (cashFlow == null)
            {
                cashFlow = new CashFlow
                {
                    Name        = cashFowName,
                    Description = cashFlowDescription,
                    Group       = defautGroup,
                };
                db.Save(cashFlow);
            }
        }
    }
}
