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
                database.Execute(Resources.db_update_v002);
                tx.Complete();
            }
        }
    }
}
