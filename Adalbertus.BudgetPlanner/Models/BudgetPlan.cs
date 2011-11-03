using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("BudgetPlan")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class BudgetPlan : Entity
    {
        private decimal _value;
        [PetaPoco.Column]
        public virtual decimal Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }
        private string _description;
        [PetaPoco.Column]
        public virtual string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        [PetaPoco.Column]
        public int BudgetId { get { return Budget.Id; } set { } }
        public Budget Budget { get; set; }
        
        [PetaPoco.Column]
        public int CashFlowId { get { return CashFlow.Id; } set { } }
        public CashFlow CashFlow { get; set; }
    }
}
