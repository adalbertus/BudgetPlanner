﻿using System;
using System.Collections.Generic;
using System.Linq;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesViewModel : BaseViewModel
    {
        public ExpensesViewModel(IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(database, configuration, cashedService, eventAggregator)
        {
            ExpensesGridCashFlows = new BindableCollectionExt<CashFlow>();
            CashFlows = new BindableCollectionExt<CashFlow>();
            Filter = new ExpensesFilterVM();
        }

        protected override void Initialize()
        {
            base.Initialize();
            _expenseValues = new BindableCollectionExt<decimal>();
            _expenseValues.CollectionChanged += (s, e) =>
            {
                NotifyOfPropertyChange(() => ExpenseTotalValue);
                NotifyOfPropertyChange(() => CanAddExpense);
                NotifyOfPropertyChange(() => IsCalculatorListBoxVisible);
            };
            SelectedExpenseDate = DateTime.Now;
        }

        // Workaround - need ExpensesGridCashFlows and CashFlows because without spliting
        // it was refresed only in DataGrid in ExpensesView
        public BindableCollectionExt<CashFlow> ExpensesGridCashFlows { get; private set; }
        public BindableCollectionExt<CashFlow> CashFlows { get; private set; }
        //public int BudgetId { get; private set; }

        #region Budget plan expenses
        public Budget Budget { get; set; }
        private BindableCollectionExt<Expense> _budgetExpenses;
        public IEnumerable<Expense> BudgetExpenses
        {
            get
            {
                return FilteredBudgetExpenses();
            }
        }

        private bool _isFilterEnabled;
        public bool IsFilterEnabled
        {
            get { return _isFilterEnabled; }
            set
            {
                _isFilterEnabled = value;
                NotifyOfPropertyChange(() => IsFilterEnabled);
            }
        }


        private CashFlow _selectedExpenseCashFlow;
        public CashFlow SelectedExpenseCashFlow
        {
            get
            {
                return _selectedExpenseCashFlow;
            }
            set
            {
                _selectedExpenseCashFlow = value;
                NotifyOfPropertyChange(() => SelectedExpenseCashFlow);
                NotifyOfPropertyChange(() => CanAddExpense);
            }
        }

        private DateTime _selectedExpenseDate;
        public DateTime SelectedExpenseDate
        {
            get
            {
                return _selectedExpenseDate;
            }
            set
            {
                _selectedExpenseDate = value;
                NotifyOfPropertyChange(() => SelectedExpenseDate);
                NotifyOfPropertyChange(() => CanAddExpense);
            }
        }

        private decimal _expenseValue;
        public decimal ExpenseValue
        {
            get
            {
                return _expenseValue;
            }
            set
            {
                _expenseValue = value;
                NotifyOfPropertyChange(() => ExpenseValue);
                NotifyOfPropertyChange(() => CanAddExpense);
            }
        }

        private string _expenseDescription;
        public string ExpenseDescription
        {
            get { return _expenseDescription; }
            set
            {
                _expenseDescription = value;
                NotifyOfPropertyChange(() => ExpenseDescription);
            }
        }

        public decimal ExpenseTotalValue
        {
            get
            {
                return ExpenseValues.Sum();
            }
        }

        private BindableCollectionExt<decimal> _expenseValues;
        public BindableCollectionExt<decimal> ExpenseValues
        {
            get
            {
                return _expenseValues;
            }
        }

        public bool IsCalculatorListBoxVisible
        {
            get { return ExpenseValues.Any(); }
        }

        #region Focus properties
        private bool _isExpenseDescriptionFocused;

        public bool IsExpenseDescriptionFocused
        {
            get { return _isExpenseDescriptionFocused; }
            set
            {
                _isExpenseDescriptionFocused = value;
                NotifyOfPropertyChange(() => IsExpenseDescriptionFocused);
            }
        }

        private bool _isExpenseValueFocused;
        public bool IsExpenseValueFocused
        {
            get
            {
                return _isExpenseValueFocused;
            }
            set
            {
                _isExpenseValueFocused = value;
                NotifyOfPropertyChange(() => IsExpenseValueFocused);
            }
        }
        #endregion

        public void LoadData(Budget budget)
        {
            Budget = budget;
            _budgetExpenses = budget.Expenses;

            LoadCashFlows();
            LoadExpenses();
            FillFilterData();

            _budgetExpenses.PropertyChanged += (s, e) => { Save(s as Expense); };
            _budgetExpenses.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        Delete(e.OldItems[0] as Expense);
                        break;
                }
            };

            NotifyOfPropertyChange(() => BudgetExpenses);
        }

        private void FillFilterData()
        {
            Filter.IsNotifying = false;
            var filterCashFlows = CachedService.GetAllCashFlows().Select(x =>
                new ExpensesFilterCashFlowVM
                {
                    CashFlowId = x.Id,
                    Name = x.Name,
                }
            ).ToList();
            
            Filter.CashFlows.IsNotifying = false;
            Filter.DateFrom              = Budget.DateFrom;
            Filter.DateTo                = Budget.DateTo;

            Filter.CashFlows.Clear();
            Filter.CashFlows.AddRange(filterCashFlows);
            //Filter.PropertyChanged           += (s, e) => { NotifyOfPropertyChange(() => BudgetExpenses); };
            Filter.CashFlows.PropertyChanged += (s, e) => { NotifyOfPropertyChange(() => BudgetExpenses); };
            Filter.IsNotifying                = true;
            Filter.CashFlows.IsNotifying      = true;
            Filter.CashFlows.Refresh();
        }

        private void LoadCashFlows()
        {
            ExpensesGridCashFlows.IsNotifying = false;
            ExpensesGridCashFlows.Clear();
            CashFlows.Clear();
            var cashFlowList = CachedService.GetAllCashFlows();
            cashFlowList.ForEach(x => ExpensesGridCashFlows.Add(x));
            cashFlowList.ForEach(x => CashFlows.Add(x));

            ExpensesGridCashFlows.IsNotifying = true;
            SelectedExpenseCashFlow = CashFlows.FirstOrDefault();

            NotifyOfPropertyChange(() => ExpensesGridCashFlows);
        }

        private void LoadExpenses()
        {
            var sql = PetaPoco.Sql.Builder
                    .Select("*")
                    .From("Expense")
                    .InnerJoin("Budget")
                    .On("Budget.Id = Expense.BudgetId")
                    .InnerJoin("CashFlow")
                    .On("CashFlow.Id = Expense.CashFlowId")
                    .Where("Expense.BudgetId = @0", Budget.Id);

            var expenses = Database.Query<Expense, Budget, CashFlow>(sql).ToList();
            expenses.ForEach(x =>
            {
                x.Flow.Saving = Database.SingleOrDefault<Saving>("WHERE CashFlowId = @0", x.CashFlowId);
                x.SavingValue = Database.SingleOrDefault<SavingValue>("WHERE ExpenseId = @0", x.Id);
                if (x.SavingValue != null)
                {
                    x.SavingValue.Expense = x;
                    x.SavingValue.Saving = x.Flow.Saving;
                    x.SavingValue.Budget = x.Budget;
                }
            });
            _budgetExpenses.IsNotifying = false;
            _budgetExpenses.Clear();
            _budgetExpenses.AddRange(expenses);
            _budgetExpenses.IsNotifying = true;
            _budgetExpenses.Refresh();
        }

        public void RemoveExpense(Expense expense)
        {
            _budgetExpenses.Remove(expense);
            NotifyOfPropertyChange(() => BudgetExpenses);
        }

        public bool CanAddExpense
        {
            get
            {
                if (ExpenseValues.Any())
                {
                    return (SelectedExpenseCashFlow != null) && (ExpenseTotalValue > 0);
                }
                return (SelectedExpenseCashFlow != null) && (ExpenseValue > 0);
            }
        }

        public void AddExpense()
        {
            decimal expenseValue = ExpenseValue;
            if (ExpenseValues.Any())
            {
                expenseValue = ExpenseTotalValue;
            }

            var expense = Budget.AddExpense(SelectedExpenseCashFlow, expenseValue, ExpenseDescription, SelectedExpenseDate);

            Save(expense);

            ExpenseValues.Clear();
            ExpenseValue = 0;
            ExpenseDescription = string.Empty;
        }

        private void Save(Expense expense)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(expense);
                if (expense.Flow.Saving == null)
                {
                    Database.Delete<SavingValue>("WHERE ExpenseId = @0", expense.Id);
                }

                if (expense.SavingValue != null)
                {
                    expense.SavingValue.UpdateDescription();
                    Database.Save(expense.SavingValue);
                    int savingValuesCount = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                        .Select("COUNT(*)")
                        .From("SavingValue")
                        .Where("ExpenseId = @0", expense.Id));
                    if (savingValuesCount > 1)
                    {
                        Database.Delete<SavingValue>("WHERE ExpenseId = @0 AND SavingId <> @1",
                            expense.Id, expense.SavingValue.SavingId);
                    }
                }

                tx.Complete();
            }
            NotifyOfPropertyChange(() => BudgetExpenses);
            PublishRefreshRequest(expense);
        }

        private void Delete(Expense expense)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete<SavingValue>("WHERE ExpenseId = @0", expense.Id);
                Database.Delete(expense);

                tx.Complete();
            }
            PublishRefreshRequest(expense);
        }

        public void AddExpenseValueToCalculator()
        {
            if (ExpenseValue == 0)
            {
                return;
            }
            ExpenseValues.Add(ExpenseValue);
            ExpenseValue = 0;
        }

        public void RemoveExpenseValueFromCalculator(decimal value)
        {
            ExpenseValues.Remove(value);
        }

        public void MoveToExpenseValue()
        {
            IsExpenseValueFocused = false;
            IsExpenseValueFocused = true;
        }

        public void AddAndMoveToExpenseValue()
        {
            if (CanAddExpense)
            {
                AddExpense();
            }
            MoveToExpenseValue();
        }


        #endregion
        public ExpensesFilterVM Filter { get; set; }

        private IEnumerable<Expense> FilteredBudgetExpenses()
        {
            var selectedCashFlows = Filter.CashFlows.Where(x => x.IsSelected).ToList();

            var predicate = PredicateBuilder.False<Expense>();
            predicate = predicate.Or(p => selectedCashFlows.Any(x => p.CashFlowId == x.CashFlowId));
            predicate = predicate.And(p => p.Date >= Filter.DateFrom && p.Date <= Filter.DateTo);
            if (Filter.ValueFrom.HasValue)
            {
                predicate = predicate.And(p => p.Value >= Filter.ValueFrom.Value);
            }
            if (Filter.ValueTo.HasValue)
            {
                predicate = predicate.And(p => p.Value <= Filter.ValueTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(Filter.Description))
            {
                predicate = predicate.And(p => p.Description.IsEqual(Filter.Description, false));
            }

            var expenses = _budgetExpenses.AsQueryable().Where(predicate).ToList();
            IsFilterEnabled = expenses.Count != _budgetExpenses.Count;
            return expenses;
        }

        public void SelectAllFilterCashFlows()
        {
            Filter.CashFlows.IsNotifying = false;
            Filter.CashFlows.Where(x => !x.IsSelected).ForEach(x => x.IsSelected = true);
            Filter.CashFlows.IsNotifying = true;
            Filter.CashFlows.Refresh();
            NotifyOfPropertyChange(() => BudgetExpenses);
        }

        public void DeselectAllFilterCashFlows()
        {
            Filter.CashFlows.IsNotifying = false;
            Filter.CashFlows.Where(x => x.IsSelected).ForEach(x => x.IsSelected = false);
            Filter.CashFlows.IsNotifying = true;
            Filter.CashFlows.Refresh();
            NotifyOfPropertyChange(() => BudgetExpenses);
        }
    }
}
