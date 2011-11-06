using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
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
        }

        protected override void Initialize()
        {
            base.Initialize();
            _expenseValues = new BindableCollectionExt<decimal>();
            _expenseValues.CollectionChanged += (s, e) =>
            {
                NotifyOfPropertyChange(() => ExpenseTotalValue);
                NotifyOfPropertyChange(() => CanAddExpense);
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
        public BindableCollectionExt<Expense> BudgetExpenses { get; private set; }

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
            BudgetExpenses = budget.Expenses;
            BudgetExpenses.PropertyChanged += ExpensesPropertyChanged;
            BudgetExpenses.CollectionChanged += ExpensesCollectionChanged;
            NotifyOfPropertyChange(() => BudgetExpenses);

            LoadCashFlows();
            LoadExpenses();
        }

        public void AttachEvents()
        {
        }

        public void DeatachEvents()
        {
            //DeatachEvents(_budget);
            BudgetExpenses.CollectionChanged -= ExpensesCollectionChanged;
            BudgetExpenses.PropertyChanged -= ExpensesPropertyChanged;
        }

        //TODO: Need refactoring - put into cache...
        private void LoadCashFlows()
        {
            ExpensesGridCashFlows.IsNotifying = false;
            ExpensesGridCashFlows.Clear();
            CashFlows.Clear();
            var cashFlowList = Database.Query<CashFlow, CashFlowGroup, Saving>(PetaPoco.Sql.Builder
                .Select("*")
                .From("CashFlow")
                .InnerJoin("CashFlowGroup")
                .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                .LeftJoin("Saving")
                .On("Saving.CashFlowId = CashFlow.Id")
                .OrderBy("CashFlow.Name ASC")).ToList();
            cashFlowList.ForEach(x =>
            {
                if (x.Saving.IsTransient())
                {
                    x.Saving = null;
                }
            });
            if (cashFlowList != null)
            {
                cashFlowList.ForEach(x => ExpensesGridCashFlows.Add(x));
                cashFlowList.ForEach(x => CashFlows.Add(x));
            }
            ExpensesGridCashFlows.IsNotifying = true;

            NotifyOfPropertyChange(() => ExpensesGridCashFlows);
        }

        private void LoadExpenses()
        {
            var sql = PetaPoco.Sql.Builder
                    .Select("Expense.*, Budget.*, CashFlow.*")
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
            BudgetExpenses.IsNotifying = false;
            BudgetExpenses.Clear();
            BudgetExpenses.AddRange(expenses);
            BudgetExpenses.IsNotifying = true;
            BudgetExpenses.Refresh();
        }

        //private void AttachEvents(Budget budget)
        //{
        //    if (budget == null)
        //    {
        //        return;
        //    }
        //    var expenes = budget.Expenses as BindableCollectionExt<Expense>;
        //    expenes.PropertyChanged += ExpensesPropertyChanged;
        //    expenes.CollectionChanged += ExpensesCollectionChanged;
        //}

        //private void DeatachEvents(Budget budget)
        //{
        //    if (budget == null)
        //    {
        //        return;
        //    }
        //    var expenes = budget.Expenses as BindableCollectionExt<Expense>;
        //    expenes.PropertyChanged -= ExpensesPropertyChanged;
        //    expenes.CollectionChanged -= ExpensesCollectionChanged;
        //}

        private void ExpensesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save(sender as Expense);
        }

        private void ExpensesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    Delete(e.OldItems[0] as Expense);
                    break;
            }
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
            PublishRefreshRequest();
        }

        private void Delete(Expense expense)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete<SavingValue>("WHERE ExpenseId = @0", expense.Id);
                Database.Delete(expense);

                tx.Complete();
            }
            PublishRefreshRequest();
        }

        public void AddExpenseValueToCalculator()
        {
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
            IsExpenseDescriptionFocused = true;
        }

        public void MoveToExpenseDescription()
        {
            IsExpenseDescriptionFocused = false;
            IsExpenseDescriptionFocused = true;
        }

        public void AddAndMoveToExpenseValue()
        {
            AddExpense();
            MoveToExpenseValue();
        }


        #endregion
    }
}
