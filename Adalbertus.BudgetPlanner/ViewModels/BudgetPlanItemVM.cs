using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using System.Collections.Specialized;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetPlanItemVM : PropertyChangedBase
    {
        public CashFlow CashFlow { get; set; }
        public string GroupName { get { return CashFlow.GroupName; } }
        public Budget Budget { get; set; }
        public virtual BindableCollectionExt<BudgetPlan> Values { get; private set; }
        public virtual string Name
        {
            get
            {
                return CashFlow.ToString();
            }
        }

        private bool _isFilterEnabled;

        public bool IsFilterEnabled
        {
            get { return _isFilterEnabled; }
            set
            {
                _isFilterEnabled = value;
                NotifyOfPropertyChange(() => IsFilterEnabled);
            }
        }


        private decimal? _newValue;
        public decimal? NewValue
        {
            get { return _newValue; }
            set
            {
                _newValue = value;
                NotifyOfPropertyChange(() => NewValue);
            }
        }

        private bool _isNewValueFocused;
        public bool IsNewValueFocused
        {
            get { return _isNewValueFocused; }
            set
            {
                _isNewValueFocused = value;
                NotifyOfPropertyChange(() => IsNewValueFocused);
            }
        }

        private string _newDescription;
        public string NewDescription
        {
            get { return _newDescription; }
            set
            {
                _newDescription = value;
                NotifyOfPropertyChange(() => NewDescription);
            }
        }

        public virtual decimal TotalValue
        {
            get
            {
                if (Values == null)
                {
                    return 0;
                }
                return Values.Sum(x => x.Value);
            }
        }

        public decimal TotalExpenseValue
        {
            get
            {
                if (Budget == null)
                {
                    return 0;
                }
                return Budget.Expenses.Where(x => x.CashFlowId == CashFlow.Id).Sum(x => x.Value);
            }
        }

        public decimal TotalBalanceValue
        {
            get
            {
                return TotalValue - TotalExpenseValue;
            }
        }

        public decimal TotalBalanceProcentValue
        {
            get
            {
                if (TotalValue == 0)
                {
                    return 0;
                }
                return (TotalExpenseValue / TotalValue) * 100M;
            }
        }

        public BudgetPlanItemVM(Budget budget, CashFlow cashFlow, IEnumerable<BudgetPlan> planItems)
        {
            IsFilterEnabled = true;
            Budget = budget;
            CashFlow = cashFlow;
            Values = new BindableCollectionExt<BudgetPlan>(planItems);
            Values.CollectionChanged += Values_CollectionChanged;
            Values.PropertyChanged += (s, e) => { NotifyOfPropertyChange(() => Values); };
        }

        public void RefreshUI()
        {
            NotifyOfPropertyChange(() => TotalValue);
            NotifyOfPropertyChange(() => TotalExpenseValue);
            NotifyOfPropertyChange(() => TotalBalanceValue);
            NotifyOfPropertyChange(() => TotalBalanceProcentValue);
        }

        private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = e.OldItems[0] as BudgetPlan;
                Budget.BudgetPlanItems.Remove(item);
            }
        }

        public BudgetPlan AddValue(decimal value, string description)
        {
            var plan = new BudgetPlan
            {
                Value = value,
                Description = description,
                Budget = Budget,
                CashFlow = CashFlow,
            };
            Budget.BudgetPlanItems.Add(plan);
            Values.Add(plan);
            return plan;
        }
    }

}
