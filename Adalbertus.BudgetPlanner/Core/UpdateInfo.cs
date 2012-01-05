using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Core
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public DateTime Date { get; set; }
        public string Changes { get; set; }
        public string Url { get; set; }
        public string ExeFile { get; set; }
    }
}
