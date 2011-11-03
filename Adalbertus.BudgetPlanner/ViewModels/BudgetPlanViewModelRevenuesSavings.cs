using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public partial class BudgetPlanViewModel : BaseViewModel
    {
        #region Budget revenues: Savings

        #region Attaching events
        private void AttachToSavingValues()
        {
            var savingValues = (_budget.SavingValues as BindableCollectionExt<SavingValue>);
            savingValues.PropertyChanged += BudgetPropertyChanged;
        }
        #endregion
        
        #region Detaching events
        private void DetachFromSavingValues()
        {
            var savingValues = (_budget.SavingValues as BindableCollectionExt<SavingValue>);
            savingValues.PropertyChanged -= BudgetPropertyChanged;
        }
        #endregion

        private BindableCollectionExt<Saving> _availableSavings;
        public BindableCollectionExt<Saving> AvailableSavings
        {
            get { return _availableSavings; }
            set { _availableSavings = value; }
        }

        private Saving _selectedAvailableSaving;
        public Saving SelectedAvailableSaving
        {
            get { return _selectedAvailableSaving; }
            set
            {
                _selectedAvailableSaving = value;
                NotifyOfPropertyChange(() => SelectedAvailableSaving);
                NotifyOfPropertyChange(() => CanAddSavingValue);
            }
        }

        private DateTime _savingValueDate;
        public DateTime SavingValueDate
        {
            get { return _savingValueDate; }
            set
            {
                _savingValueDate = value;
                NotifyOfPropertyChange(() => SavingValueDate);
                NotifyOfPropertyChange(() => CanAddSavingValue);
            }
        }

        private decimal _savingValueValue;
        public decimal SavingValueValue
        {
            get { return _savingValueValue; }
            set
            {
                _savingValueValue = value;
                NotifyOfPropertyChange(() => SavingValueValue);
                NotifyOfPropertyChange(() => CanAddSavingValue);
            }
        }

        private string _savingValueDescription;
        public string SavingValueDescription
        {
            get { return _savingValueDescription; }
            set
            {
                _savingValueDescription = value;
                NotifyOfPropertyChange(() => SavingValueDescription);
            }
        }

        public decimal SumOfRevenueSavings
        {
            get
            {
                return _budget.SavingValues.Where(x => x.Expense != null).Sum(x => x.BudgetValue);
            }
        }

        public bool CanAddSavingValue
        {
            get
            {
                bool isSavingSelected = SelectedAvailableSaving != null;
                bool isSavingValueNotZero = SavingValueValue > 0;

                return isSavingSelected && isSavingValueNotZero;
            }
        }

        public IList<SavingValue> BudgetSavingValues
        {
            get { return _budget.SavingValues; }
        }

        private void LoadAvailableSavings()
        {
            var savingValues = Database.Query<SavingValue, Saving, Budget, Expense>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("SavingValue")
                    .InnerJoin("Saving")
                    .On("Saving.Id = SavingValue.SavingId")
                    .InnerJoin("Budget")
                    .On("Budget.Id = SavingValue.BudgetId")
                    .LeftJoin("Expense")
                    .On("Expense.Id = SavingValue.ExpenseId")
                    .Where("SavingValue.BudgetId = @0", _budget.Id));
            BudgetSavingValues.Clear();
            savingValues.Where(x => x.Expense.IsTransient()).ForEach(x => BudgetSavingValues.Add(x));

            AvailableSavings.Clear();
            var savings = Database.Query<Saving>("ORDER BY Name ASC");
            AvailableSavings.AddRange(savings);

            SavingValueDate = DateTime.Now;
            SavingValueValue = 0;
            SelectedAvailableSaving = null;
            NotifyOfPropertyChange(() => BudgetSavingValues);
            RefreshRevenueSums();
        }

        public void AddSavingValue()
        {
            if (_budget == null)
            {
                throw new InvalidOperationException("Unable to find the Budget");
            }

            if (SelectedAvailableSaving != null)
            {
                using (var tx = Database.GetTransaction())
                {
                    var savingValue = _budget.WithdrawSavingValue(SelectedAvailableSaving, SavingValueValue, SavingValueDate, SavingValueDescription);
                    Database.Save(savingValue);
                    tx.Complete();
                }
                LoadAvailableSavings();
            }
        }

        public void RemoveSavingValue(SavingValue savingValue)
        {
            if (_budget == null)
            {
                throw new InvalidOperationException("Unable to find Budget");
            }

            using (var tx = Database.GetTransaction())
            {
                _budget.CancelWithdrawSavingValue(savingValue);
                Database.Delete(savingValue);
                tx.Complete();
            }
            LoadAvailableSavings();
        }

        #endregion
    }
}
