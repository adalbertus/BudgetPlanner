using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("Configuration")]
    [PetaPoco.PrimaryKey("Key")]
    [PetaPoco.ExplicitColumns]
    public class Configuration : PropertyChangedBase
    {
        [PetaPoco.Column]
        public string Key { get; set; }
        [PetaPoco.Column]
        public bool IsActive { get; set; }
        [PetaPoco.Column]
        public string Value { get; set; }
        [PetaPoco.Column]
        public string Decription { get; set; }
    }
}
