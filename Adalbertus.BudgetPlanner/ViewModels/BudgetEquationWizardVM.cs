using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardVM : PropertyChangedBase
    {
        private BudgetCalculatorEquation _equation;
        public BudgetCalculatorEquation Equation { get { return _equation; } }
        public List<BudgetCalculatorItem> InnerEquationList { get; set; }

        public BudgetCalculatorEvaluator BudgetCalculatorEvaluator { get; set; }

        public string EquationName
        {
            get { return _equation.Name; }
            set
            {
                _equation.Name = value;
                NotifyOfPropertyChange(() => EquationName);
            }
        }

        public decimal? EquationValue
        {
            get { return _equation.Value;  }
        }

        public bool IsVisible 
        {
            get { return _equation.IsVisible; }
            set
            {
                _equation.IsVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        public BindableCollectionExt<BudgetCalculatorItem> Items
        {
            get { return _equation.Items; }
        }

        private int _currentPageIndex;
        public int CurrentPageNumber
        {
            get { return _currentPageIndex + 1; }
        }

        public int TotalPagesNumber
        {
            get { return InnerEquationList.Count; }
        }

        public BudgetCalculatorItem CurrentItem { get; set; }
        

        public bool MoveNext()
        {
            RefreshCalculations();
            if (CurrentItem == null)
            {
                return false;
            }

            if (_currentPageIndex < 0)
            {
                return false;
            }

            _currentPageIndex++;
            CurrentItem = Items.GetOrDefaultByIndex(_currentPageIndex);
            if (CurrentItem == null)
            {
                var lastInner = InnerEquationList.GetOrDefaultByIndex(_currentPageIndex);
                if (lastInner != null)
                {
                    CurrentItem = lastInner;
                }
            }
            return true;

        }

        private void RefreshCalculations()
        {
            if (BudgetCalculatorEvaluator != null && Equation != null)
            {
                BudgetCalculatorEvaluator.Refresh(Equation);
            }
        }

        public bool MoveBack()
        {
            RefreshCalculations();
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

        public IEnumerable<BudgetCalculatorEquation> Equations { get; set; }
        public IEnumerable<ComboItemVM<CalculatorValueType>> ValueTypes { get; set; }
        public IEnumerable<ComboItemVM<CalculatorOperatorType>> OperatorTypes { get; set; }
        public IEnumerable<CashFlow> CashFlows { get; set; }
        public IEnumerable<CashFlowGroup> CashFlowGroups { get; set; }
        public IEnumerable<Income> Incomes { get; set; }
        public IEnumerable<Saving> Savings { get; set; }

        private BudgetEquationWizardVM()
        {
            _currentPageIndex = 0;
            _equation = new BudgetCalculatorEquation();
            InnerEquationList = new List<BudgetCalculatorItem>();
        }

        public static BudgetEquationWizardVM CreateInstance()
        {
            return new BudgetEquationWizardVM();
        }

        public void Clear(BudgetCalculatorEquation equation = null)
        {            
            _currentPageIndex = 0;
            InnerEquationList.Clear();
            if (equation == null)
            {
                _equation = new BudgetCalculatorEquation();
            }
            else
            {
                _equation = equation;
                InnerEquationList.AddRange(equation.Items);
            }

            RefreshCalculations();
            CurrentItem = Items.FirstOrDefault();
            Refresh();
        }

        public void CreateDefaultCurrentItem()
        {
            CurrentItem = new BudgetCalculatorItem
            {
                Equation = this.Equation                
            };
            InnerEquationList.Add(CurrentItem);
        }

        public BudgetCalculatorItem AddItem(CalculatorValueType valueType, CalculatorOperatorType operatorType, decimal? value, int foreignId)
        {
            CurrentItem = _equation.AddItem(string.Empty, valueType, operatorType, value, foreignId);
            return CurrentItem;
        }
    }
}
