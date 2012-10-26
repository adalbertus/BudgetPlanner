using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Services
{
    public interface IFileService
    {
        string SaveFile(string defaultFileName = null, string defaultExt = null, string fileFilter = null);
        string SaveExcelFile(string defaultFileName = null);
    }
}
