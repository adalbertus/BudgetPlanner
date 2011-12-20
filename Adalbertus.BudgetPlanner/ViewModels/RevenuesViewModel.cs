﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Specialized;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class RevenuesViewModel : BaseViewModel
    {
        public RevenuesViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            AvailableIncomes = new BindableCollectionExt<Income>();
            AvailableSavings = new BindableCollectionExt<Saving>();
        }

        public Budget Budget { get; set; }

        public decimal TotalSumOfRevenues
        {
            get { return Budget.TotalSumOfRevenues; }
        }

        public decimal SumOfRevenueIncomes
        {
            get { return Budget.SumOfRevenueIncomes; }
        }

        public decimal SumOfRevenueSavings
        {
            get
            {
                var s1 = Budget.SavingValues.Where(x => x.Expense != null).Sum(x => x.BudgetValue);
                return Budget.SumOfRevenueSavings; }
        }

        public void LoadData(Budget budget)
        {
            Budget = budget;
            BudgetIncomeValues = Budget.IncomeValues as BindableCollectionExt<IncomeValue>;
            BudgetIncomeValues.PropertyChanged += (s, e) => { SaveRevenue(s as Entity); };
            NotifyOfPropertyChange(() => BudgetIncomeValues);

            BudgetSavingValues = Budget.SavingValues as BindableCollectionExt<SavingValue>;
            BudgetSavingValues.PropertyChanged += (s, e) => { SaveRevenue(s as Entity); };
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
                    .Where("IncomeValue.BudgetId = @0", Budget.Id)).ToList();
            BudgetIncomeValues.IsNotifying = false;
            BudgetIncomeValues.Clear();
            incomeValues.ForEach(x => BudgetIncomeValues.Add(x));
            
            AvailableIncomes.Clear();            
            AvailableIncomes.AddRange(CachedService.GetAllIncomes());
            
            IncomeValueDate = DateTime.Now;
            IncomeValueValue = 0;
            SelectedAvailableIncome = null;

            BudgetIncomeValues.IsNotifying = true;
            BudgetIncomeValues.Refresh();
        }

        public void AddIncomeValue()
        {
            if ((SelectedAvailableIncome != null) && (IncomeValueValue > 0))
            {
                var incomeValue = Budget.AddIncomeValue(SelectedAvailableIncome, IncomeValueValue, IncomeValueDate, IncomeValueDescription);
                SaveRevenue(incomeValue);
                IncomeValueValue = 0;
                IncomeValueDescription = string.Empty;
                SelectedAvailableIncome = null;
            }
        }

        public void RemoveIncomeValue(IncomeValue incomeValue)
        {
            Budget.RemoveIncomeValue(incomeValue);
            DeleteRevenue(incomeValue);
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

        public BindableCollectionExt<SavingValue> BudgetSavingValues { get; private set; }

        private void LoadSavings()
        {
            var budgetSavingValues = Database.Query<SavingValue, Saving, Budget, Expense>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("SavingValue")
                    .InnerJoin("Saving")
                    .On("Saving.Id = SavingValue.SavingId")
                    .InnerJoin("Budget")
                    .On("Budget.Id = SavingValue.BudgetId")
                    .LeftJoin("Expense")
                    .On("Expense.Id = SavingValue.ExpenseId")
                    .Where("SavingValue.BudgetId = @0", Budget.Id)).ToList();
            BudgetSavingValues.IsNotifying = false;
            BudgetSavingValues.Clear();
            budgetSavingValues.Where(x => x.Expense.IsTransient()).ForEach(x => BudgetSavingValues.Add(x));
            BudgetSavingValues.IsNotifying = true;

            AvailableSavings.Clear();
            var savings = Database.Query<Saving>("ORDER BY Name ASC").ToList();
            AvailableSavings.AddRange(savings);

            SavingValueDate = DateTime.Now;
            SavingValueValue = 0;
            SelectedAvailableSaving = null;

            BudgetSavingValues.Refresh();
        }

        public void AddSavingValue()
        {
            if ((SelectedAvailableSaving != null) && (SavingValueValue > 0))
            {
                var savingValue = Budget.WithdrawSavingValue(SelectedAvailableSaving, SavingValueValue, SavingValueDate, SavingValueDescription);
                SaveRevenue(savingValue);
                SavingValueValue = 0;
                SavingValueDescription = string.Empty;
                SelectedAvailableSaving = null;
            }
        }

        public void RemoveSavingValue(SavingValue savingValue)
        {
            Budget.CancelWithdrawSavingValue(savingValue);
            DeleteRevenue(savingValue);
        }

        #endregion

        private void SaveRevenue(Entity entity)
        {
            Save(entity);
            if (entity is IncomeValue)
            {
                CachedService.Clear(CachedServiceKeys.AllIncomes);
            }
            if (entity is SavingValue)
            {
                CachedService.Clear(CachedServiceKeys.AllSavings);
            }
            RefreshSummaryValues();
        }

        private void DeleteRevenue(Entity entity)
        {
            Delete(entity);
            if (entity is IncomeValue)
            {
                CachedService.Clear(CachedServiceKeys.AllIncomes);
            }
            if (entity is SavingValue)
            {
                CachedService.Clear(CachedServiceKeys.AllSavings);
            }
            RefreshSummaryValues();
        }
    }
}
