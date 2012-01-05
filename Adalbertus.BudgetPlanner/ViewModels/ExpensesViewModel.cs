using System;
using System.Collections.Generic;
using System.Linq;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using Caliburn.Micro;
using System.Linq.Expressions;
using System.Windows.Data;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesViewModel : BaseViewModel, IHandle<ExpensesFilterVM>
    {
        public ExpensesViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            _filteredBudgetExpenses = new List<Expense>();
            ExpensesGridCashFlows = new BindableCollectionExt<CashFlow>();
            CashFlows = new BindableCollectionExt<CashFlow>();
            Filter = new ExpensesFilterVM(EventAggregator);
            IsFilteringEnabled = true;
            ExpensesFilteringViewModel = IoC.Get<ExpensesFilteringViewModel>();
        }

        protected override void Initialize()
        {
            base.Initialize();
            SelectedExpenseDate = DateTime.Now;
        }

        public ExpensesFilteringViewModel ExpensesFilteringViewModel { get; set; }

        // Workaround - need ExpensesGridCashFlows and CashFlows because without spliting
        // it was refresed only in DataGrid in ExpensesView
        public BindableCollectionExt<CashFlow> ExpensesGridCashFlows { get; private set; }
        public BindableCollectionExt<CashFlow> CashFlows { get; private set; }
        //public int BudgetId { get; private set; }

        #region Budget plan expenses
        private Budget _budget;
        public Budget Budget
        {
            get { return _budget; }
            set
            {
                _budget = value;
                NotifyOfPropertyChange(() => Budget);
            }
        }

        private BindableCollectionExt<Expense> _budgetExpenses;
        private List<Expense> _filteredBudgetExpenses;
        public IEnumerable<Expense> BudgetExpenses
        {
            get
            {
                return _filteredBudgetExpenses;
            }
        }

        public int BudgetExpensesTotalCount
        {
            get { return _budgetExpenses.Count; }
        }

        public int BudgetExpensesFilteredCount
        {
            get { return _filteredBudgetExpenses.Count; }
        }

        public decimal TotalExpensesValue
        {
            get
            {
                return _filteredBudgetExpenses.Sum(x => x.Value);
            }
        }

        private bool _isExpensesFiltered;
        public bool IsExpensesFiltered
        {
            get { return _isExpensesFiltered; }
            set
            {
                _isExpensesFiltered = value;
                NotifyOfPropertyChange(() => IsExpensesFiltered);
            }
        }

        private bool _isFilteringEnabled;
        public bool IsFilteringEnabled
        {
            get { return _isFilteringEnabled; }
            set
            {
                _isFilteringEnabled = value;
                NotifyOfPropertyChange(() => IsFilteringEnabled);
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

        public void RefreshUI()
        {
            Diagnostics.Start();
            FilterBudgetExpenses();
            NotifyOfPropertyChange(() => BudgetExpenses);
            NotifyOfPropertyChange(() => TotalExpensesValue);
            Diagnostics.Stop();
        }

        public void LoadData(Budget budget)
        {
            Diagnostics.Start();
            Budget = budget;
            _budgetExpenses = budget.Expenses;
            _filteredBudgetExpenses = _budgetExpenses.ToList();

            LoadCashFlows();
            LoadExpenses();
            ExpensesFilteringViewModel.LoadData(budget);

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

            RefreshUI();
            Diagnostics.Stop();
        }

        private void LoadCashFlows()
        {
            Diagnostics.Start();
            ExpensesGridCashFlows.IsNotifying = false;
            ExpensesGridCashFlows.Clear();
            CashFlows.Clear();
            var cashFlowList = CachedService.GetAllCashFlows();
            cashFlowList.ForEach(x => ExpensesGridCashFlows.Add(x));
            cashFlowList.ForEach(x => CashFlows.Add(x));

            ExpensesGridCashFlows.IsNotifying = true;
            SelectedExpenseCashFlow = CashFlows.FirstOrDefault();

            NotifyOfPropertyChange(() => ExpensesGridCashFlows);
            Diagnostics.Stop();
        }

        private void LoadExpenses()
        {
            Diagnostics.Start();
            var sql = PetaPoco.Sql.Builder
                    .Select("*")
                    .From("Expense")
                    .InnerJoin("Budget")
                    .On("Budget.Id = Expense.BudgetId")
                    .InnerJoin("CashFlow")
                    .On("CashFlow.Id = Expense.CashFlowId")
                    .InnerJoin("CashFlowGroup")
                    .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                    .Where("Expense.BudgetId = @0", Budget.Id);

            var expenses = Database.Query<Expense, Budget, CashFlow, CashFlowGroup>(sql).ToList();
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
            Diagnostics.Stop();
        }

        public void RemoveExpense(Expense expense)
        {
            Diagnostics.Start();
            _budgetExpenses.Remove(expense);
            RefreshUI();
            Diagnostics.Stop();
        }

        public bool CanAddExpense
        {
            get
            {
                bool isCashFlowSelected = SelectedExpenseCashFlow != null;
                bool hasPositiveExpenseValue = ExpenseValue > 0;
                bool isDateInBudgetDange = (SelectedExpenseDate >= Budget.DateFrom && SelectedExpenseDate <= Budget.DateTo);
                return (isCashFlowSelected && hasPositiveExpenseValue && isDateInBudgetDange);
            }
        }

        private Expense _scrollToExpense;
        public Expense ScrollToExpense
        {
            get { return _scrollToExpense; }
            set
            {
                _scrollToExpense = value;
                NotifyOfPropertyChange(() => ScrollToExpense);
            }
        }


        public void AddExpense()
        {
            Diagnostics.Start();
            var expense = Budget.AddExpense(SelectedExpenseCashFlow, ExpenseValue, ExpenseDescription, SelectedExpenseDate);

            Save(expense);
            FilterBudgetExpenses();
            NotifyOfPropertyChange(() => BudgetExpenses);
            ExpenseValue = 0;
            ExpenseDescription = string.Empty;

            ScrollToExpense = expense;

            Diagnostics.Stop();
        }

        private void Save(Expense expense)
        {
            Diagnostics.Start();
            Diagnostics.Start("Database operations");
            using (var tx = Database.GetTransaction())
            {
                int savingsDeletedCounter = 0;
                Database.Save(expense);
                if (expense.Flow.Saving == null)
                {
                    savingsDeletedCounter = Database.Delete<SavingValue>("WHERE ExpenseId = @0", expense.Id);
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
                        savingsDeletedCounter = 
                            Database.Delete<SavingValue>("WHERE ExpenseId = @0 AND SavingId <> @1", expense.Id, expense.SavingValue.SavingId);
                    }
                }

                tx.Complete();                
                CachedService.Clear(CachedServiceKeys.AllSavings);
            }
            Diagnostics.Stop();

            NotifyOfPropertyChange(() => TotalExpensesValue);
            PublishRefreshRequest(expense);
            Diagnostics.Stop();
        }

        private void Delete(Expense expense)
        {
            Diagnostics.Start();
            using (var tx = Database.GetTransaction())
            {
                Database.Delete<SavingValue>("WHERE ExpenseId = @0", expense.Id);
                Database.Delete(expense);

                tx.Complete();
            }
            PublishRefreshRequest(expense);
            Diagnostics.Stop();
        }

        public void MoveToExpenseValue()
        {
            Diagnostics.Start();
            IsExpenseValueFocused = false;
            IsExpenseValueFocused = true;
            Diagnostics.Stop();
        }

        public void AddAndMoveToExpenseValue()
        {
            Diagnostics.Start();
            if (CanAddExpense)
            {
                AddExpense();
            }
            MoveToExpenseValue();
            Diagnostics.Stop();
        }


        #endregion

        public ExpensesFilterVM Filter { get; set; }

        private void FilterBudgetExpenses()
        {
            Diagnostics.Start();

            if (IsFilteringEnabled)
            {
                var predicate = PredicateBuilder.True<Expense>();

                if (Filter.CashFlow != null || Filter.CashFlowGroup != null)
                {
                    var flowPredicate = PredicateBuilder.False<Expense>();
                    if (Filter.CashFlowGroup != null)
                    {
                        flowPredicate = flowPredicate.And(p => Filter.CashFlowGroup.Id == p.CashFlowGroupId);
                    }
                    if (Filter.CashFlow != null)
                    {
                        flowPredicate = flowPredicate.And(p => Filter.CashFlow.Id == p.Id);
                    }
                }

                if (Filter.CashFlowGroup != null && !Filter.CashFlowGroup.IsTransient())
                {
                    predicate = predicate.And(p => Filter.CashFlowGroup.Id == p.CashFlowGroupId);
                }

                if (Filter.CashFlow != null && !Filter.CashFlow.IsTransient())
                {
                    predicate = predicate.And(p => Filter.CashFlow.Id == p.CashFlowId);
                }


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
                    predicate = predicate.And(p => p.Description.Contains(Filter.Description, false));
                }
                _filteredBudgetExpenses = _budgetExpenses.AsQueryable().Where(predicate).ToList();
            }
            else
            {
                _filteredBudgetExpenses = _budgetExpenses.ToList();
            }


            if (_budgetExpenses.Any())
            {
                IsExpensesFiltered = _filteredBudgetExpenses.Count != _budgetExpenses.Count;
            }
            else
            {
                IsExpensesFiltered = false;
            }
            NotifyOfPropertyChange(() => BudgetExpensesTotalCount);
            NotifyOfPropertyChange(() => BudgetExpensesFilteredCount);
            Diagnostics.Stop();
        }

        #region IHandle<ExpensesFilterVM> Members

        public void Handle(ExpensesFilterVM message)
        {
            Diagnostics.Start();
            if (IsFilteringEnabled)
            {
                if (_budgetExpenses != null)
                {
                    RefreshUI();
                }
            }
            Diagnostics.Stop();
        }

        #endregion
    }
}
