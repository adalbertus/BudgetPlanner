using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardElementViewModel : WizardPageViewModel<BudgetEquationWizardVM>
    {
        public BudgetEquationWizardElementViewModel()
        {
            BackPageName = typeof(BudgetEquationWizardStartViewModel).Name;
            NextPageName = typeof(BudgetEquationWizardElementViewModel).Name;
        }

        public IEnumerable<dynamic> ValueTypes { get { return Model.ValueTypes; } }
        public IEnumerable<BudgetCalculatorEquation> Equations { get { return Model.Equations; } }

        private dynamic _selectedValueType;
        public dynamic SelectedValueType
        {
            get { return _selectedValueType; }
            set
            {
                _selectedValueType = value;
                NotifyOfPropertyChange(() => SelectedValueType);
            }
        }

        public bool IsOperatorElementVisible
        {
            get
            {
                return Model.CurrentPageNumber % 2 == 0;
            }
        }

        private decimal? _userValue;
        public decimal? UserValue
        {
            get { return _userValue; }
            set
            {
                _userValue = value;
                NotifyOfPropertyChange(() => UserValue);
            }
        }

        public bool IsAddOperatorSelected
        {
            get
            {
                return SelectedOperatorType != null && SelectedOperatorType == CalculatorOperatorType.Add;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Add;
                NotifyOfPropertyChange(() => IsAddOperatorSelected);
                NotifyOfPropertyChange(() => IsSubstractOperatorSelected);
                NotifyOfPropertyChange(() => IsMultiplyOperatorSelected);
                NotifyOfPropertyChange(() => IsDivideOperatorSelected);
            }
        }

        public bool IsSubstractOperatorSelected
        {
            get
            {
                return SelectedOperatorType != null && SelectedOperatorType == CalculatorOperatorType.Substract;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Substract;
                NotifyOfPropertyChange(() => IsAddOperatorSelected);
                NotifyOfPropertyChange(() => IsSubstractOperatorSelected);
                NotifyOfPropertyChange(() => IsMultiplyOperatorSelected);
                NotifyOfPropertyChange(() => IsDivideOperatorSelected);
            }
        }

        public bool IsMultiplyOperatorSelected
        {
            get
            {
                return SelectedOperatorType != null && SelectedOperatorType == CalculatorOperatorType.Multiply;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Multiply;
                NotifyOfPropertyChange(() => IsAddOperatorSelected);
                NotifyOfPropertyChange(() => IsSubstractOperatorSelected);
                NotifyOfPropertyChange(() => IsMultiplyOperatorSelected);
                NotifyOfPropertyChange(() => IsDivideOperatorSelected);
            }
        }

        public bool IsDivideOperatorSelected
        {
            get
            {
                return SelectedOperatorType != null && SelectedOperatorType == CalculatorOperatorType.Divide;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Divide;
                NotifyOfPropertyChange(() => IsAddOperatorSelected);
                NotifyOfPropertyChange(() => IsSubstractOperatorSelected);
                NotifyOfPropertyChange(() => IsMultiplyOperatorSelected);
                NotifyOfPropertyChange(() => IsDivideOperatorSelected);
            }
        }

        private BudgetCalculatorEquation _selectedEquation;
        public BudgetCalculatorEquation SelectedEquation
        {
            get { return _selectedEquation; }
            set
            {
                _selectedEquation = value;
                NotifyOfPropertyChange(() => SelectedEquation);
            }
        }

        private bool _isNextElement;
        public bool IsNextElement
        {
            get { return _isNextElement; }
            set
            {
                _isNextElement = value;
                if (!IsNextElement)
                {
                    SelectedOperatorType = CalculatorOperatorType.None;
                }
                NotifyOfPropertyChange(() => IsNextElement);
            }
        }

        public CalculatorOperatorType SelectedOperatorType { get; set; }

        protected override void OnModelLoaded()
        {
            base.OnModelLoaded();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (Model.CurrentItem == null)
            {
                SelectedValueType = null;
                UserValue = null;
                SelectedEquation = null;
                SelectedOperatorType = CalculatorOperatorType.None;
                IsNextElement = true;
                IsAddOperatorSelected = true;
            }
            else
            {
                var currentItem = Model.CurrentItem;
                IsNextElement = currentItem.OperatorType != CalculatorOperatorType.None;
                SelectedOperatorType = currentItem.OperatorType;
                SelectedValueType = ValueTypes.FirstOrDefault(x => x.Value == currentItem.ValueType);
                UserValue = currentItem.Value;
                SelectedEquation = Equations.FirstOrDefault(x => x.Id == currentItem.ForeignId);
            }

            NotifyOfPropertyChange(() => IsOperatorElementVisible);
            NotifyOfPropertyChange(() => IsAddOperatorSelected);
            NotifyOfPropertyChange(() => IsSubstractOperatorSelected);
            NotifyOfPropertyChange(() => IsMultiplyOperatorSelected);
            NotifyOfPropertyChange(() => IsDivideOperatorSelected);

            Title = string.Format("Składowe równania ({0}/{1})", Model.CurrentPageNumber, Model.TotalPagesNumber);
        }

        private void AddItem()
        {
            if (Model.CurrentItem == null)
            {
                CalculatorValueType valueType = CalculatorValueType.Operator;

                if (!IsOperatorElementVisible)
                {
                    valueType = (CalculatorValueType)SelectedValueType.Value;
                }
                Model.AddItem(valueType, SelectedOperatorType, UserValue, SelectedEquation);
            }
        }

        public override void MoveBack()
        {
            if (!Model.MoveBack())
            {
                BackPageName = typeof(BudgetEquationWizardStartViewModel).Name;
            }
            base.MoveBack();
            RefreshUI();
        }

        public override void MoveNext()
        {
            AddItem();
            if (IsNextElement)
            {                
                Model.MoveNext();
                BackPageName = Name;
                NextPageName = Name;
            }
            else
            {
                NextPageName = typeof(BudgetEquationWizardFinishViewModel).Name;
            }

            base.MoveNext();
            if (IsNextElement)
            {
                RefreshUI();
            }
        }
    }
}
