using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesFilterCashFlowVM : PropertyChangedBase
    {
        public string Name { get; set; }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }


        public int CashFlowId { get; set; }

        public ExpensesFilterCashFlowVM()
        {
            IsSelected = true;
        }
    }
}
