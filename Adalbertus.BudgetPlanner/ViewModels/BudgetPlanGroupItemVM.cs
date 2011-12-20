using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetPlanGroupItemVM : PropertyChangedBase
    {
        public string GroupName { get; set; }
        public BindableCollectionExt<BudgetPlanItemVM> Items { get; set; }
        public decimal TotalValue
        {
            get { return Items.Sum(x => x.TotalValue); }
        }

        public IEnumerable<BudgetPlanItemVM> ToolTipValues
        {
            get
            {
                // prevent from exception: 'DeferRefresh' is not allowed during an AddNew or EditItem transaction.
                // when user is modifing values in DataGrid and hover on border (show ToolTip) above exception
                // occures
                return Items.ToList();
            }
        }

        public decimal TotalExpenseValue
        {
            get { return Items.Sum(x => x.TotalExpenseValue); }
        }

        public decimal TotalBalanceValue
        {
            get { return Items.Sum(x => x.TotalBalanceValue); }
        }

        public BudgetPlanGroupItemVM(IEnumerable<BudgetPlanItemVM> items = null)
        {
            Items = new BindableCollectionExt<BudgetPlanItemVM>(items);
            Items.PropertyChanged += (s, e) => { RefreshUI(); };
            Items.CollectionChanged += (s, e) => { RefreshUI(); };
        }

        public void RefreshUI()
        {
            NotifyOfPropertyChange(() => TotalValue);
            NotifyOfPropertyChange(() => TotalExpenseValue);
            NotifyOfPropertyChange(() => TotalBalanceValue);
        }
    }
}
