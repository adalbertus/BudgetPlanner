using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Core;
using System.Collections.Specialized;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public partial class BudgetPlanViewModel : BaseViewModel
    {
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

        #region Budget plan expenses
        public BindableCollectionExt<Expense> BudgetExpenses
        {
            get { return _budget.Expenses; }
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

        private void LoadExpenses()
        {
            var sql = PetaPoco.Sql.Builder
                    .Select("Expense.*, Budget.*, CashFlow.*")
                    .From("Expense")
                    .InnerJoin("Budget")
                    .On("Budget.Id = Expense.BudgetId")
                    .InnerJoin("CashFlow")
                    .On("CashFlow.Id = Expense.CashFlowId")
                    .Where("Expense.BudgetId = @0", _budget.Id);

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
            expenses.ForEach(e => _budget.Expenses.Add(e));
        }

        private void AttachToExpenses()
        {
            BudgetExpenses.PropertyChanged += BudgetPropertyChanged;
            BudgetExpenses.CollectionChanged += BudgetExpenses_CollectionChanged;
        }

        
        private void DetachFromExpenses()
        {
            BudgetExpenses.PropertyChanged -= BudgetPropertyChanged;
            BudgetExpenses.CollectionChanged -= BudgetExpenses_CollectionChanged;
        }

        private void BudgetExpenses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
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

            var expense = _budget.AddExpense(SelectedExpenseCashFlow, expenseValue, ExpenseDescription, SelectedExpenseDate);

            SaveExpense(expense);

            ExpenseValues.Clear();
            ExpenseValue = 0;
            ExpenseDescription = string.Empty;
        }

        private void SaveExpense(Expense expense)
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
                //NotifyOfPropertyChange(() => BudgetPlanList);
                BudgetPlanList.Refresh();
            }
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
