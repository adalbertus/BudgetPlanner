using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Services
{
    public class FileService : IFileService
    {
        #region Private fields
        #endregion Private fields

        #region Constructors
        #endregion Constructors

        #region Properties
        #endregion Properties

        #region Public methods
        public string SaveFile(string defaultFileName = null, string defaultExt = null, string fileFilter = null)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = defaultFileName;
            saveFileDialog.DefaultExt = defaultExt;
            saveFileDialog.Filter = fileFilter;

            var result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        public string SaveExcelFile(string defaultFileName = null)
        {
            return SaveFile(defaultFileName, "xlsx", "Pliki Excel (*.xls;*.xlsx)|*.xls;*.xlsx");
        }
        #endregion Public methods

        #region Private methods
        #endregion Private methods


        
    }
}
