using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Text;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("Saving")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class Saving : Entity
    {
        private string _name;
        [PetaPoco.Column]
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                CashFlow.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string NameWithBilance 
        {
            get { return string.Format("{0} [{1}]", Name, TotalValue.ToString("C2")); }
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
        public int CashFlowId { get { return CashFlow.Id; } set { } }

        public CashFlow CashFlow { get; set; }

        public virtual IList<SavingValue> Values { get; private set; }
        
        public virtual decimal TotalValue
        {
            get { return Values.Sum(x => x.Value); }            
        }

        public Saving()
        {
            Values = new BindableCollectionExt<SavingValue>();
            CashFlow = new CashFlow { Description = "Oszczędności" };
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual SavingValue Withdraw(decimal value, DateTime date, string description, Budget withdrawToBudget)
        {
            SavingValue newSavingValue = new SavingValue
            {
                Saving      = this,
                Date        = date,
                Value       = -value,
                Description = description,
                Budget      = withdrawToBudget,
                Expense     = new Expense(),
            };
            newSavingValue.UpdateDescription();
            Values.Add(newSavingValue);
            return newSavingValue;
        }

        public virtual SavingValue Deposit(Expense depositSource, decimal value, DateTime date, string description = null)
        {
            SavingValue newSavingValue = new SavingValue
                            {
                                Date        = date,
                                Value       = value,
                                Saving      = this,
                                Description = description,
                                Expense     = depositSource,
                            };
            if (depositSource != null)
            {
                newSavingValue.Budget = depositSource.Budget;
                newSavingValue.UpdateDescription();
            }
            
            Values.Add(newSavingValue);
            
            return newSavingValue;
        }

        public virtual void RemoveValue(SavingValue value)
        {
            Values.Remove(value);
        }

        public void DepositSavingValue(Budget budget, Expense expense, decimal value, DateTime date)
        {
            throw new NotImplementedException();
        }
    }

}
