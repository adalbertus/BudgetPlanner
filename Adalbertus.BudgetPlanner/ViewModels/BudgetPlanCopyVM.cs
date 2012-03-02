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
        private bool _suppressEvent = false;
        public int Id { get; set; }
        public string Name { get; set; }
        public BudgetPlanCopyVM Parent { get; set; }
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                ProcessIsSelected(value);                
            }
        }

        private void ProcessIsSelected(bool value)
        {
            if (_suppressEvent)
            {
                return;
            }
            _suppressEvent = true;
            // all children should be selected/deselected
            // parent should be selected when value == true
            // parent should be deselected when all children are dselected

            _isSelected = value;

            if (value && Parent != null)
            {
                Parent.SelectWithoutChildPropagation();
            }

            if (!value && Parent != null)
            {
                Parent.DeselectIfAllChildrenAreNotSelected();
            }

            Children.ForEach(x => x.IsSelected = value);

            NotifyOfPropertyChange(() => IsSelected);
            _suppressEvent = false;
        }

        public IList<BudgetPlanCopyVM> Children { get; private set; }

        public BudgetPlanCopyVM()
            : this(null)
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

        private void SelectWithoutChildPropagation()
        {
            if (IsSelected)
            {
                return;
            }
            _isSelected = true;
            if (Parent != null)
            {
                Parent.SelectWithoutChildPropagation();
            }
            NotifyOfPropertyChange(() => IsSelected);
        }

        private void DeselectIfAllChildrenAreNotSelected()
        {
            if (!IsSelected)
            {
                return;
            }
            if (!Children.Any(x => x.IsSelected))
            {
                _isSelected = false;
                NotifyOfPropertyChange(() => IsSelected);
                if (Parent != null)
                {
                    Parent.DeselectIfAllChildrenAreNotSelected();
                }
            }
        }

    }
}
