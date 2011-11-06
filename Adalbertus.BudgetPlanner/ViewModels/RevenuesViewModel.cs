using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Specialized;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class RevenuesViewModel : BaseViewModel
    {
        public RevenuesViewModel(IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(database, configuration, cashedService, eventAggregator)
        {
            AvailableIncomes = new BindableCollectionExt<Income>();
        }

        public Budget Budget { get; set; }

        public decimal TotalSumOfRevenues
        {
            get
            {
                return SumOfRevenueIncomes + SumOfRevenueSavings;
            }
        }

        public void LoadData(Budget budget)
        {
            Budget = budget;
            BudgetIncomeValues = Budget.IncomeValues as BindableCollectionExt<IncomeValue>;
            BudgetIncomeValues.PropertyChanged += (s, e) => { Save(s as Entity); };
            NotifyOfPropertyChange(() => BudgetIncomeValues);

            BudgetSavingValues = Budget.SavingValues as BindableCollectionExt<SavingValue>;
            BudgetSavingValues.PropertyChanged += (s, e) => { Save(s as Entity); };
            NotifyOfPropertyChange(() => BudgetSavingValues);

            LoadIncomes();
            LoadSavings();

            RefreshSummaryValues();
        }

        private void RefreshSummaryValues()
        {
            NotifyOfPropertyChange(() => SumOfRevenueIncomes);
            NotifyOfPropertyChange(() => SumOfRevenueSavings);
            NotifyOfPropertyChange(() => TotalSumOfRevenues);
        }

        #region Budget revenues: Income
        public BindableCollectionExt<Income> AvailableIncomes { get; private set; }

        private Income _selectedAvailableIncome;
        public Income SelectedAvailableIncome
        {
            get { return _selectedAvailableIncome; }
            set
            {
                _selectedAvailableIncome = value;
                NotifyOfPropertyChange(() => SelectedAvailableIncome);
            }
        }

        private DateTime _incomeValueDate;
        public DateTime IncomeValueDate
        {
            get { return _incomeValueDate; }
            set
            {
                _incomeValueDate = value;
                NotifyOfPropertyChange(() => IncomeValueDate);
                NotifyOfPropertyChange(() => CanAddIncomeValue);
            }
        }

        private decimal _incomeValueValue;
        public decimal IncomeValueValue
        {
            get { return _incomeValueValue; }
            set
            {
                _incomeValueValue = value;
                NotifyOfPropertyChange(() => IncomeValueValue);
                NotifyOfPropertyChange(() => CanAddIncomeValue);
            }
        }

        private string _incomeValueDescription;
        public string IncomeValueDescription
        {
            get { return _incomeValueDescription; }
            set
            {
                _incomeValueDescription = value;
                NotifyOfPropertyChange(() => IncomeValueDescription);
            }
        }


        public decimal SumOfRevenueIncomes
        {
            get
            {
                return BudgetIncomeValues.Sum(x => x.Value);
            }
        }

        public bool CanAddIncomeValue
        {
            get
            {
                bool isIncomeSelected = SelectedAvailableIncome != null;
                bool isIncomeEntryValueNotZero = IncomeValueValue > 0;

                return isIncomeSelected && isIncomeEntryValueNotZero;
            }
        }

        public BindableCollectionExt<IncomeValue> BudgetIncomeValues { get; private set; }

        private void LoadIncomes()
        {
            var incomeValues = Database.Query<IncomeValue, Income, Budget>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("IncomeValue")
                    .InnerJoin("Income")
                    .On("Income.Id = IncomeValue.IncomeId")
                    .InnerJoin("Budget")
                    .On("Budget.Id = IncomeValue.BudgetId")
                    .Where("IncomeValue.BudgetId = @0", Budget.Id));
            BudgetIncomeValues.IsNotifying = false;
            BudgetIncomeValues.Clear();
            incomeValues.ForEach(x => BudgetIncomeValues.Add(x));

            AvailableIncomes.Clear();
            var incomes = Database.Query<Income>("ORDER BY Name ASC");
            AvailableIncomes.AddRange(incomes);

            IncomeValueDate = DateTime.Now;
            IncomeValueValue = 0;
            SelectedAvailableIncome = null;

            BudgetIncomeValues.IsNotifying = true;
            BudgetIncomeValues.Refresh();
        }

        public void AddIncomeValue()
        {
            if (SelectedAvailableIncome != null)
            {
                var incomeValue = Budget.AddIncomeValue(SelectedAvailableIncome, IncomeValueValue, IncomeValueDate, IncomeValueDescription);
                Save(incomeValue);
            }
        }

        public void RemoveIncomeValue(IncomeValue incomeValue)
        {
            Budget.RemoveIncomeValue(incomeValue);
            Delete(incomeValue);
        }

        #endregion

        #region Budget revenues: Savings

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
                return BudgetSavingValues.Where(x => x.Expense != null).Sum(x => x.BudgetValue);
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

        public BindableCollectionExt<SavingValue> BudgetSavingValues { get; private set; }

        private void LoadSavings()
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
                    .Where("SavingValue.BudgetId = @0", Budget.Id));
            BudgetSavingValues.IsNotifying = false;
            BudgetSavingValues.Clear();
            savingValues.Where(x => x.Expense.IsTransient()).ForEach(x => BudgetSavingValues.Add(x));
            BudgetSavingValues.IsNotifying = true;

            AvailableSavings.Clear();
            var savings = Database.Query<Saving>("ORDER BY Name ASC");
            AvailableSavings.AddRange(savings);

            SavingValueDate = DateTime.Now;
            SavingValueValue = 0;
            SelectedAvailableSaving = null;

            BudgetSavingValues.Refresh();
        }

        public void AddSavingValue()
        {
            if (SelectedAvailableSaving != null)
            {
                var savingValue = Budget.WithdrawSavingValue(SelectedAvailableSaving, SavingValueValue, SavingValueDate, SavingValueDescription);
                Save(savingValue);
            }
        }

        public void RemoveSavingValue(SavingValue savingValue)
        {
            Budget.CancelWithdrawSavingValue(savingValue);
            Delete(savingValue);
        }

        #endregion

        private void Save(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(entity);
                tx.Complete();
            }
            RefreshSummaryValues();
        }

        private void Delete(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(entity);
                tx.Complete();
            }
            RefreshSummaryValues();
        }
    }
}
