using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("Dictionary")]
    [PetaPoco.PrimaryKey("Key")]
    public class Dictionary
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }
        public bool IsActive { get; set; }
    }
}
