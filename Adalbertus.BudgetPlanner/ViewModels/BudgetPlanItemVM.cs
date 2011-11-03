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

        public BudgetPlan AddedBudgetPlanItem { get; private set; }

        public BudgetPlanItemVM(Budget budget, CashFlow cashFlow, IEnumerable<BudgetPlan> planItems)
        {
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
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var item = e.NewItems[0] as BudgetPlan;
                item.Budget = Budget;
                item.CashFlow = CashFlow;
            }
        }
    }

}
