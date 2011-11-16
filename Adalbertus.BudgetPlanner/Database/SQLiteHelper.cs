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
                var connectionBuilder = new SQLiteConnectionStringBuilder(ConfigurationHelper.DefaultConnectionString);
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
                CreateSampleData();
                return;
            }

            SQLiteConnection.CreateFile(DatabaseFile);

            using (var db = CreateDatabaseInstance())
            {
                db.Execute(BudgetPlanner.db_schema);
            };
            CreateSampleData();
        }

        public static void CreateSampleData()
        {
            using (var db = CreateDatabaseInstance())
            using (var tx = db.GetTransaction())
            {
                var defautGroup = db.SingleOrDefault<CashFlowGroup>("WHERE Name = 'Domyślna'");
                if (defautGroup == null)
                {
                    defautGroup = new CashFlowGroup { Name = "Domyślna", Description = "Domyślna grupa", IsReadOnly = true };
                    db.Save(defautGroup);
                    db.Execute("UPDATE CashFlow SET CashFlowGroupId = @0", defautGroup.Id);
                }

                var savingsGroup = db.SingleOrDefault<CashFlowGroup>("WHERE Name = 'Oszczędności'");
                if (savingsGroup == null)
                {
                    savingsGroup = new CashFlowGroup { Name = "Oszczędności", IsReadOnly = true };
                    db.Save(savingsGroup);
                    db.Execute("UPDATE CashFlow SET CashFlowGroupId = @0 WHERE Id IN (SELECT CashFlowId FROM Saving)", savingsGroup.Id);
                }

                AddOrNothingCashFlowByName(db, defautGroup, "Jedzenie");
                AddOrNothingCashFlowByName(db, defautGroup, "Dzieci", "Wydatki na dzieci");
                AddOrNothingCashFlowByName(db, defautGroup, "Dom");

                tx.Complete();
            }
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
