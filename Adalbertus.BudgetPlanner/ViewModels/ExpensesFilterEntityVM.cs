using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesFilterEntityVM : PropertyChangedBase
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

        public int EntityId { get; set; }
        public Entity Parent { get; set; }
        public int ParentId {
            get
            {
                if(Parent == null)
                {
                    return default(int);
                }
                return Parent.Id;
            }
        }

        public ExpensesFilterEntityVM()
        {
            IsSelected = true;
        }
    }
}
