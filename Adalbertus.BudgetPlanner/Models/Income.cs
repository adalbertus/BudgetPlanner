using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("Income")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class Income : Entity
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
                NotifyOfPropertyChange(() => Name);
            }
        }
        public virtual BindableCollectionExt<IncomeValue> Values { get; private set; }
        public virtual decimal TotalValue
        {
            get { return Values.Sum(x => x.Value); }
        }

        public Income()
        {
            Values = new BindableCollectionExt<IncomeValue>();
        }

        public virtual IncomeValue AddIncomeValue(Budget budget, decimal value, DateTime date, string description)
        {
            IncomeValue incomeValue = new IncomeValue
            {
                Date        = date,
                Income      = this,
                Value       = value,
                Description = description,
                Budget      = budget,
            };
            Values.Add(incomeValue);
            return incomeValue;
        }
    }
}
