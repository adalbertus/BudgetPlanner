using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpenseVM : WrappedViewModel<Expense>
    {
        #region Private fields
        #endregion Private fields

        #region Constructors
        public ExpenseVM(Expense expense, int? currentBudgetId = null)
            : base(expense)
        {
            CurrentBudgetId = currentBudgetId;
            StorePreviousInstance();
        }
        #endregion Constructors

        #region Properties
        public Expense PreviousExpense { get; set; }

        public int? CurrentBudgetId { get; private set; }
        public bool IsCurrentBudget
        {
            get
            {
                return WrappedItem.BudgetId == CurrentBudgetId.GetValueOrDefault(-1);
            }
        }

        public string GroupName { get { return WrappedItem.GroupName; } }

        public int BudgetId { get { return WrappedItem.BudgetId; } }
        public DateTime BudgetDateFrom { get { return WrappedItem.Budget.DateFrom; } }
        public DateTime BudgetDateTo { get { return WrappedItem.Budget.DateTo; } }

        public CashFlow Flow
        {
            get { return WrappedItem.Flow; }
            set
            {
                StorePreviousInstance();
                WrappedItem.Flow = value;
                NotifyOfPropertyChange(() => Flow);
            }
        }

        public DateTime Date
        {
            get { return WrappedItem.Date; }
            set
            {
                StorePreviousInstance();
                WrappedItem.Date = value;
                NotifyOfPropertyChange(() => Date);
            }
        }

        public decimal Value
        {
            get { return WrappedItem.Value; }
            set
            {
                StorePreviousInstance();
                WrappedItem.Value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        public string Description
        {
            get { return WrappedItem.Description; }
            set
            {
                StorePreviousInstance();
                WrappedItem.Description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }
        #endregion Properties

        #region Public methods
        public void UndoChanges()
        {
            WrappedItem.Date = PreviousExpense.Date;
            WrappedItem.Flow = PreviousExpense.Flow;
            WrappedItem.Value = PreviousExpense.Value;
            WrappedItem.Description = PreviousExpense.Description;

            NotifyOfPropertyChange(() => Date);
            NotifyOfPropertyChange(() => Flow);
            NotifyOfPropertyChange(() => Value);
            NotifyOfPropertyChange(() => Description);
        }
        #endregion Public methods

        #region Private methods
        private void StorePreviousInstance()
        {
            PreviousExpense = WrappedItem.Clone();
        }
        #endregion Private methods
    }
}
