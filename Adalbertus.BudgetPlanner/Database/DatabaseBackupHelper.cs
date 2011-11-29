using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Adalbertus.BudgetPlanner.Database
{
    public class DatabaseBackupHelper
    {
        public bool CreateBackup()
        {
            var backupDirectory = "Archiwum";
            int numberOfBackups = 30;
#if DEBUG
            numberOfBackups = 0;
#endif

            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            if (!File.Exists(SQLiteHelper.DatabaseFile))
            {
                throw new ApplicationException(string.Format("Nie można zrobić kopii bazy danych - baza danych nie istnieje w lokalizacji: {0}", SQLiteHelper.DatabaseFile));
            }

            var backupToFile = Path.Combine(backupDirectory, string.Format("{0}_{1}", DateTime.Now.ToString("yyyy-MM-dd_HHmmss"), SQLiteHelper.DatabaseFile));
            File.Copy(SQLiteHelper.DatabaseFile, backupToFile);

            var backupList = Directory.GetFiles(backupDirectory);
            if (backupList.Length > numberOfBackups)
            {
                foreach (var file in backupList.Take(backupList.Length - numberOfBackups))
                {
                    File.Delete(file);
                }
            }
            


            return true;
        }
    }
}
