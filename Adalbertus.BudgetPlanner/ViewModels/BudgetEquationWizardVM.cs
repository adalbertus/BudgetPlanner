using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardVM : PropertyChangedBase
    {
        public List<BudgetCalculatorItem> Items { get; set; }
        private string _equationName;
        public string EquationName
        {
            get { return _equationName; }
            set
            {
                _equationName = value;
                NotifyOfPropertyChange(() => EquationName);
            }
        }

        private int _currentPageIndex;
        public int CurrentPageNumber
        {
            get { return _currentPageIndex + 1; }
        }

        public int TotalPagesNumber
        {
            get { return Items.Count; }
        }

        public BudgetCalculatorItem CurrentItem { get; set; }
        

        public bool MoveNext()
        {
            if (CurrentItem == null)
            {
                return false;
            }

            if (_currentPageIndex < 0)
            {
                return false;
            }

            _currentPageIndex++;
            if (Items.Count >= _currentPageIndex + 1)
            {
                CurrentItem = Items[_currentPageIndex];
            }
            else
            {
                CurrentItem = null;
            }
            return true;

        }

        public bool MoveBack()
        {
            if (_currentPageIndex <= 0)
            {
                return false;
            }
            
            _currentPageIndex--;

            if (_currentPageIndex >= Items.Count)
            {
                return false;               
            }

            CurrentItem = Items[_currentPageIndex];
            return true;
        }

        public int MyProperty { get; set; }
        
        public IEnumerable<BudgetCalculatorEquation> Equations { get; set; }
        public IEnumerable<dynamic> ValueTypes { get; set; }
        public IEnumerable<dynamic> OperatorTypes { get; set; }

        public BudgetEquationWizardVM()
        {
            Items = new List<BudgetCalculatorItem>();
            _currentPageIndex = 0;
        }

        public BudgetCalculatorItem AddItem(CalculatorValueType valueType, CalculatorOperatorType operatorType, decimal? value = null, BudgetCalculatorEquation equation = null)
        {
            CurrentItem = new BudgetCalculatorItem
            {
                ValueType = valueType,
                OperatorType = operatorType,
                Value = value,
                Equation = equation,
                ForeignId = 0,
            };
            if (equation != null)
            {
                CurrentItem.ForeignId = equation.Id;
            }

            Items.Add(CurrentItem);

            return CurrentItem;
        }
    }
}
