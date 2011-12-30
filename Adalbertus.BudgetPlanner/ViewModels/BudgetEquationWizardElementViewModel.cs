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
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(BudgetEquationWizardElementViewModel_PropertyChanged);
        }

        private bool _suppressEvents = false;

        private void BudgetEquationWizardElementViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedValueType":
                case "SelectedEquation":
                case "UserValue":
                case "SelectedCashFlow":
                case "SelectedIncome":
                case "SelectedSaving":
                case "IsLastElement":
                    if (_suppressEvents)
                    {
                        return;
                    }
                    if (Parent != null)
                    {
                        Parent.RefreshNavigationUI();
                    }
                    break;
            }
        }

        public IEnumerable<ComboItemVM<CalculatorValueType>> ValueTypes { get { return Model.ValueTypes; } }
        public IEnumerable<BudgetCalculatorEquation> Equations { get { return Model.Equations; } }
        public IEnumerable<CashFlow> CashFlows { get { return Model.CashFlows; } }
        public IEnumerable<CashFlowGroup> CashFlowGroups { get { return Model.CashFlowGroups; } }
        public IEnumerable<Income> Incomes { get { return Model.Incomes; } }
        public IEnumerable<Saving> Savings { get { return Model.Savings; } }

        private ComboItemVM<CalculatorValueType> _selectedValueType;
        public ComboItemVM<CalculatorValueType> SelectedValueType
        {
            get { return _selectedValueType; }
            set
            {
                _selectedValueType = value;
                if (Model != null && Model.CurrentItem != null)
                {
                    if (value != null)
                    {
                        Model.CurrentItem.ValueType = value.Value;
                    }
                    else
                    {
                        Model.CurrentItem.ValueType = CalculatorValueType.Operator;
                    }
                }
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

        public decimal? UserValue
        {
            get
            {
                if (Model.CurrentItem == null)
                {
                    return null;
                }
                return Model.CurrentItem.Value;
            }
            set
            {
                Model.CurrentItem.Value = value;
                NotifyOfPropertyChange(() => UserValue);
            }
        }

        public string UserText
        {
            get
            {
                if (Model.CurrentItem == null)
                {
                    return null;
                }
                return Model.CurrentItem.Text;
            }
            set
            {
                Model.CurrentItem.Text = value;
                NotifyOfPropertyChange(() => UserText);
            }
        }

        public bool IsNoneOperatorSelected
        {
            get
            {
                return SelectedOperatorType == CalculatorOperatorType.None;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.None;
                NotifyOfPropertyChange(() => IsNoneOperatorSelected);
                NotifyOfPropertyChange(() => IsAddOperatorSelected);
                NotifyOfPropertyChange(() => IsSubstractOperatorSelected);
                NotifyOfPropertyChange(() => IsMultiplyOperatorSelected);
                NotifyOfPropertyChange(() => IsDivideOperatorSelected);
            }
        }

        public bool IsAddOperatorSelected
        {
            get
            {
                return SelectedOperatorType == CalculatorOperatorType.Add;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Add;
                NotifyOfPropertyChange(() => IsNoneOperatorSelected);
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
                return SelectedOperatorType == CalculatorOperatorType.Substract;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Substract;
                NotifyOfPropertyChange(() => IsNoneOperatorSelected);
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
                return SelectedOperatorType == CalculatorOperatorType.Multiply;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Multiply;
                NotifyOfPropertyChange(() => IsNoneOperatorSelected);
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
                return SelectedOperatorType == CalculatorOperatorType.Divide;
            }
            set
            {
                SelectedOperatorType = CalculatorOperatorType.Divide;
                NotifyOfPropertyChange(() => IsNoneOperatorSelected);
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
                if (Model != null && Model.CurrentItem != null)
                {
                    if (Model.CurrentItem.ValueType == CalculatorValueType.CalculatorEquationValue)
                    {
                        if (value != null)
                        {
                            Model.CurrentItem.ForeignId = value.Id;
                        }
                    }
                    if (value == null)
                    {
                        Model.CurrentItem.ForeignId = 0;
                    }
                }
                NotifyOfPropertyChange(() => SelectedEquation);
            }
        }

        private CashFlow _selectedCashFlow;
        public CashFlow SelectedCashFlow
        {
            get { return _selectedCashFlow; }
            set
            {
                _selectedCashFlow = value;
                SetCurrentItemForeignId(value);
                NotifyOfPropertyChange(() => SelectedCashFlow);
            }
        }

        private CashFlowGroup _selectedCashFlowGroup;
        public CashFlowGroup SelectedCashFlowGroup
        {
            get { return _selectedCashFlowGroup; }
            set
            {
                _selectedCashFlowGroup = value;
                SetCurrentItemForeignId(value);
                NotifyOfPropertyChange(() => SelectedCashFlowGroup);
            }
        }

        private Income _selectedIncome;
        public Income SelectedIncome
        {
            get { return _selectedIncome; }
            set
            {
                _selectedIncome = value;
                SetCurrentItemForeignId(value);
                NotifyOfPropertyChange(() => SelectedIncome);
            }
        }

        private Saving _selectedSaving;
        public Saving SelectedSaving
        {
            get { return _selectedSaving; }
            set
            {
                _selectedSaving = value;
                SetCurrentItemForeignId(value);
                NotifyOfPropertyChange(() => SelectedSaving);
            }
        }

        private bool _isLastElement;
        public bool IsLastElement
        {
            get { return _isLastElement; }
            set
            {
                _isLastElement = value;
                NotifyOfPropertyChange(() => IsLastElement);
            }
        }

        public CalculatorOperatorType SelectedOperatorType
        {
            get
            {
                if (Model.CurrentItem == null)
                {
                    return CalculatorOperatorType.None;
                }
                return Model.CurrentItem.OperatorType;
            }
            set { Model.CurrentItem.OperatorType = value; }
        }

        protected override void OnModelLoaded()
        {
            base.OnModelLoaded();
            if (Model.Items.Any())
            {
                Model.CurrentItem = Model.Items.First();
            }
            else
            {
                Model.CreateDefaultCurrentItem();
            }
            SelectedOperatorType = CalculatorOperatorType.None;
            RefreshUI();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        private void SetToDefault()
        {
            SelectedValueType = ValueTypes.FirstOrDefault();
            SelectedCashFlow = CashFlows.FirstOrDefault();
            SelectedCashFlowGroup = CashFlowGroups.FirstOrDefault();
            SelectedIncome = null;
            SelectedSaving = null;
            UserValue = null;
            UserText = string.Empty;
            SelectedEquation = null;
            SelectedOperatorType = CalculatorOperatorType.None;

            var lastItem = Model.Items.LastOrDefault();
            bool isLastItemNotOperator = (lastItem != null) && (lastItem.ValueType != CalculatorValueType.Operator);
            IsLastElement = (Model.Items.Count > 2) && (isLastItemNotOperator);

            if (IsOperatorElementVisible)
            {
                IsAddOperatorSelected = true;
            }

        }

        private void RefreshUI()
        {
            _suppressEvents = true;
            if (Model.CurrentItem == null)
            {
                Model.CreateDefaultCurrentItem();
                SetToDefault();
            }
            else if (!Model.Items.Contains(Model.CurrentItem))
            {
                SetToDefault();
            }
            else
            {
                var currentItem = Model.CurrentItem;
                bool isOperatorType = currentItem.ValueType == CalculatorValueType.Operator;
                bool isNoneOperatorType = currentItem.OperatorType == CalculatorOperatorType.None;
                IsLastElement = isOperatorType && isNoneOperatorType;
                SelectedOperatorType = currentItem.OperatorType;
                SelectedValueType = ValueTypes.FirstOrDefault(x => x.Value == currentItem.ValueType);
                SelectedCashFlow = CashFlows.FirstOrDefault(x => x.Id == currentItem.ForeignId);
                SelectedCashFlowGroup = CashFlowGroups.FirstOrDefault(x => x.Id == currentItem.ForeignId);
                SelectedIncome = Incomes.FirstOrDefault(x => x.Id == currentItem.ForeignId);
                SelectedSaving = Savings.FirstOrDefault(x => x.Id == currentItem.ForeignId);
                UserValue = currentItem.Value;
                UserText = currentItem.Text;
                SelectedEquation = Equations.FirstOrDefault(x => x.Id == currentItem.ForeignId);
            }

            NotifyOfPropertyChange(() => IsNoneOperatorSelected);
            NotifyOfPropertyChange(() => IsOperatorElementVisible);
            NotifyOfPropertyChange(() => IsAddOperatorSelected);
            NotifyOfPropertyChange(() => IsSubstractOperatorSelected);
            NotifyOfPropertyChange(() => IsMultiplyOperatorSelected);
            NotifyOfPropertyChange(() => IsDivideOperatorSelected);

            Title = string.Format("Składowe równania ({0}/{1})", Model.CurrentPageNumber, Model.TotalPagesNumber);
            _suppressEvents = false;
        }

        private void AddItem()
        {
            if (!Model.Items.Contains(Model.CurrentItem))
            {
                CalculatorValueType valueType = CalculatorValueType.Operator;

                if (!IsOperatorElementVisible)
                {
                    valueType = (CalculatorValueType)SelectedValueType.Value;
                }

                int foreignId = GetForeignId();
                Model.AddItem(valueType, SelectedOperatorType, UserValue, foreignId);
            }
        }

        private int GetForeignId()
        {
            int foreignId = 0;
            if (SelectedValueType == null)
            {
                return foreignId;
            }
            switch (SelectedValueType.Value)
            {
                case CalculatorValueType.BudgetExpensesValueOfType:
                    foreignId = SelectedCashFlow.Id;
                    break;
                case CalculatorValueType.CalculatorEquationValue:
                    foreignId = SelectedEquation.Id;
                    break;
                case CalculatorValueType.BudgetPlanValueOfGroup:
                    foreignId = SelectedCashFlowGroup.Id;
                    break;
                case CalculatorValueType.BudgetPlanValueOfCategory:
                    foreignId = SelectedCashFlow.Id;
                    break;
                case CalculatorValueType.BudgetIncomesValueOfType:
                    foreignId = SelectedIncome.Id;
                    break;
                case CalculatorValueType.BudgetSavingsValueOfType:
                    foreignId = SelectedSaving.Id;
                    break;                        
            }

            return foreignId;
        }

        public override void OnActivating()
        {
            base.OnActivating();
        }

        public override void OnActivated()
        {
            base.OnActivated();
            RefreshUI();
        }

        public override void MoveBack()
        {
            DeleteElementsIfNecessary();
            if (!Model.MoveBack())
            {
                BackPageName = typeof(BudgetEquationWizardStartViewModel).Name;
            }
            base.MoveBack();
        }

        public override void MoveNext()
        {
            if (!IsLastElement)
            {
                AddItem();
                Model.MoveNext();
                BackPageName = Name;
                NextPageName = Name;
            }
            else
            {
                DeleteElementsIfNecessary();
                NextPageName = typeof(BudgetEquationWizardFinishViewModel).Name;
            }

            base.MoveNext();
        }

        public override void Finish()
        {
            AddItem();
            DeleteElementsIfNecessary();
        }

        protected override bool ValidateMoveNext()
        {
            bool canMove = base.ValidateMoveNext();
            if (canMove)
            {
                canMove = ValidateMovement();
            }

            return canMove;
        }

        protected override bool ValidateMoveBack()
        {
            bool canMove = base.ValidateMoveBack();
            if (canMove)
            {
                canMove = ValidateMovement();
            }

            return canMove;
        }

        protected override bool ValidateFinish()
        {
            bool canFinish = base.ValidateFinish();

            if (IsOperatorElementVisible && Model.CurrentItem != null && Model.CurrentItem.OperatorType != CalculatorOperatorType.None)
            {
                if (Model.CurrentPageNumber < Model.Items.Count)
                {
                    return true;
                }
                return IsLastElement;
            }

            if (!ValidateMovement())
            {
                return false;
            }
            return canFinish;
        }

        private bool ValidateMovement()
        {
            if (SelectedOperatorType != CalculatorOperatorType.None)
            {
                return true;
            }
            if (SelectedValueType == null)
            {
                return false;
            }

            switch (SelectedValueType.Value)
            {
                case CalculatorValueType.CalculatorEquationValue:
                    return SelectedEquation != null;
                case CalculatorValueType.UserValue:
                    return UserValue.HasValue;
                case CalculatorValueType.BudgetExpensesValueOfType:
                case CalculatorValueType.BudgetPlanValueOfCategory:
                    return SelectedCashFlow != null;
                case CalculatorValueType.BudgetPlanValueOfGroup:
                    return SelectedCashFlowGroup != null;
                case CalculatorValueType.BudgetIncomesValueOfType:
                    return SelectedIncome != null;                    
                case CalculatorValueType.BudgetSavingsValueOfType:
                    return SelectedSaving != null;

            }

            return true;
        }

        private void DeleteElementsIfNecessary()
        {
            if (Model.CurrentItem == null)
            {
                return;
            }

            bool isOperatorType = Model.CurrentItem.ValueType == CalculatorValueType.Operator;
            bool hasElements = Model.CurrentPageNumber <= Model.Items.Count;

            if (IsLastElement && isOperatorType && hasElements)
            {
                var itemsToRemove = Model.Items.Skip(Model.CurrentPageNumber - 1).ToList();
                Model.InnerEquationList.RemoveRange(Model.CurrentPageNumber - 1, itemsToRemove.Count);
                Model.Items.RemoveRange(itemsToRemove);
            }
            var lastElement = Model.Items.LastOrDefault();
            if (lastElement != null && lastElement.ValueType == CalculatorValueType.Operator)
            {
                Model.Items.Remove(lastElement);
            }
        }

        private void SetCurrentItemForeignId(Entity entity)
        {
            if (Model != null && Model.CurrentItem != null)
            {
                if (entity == null)
                {
                    Model.CurrentItem.ForeignId = 0;
                }
                else
                {
                    Model.CurrentItem.ForeignId = entity.Id;
                }
            }
        }
    }
}
