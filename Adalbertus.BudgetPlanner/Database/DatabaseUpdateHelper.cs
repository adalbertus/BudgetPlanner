using System.Data.Common;
using System;
using Adalbertus.BudgetPlanner.Properties;

namespace Adalbertus.BudgetPlanner.Database
{
    public static class DatabaseUpdateHelper
    {
        public static void UpdateIfNeeded(IDatabase database, int currentDatabaseVersion)
        {
            try
            {
                switch (currentDatabaseVersion)
                {
                    case 1:
                        UpdateToVersion2(database);
                        break;
                    case 2:
                        UpdateToVersion3(database);
                        return;
                    default:
                        throw new InvalidOperationException(string.Format("Procedura aktualizacji bazy danych nie może być wykonana. Błędna wersja bazy danych: {0}", currentDatabaseVersion));
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("W trakcie aktualizacji bazy danych z wersji {0} do wersji {1} wystąpił błąd:\r\n{2}", currentDatabaseVersion, currentDatabaseVersion + 1, ex.Message));
            }
            UpdateIfNeeded(database, currentDatabaseVersion + 1);

        }

        private static void UpdateToVersion2(IDatabase database)
        {            
            using (var tx = database.GetTransaction())
            {
                database.AlterTable("Saving", "[StartingBalance]", "ALTER TABLE [Saving] ADD COLUMN [StartingBalance] NUMERIC NOT NULL DEFAULT 0");
                database.Execute(Resources.db_update_v002);
                tx.Complete();
            }
        }

        private static void UpdateToVersion3(IDatabase database)
        {
            using (var tx = database.GetTransaction())
            {
                database.AlterTable("Note", "[BudgetId]", "ALTER TABLE [Note] ADD COLUMN [BudgetId] INT");
                database.Execute(Resources.db_update_v003);
                tx.Complete();
            }
        }

        private static void AlterTable(this IDatabase database, string tableName, string columnName, string query)
        {
            // SQLite doesn't allow to alter table weather column in it exists or not
            var ddlTable = database.ExecuteScalar<string>(PetaPoco.Sql.Builder.Select("sql")
                .From("sqlite_master")
                .Where("name=@0", tableName));
            if (!ddlTable.ToLower().Contains(columnName.ToLower()))
            {
                database.Execute(query);
            }
        }
    }
}
