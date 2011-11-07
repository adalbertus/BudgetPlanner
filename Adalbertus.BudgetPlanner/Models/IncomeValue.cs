using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("IncomeValue")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class IncomeValue : Entity
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
        private DateTime _date;
        [PetaPoco.Column]
        public virtual DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                NotifyOfPropertyChange(() => Date);
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

        public virtual string IncomeName
        {
            get
            {
                if (Income == null)
                {
                    return string.Empty;
                }
                return Income.Name;
            }
        }

        [PetaPoco.Column]
        public int IncomeId { get { return Income.Id; } set { } }
        public virtual Income Income { get; set; }

        [PetaPoco.Column]
        public int BudgetId { get { return Budget.Id; } set { } }
        public Budget Budget { get; set; }
    }
}
