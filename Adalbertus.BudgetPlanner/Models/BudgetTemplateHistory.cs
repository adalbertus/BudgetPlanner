using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("BudgetTemplateHistory")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class BudgetTemplateHistory : Entity
    {
        [PetaPoco.Column]
        public int BudgetTemplateItemId { get { return BudgetTemplateItem.Id; } set { } }
        public BudgetTemplateItem BudgetTemplateItem { get; set; }

        [PetaPoco.Column]
        public int BudgetId { get { return Budget.Id; } set { } }
        public Budget Budget { get; set; }

        [PetaPoco.Column]
        public DateTime Date { get; set; }
    }
}
