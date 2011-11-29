using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("Expense")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class Expense : Entity
    {
        public virtual SavingValue SavingValue { get; set; }

        [PetaPoco.Column]
        public int CashFlowId { get { return Flow.Id; } set { } }        
        private CashFlow _flow;
        public virtual CashFlow Flow
        {
            get
            {
                return _flow;
            }
            set
            {
                if (_flow == value)
                {
                    return;
                }
                UpdateFow(value);
                NotifyOfPropertyChange(() => Flow);
            }
        }
        public int CashFlowGroupId {
            get
            {
                if (Flow == null)
                {
                    return default(int);
                }
                return Flow.CashFlowGroupId;
            }
        }
        public string GroupName
        {
            get
            {
                if (Flow == null)
                {
                    return string.Empty;
                }
                return Flow.GroupName;
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
                UpdateSavingValue();
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
        
        public static Expense CreateExpense(Budget budget, CashFlow flow, decimal value, string description, DateTime date)
        {
            var expense = new Expense
            {
                Description = description,                
                Date        = date,
                Budget      = budget,
            };

            expense._value = value;
            expense._flow  = flow;            

            if (flow.Saving != null)
            {
                expense.SavingValue = flow.Saving.Deposit(expense, value, date, description);
            }
            
            return expense;
        }

        private void UpdateFow(CashFlow newValue)
        {
            if (_flow == null)
            {
                _flow = newValue;
                return;
            }

            if (_flow.Saving != null)
            {
                _flow.Saving.RemoveValue(SavingValue);
                SavingValue = null;
            }

            _flow = newValue;

            if (_flow.Saving != null)
            {
                SavingValue = _flow.Saving.Deposit(this, Value, Date);
            }
        }

        private void UpdateSavingValue()
        {
            if (SavingValue == null)
            {
                return;
            }
            SavingValue.Value = Value;
        }

        [PetaPoco.Column]
        public int BudgetId { get { return Budget.Id; } set { } }
        public Budget Budget { get; set; }
    }
}
