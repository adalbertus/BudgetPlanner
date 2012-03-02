using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Extensions;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetPlanCopyVM : PropertyChangedBase
    {
        private bool _supressEvents = false;

        public int Id { get; set; }
        public string Name { get; set; }
        public BudgetPlanCopyVM Parent { get; set; }
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_supressEvents)
                {
                    return;
                }

                _supressEvents = true;
                _isSelected = value;
                if (Parent != null && value)
                {
                    Parent.IsSelected = value;
                }
                if (!value)
                {
                    Children.ForEach(x => x.IsSelected = value);
                }
                
                NotifyOfPropertyChange(() => IsSelected);
                _supressEvents = false;
            }
        }

        public IList<BudgetPlanCopyVM> Children { get; private set; }

        public BudgetPlanCopyVM()
            :this(null)
        {
        }
        public BudgetPlanCopyVM(BudgetPlanCopyVM parent)
        {
            Parent = parent;
            Children = new List<BudgetPlanCopyVM>();            
        }

        public BudgetPlanCopyVM AddChild(BudgetPlanCopyVM child)
        {
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        public BudgetPlanCopyVM AddChild(string name, int id)
        {
            var child = new BudgetPlanCopyVM
            {
                Id = id,
                Name = name,
                Parent = this,
            };
            Children.Add(child);
            return child;
        }
    }
}
