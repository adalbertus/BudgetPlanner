using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("SavingValue")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class SavingValue : Entity
    {
        public bool IsReadOnly
        {
            get
            {
                bool isFromBudgetPlan = Budget != null;
                bool isFromExpense = Expense != null;
                return isFromBudgetPlan || isFromExpense;
            }
        }

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

        public virtual decimal BudgetValue
        {
            get
            {
                return -Value;
            }
            set
            {
                Value = -value;
                NotifyOfPropertyChange(() => BudgetValue);
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

        public virtual string SavingName
        {
            get
            {
                if (Saving == null)
                {
                    return string.Empty;
                }
                return Saving.Name;
            }
        }

        [PetaPoco.Column]
        public int SavingId { get { return Saving.Id; } set { } }
        public virtual Saving Saving { get; set; }

        [PetaPoco.Column]
        public int BudgetId
        {
            get
            {
                if (Budget == null)
                {
                    return default(int);
                }
                else
                {
                    return Budget.Id;
                }
            }
            set { }
        }
        public virtual Budget Budget { get; set; }

        [PetaPoco.Column]
        public int ExpenseId
        {
            get
            {
                if (Expense == null)
                {
                    return default(int);
                }
                else
                {
                    return Expense.Id;
                }
            }
            set { }
        }
        public virtual Expense Expense { get; set; }

        public void UpdateDescription()
        {
            if (Budget == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            
            sb.AppendFormat("Budżet {0}-{1}", Budget.DateFrom.Year.ToString("0000"), Budget.DateFrom.Month.ToString("00"));
            if (Expense != null)
            {
                if (!string.IsNullOrWhiteSpace(Expense.Description))
                {
                    sb.AppendFormat(": {0}", Expense.Description);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Description))
                {
                    sb.AppendFormat(": {0}", Description);
                }
            }
            Description = sb.ToString();
        }
    }

}
