using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public partial class BudgetPlanViewModel : BaseViewModel
    {
        public BudgetPlanViewModel(IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(database, configuration, cashedService, eventAggregator)
        {
            ExpensesViewModel = new ExpensesViewModel(database, configuration, cashedService, eventAggregator);

            BudgetPlanList = new BindableCollectionExt<BudgetPlanItemVM>();
            _availableIncomes = new BindableCollectionExt<Income>();
            _availableSavings = new BindableCollectionExt<Saving>();
        }

        private ExpensesViewModel _expensesViewModel;
        public ExpensesViewModel ExpensesViewModel {
            get { return _expensesViewModel; }
            set
            {
                _expensesViewModel = value;
                NotifyOfPropertyChange(() => ExpensesViewModel);
            }
        }

        private Budget _budget;
        public DateTime BudgetDate { get; set; }

        #region Loading data

        public override void LoadData()
        {
            LoadOrCreateDefaultBudget();
            
            LoadBudgetPlanItems();
            LoadAvailableIncomes();
            LoadAvailableSavings();
            
            ExpensesViewModel.LoadData(_budget);

            RefreshUI();
        }


        private void LoadOrCreateDefaultBudget()
        {
            using (var tx = Database.GetTransaction())
            {
                var cashFlowList = CachedService.GetAllCashFlows();

                var sql = PetaPoco.Sql.Builder
                                .Select("*")
                                .From("Budget")
                                .Where("@0 BETWEEN DateFrom AND DateTo", BudgetDate.Date);
                _budget = Database.SingleOrDefault<Budget>(sql);
                if (_budget == null)
                {
                    _budget = Budget.CreateEmptyForDate(BudgetDate, cashFlowList);
                    Database.Save(_budget);
                }

                tx.Complete();
            }            
        }

        protected override void OnRefreshRequest(string senderName)
        {
            if (senderName == typeof(ExpensesViewModel).Name)
            {
                //TODO: need refactorisation (should refresh only budget plan items)
                //LoadOrCreateDefaultBudget();
                LoadBudgetPlanItems();
            }
        }

        private void RefreshUI()
        {
            NotifyOfPropertyChange(() => DateFrom);
            NotifyOfPropertyChange(() => DateTo);
            BudgetPlanList.Refresh();

            RefreshRevenueSums();
        }

        private void RefreshRevenueSums()
        {
            NotifyOfPropertyChange(() => TotalSumOfRevenues);
            NotifyOfPropertyChange(() => SumOfRevenueSavings);
            NotifyOfPropertyChange(() => SumOfRevenueIncomes);
        }
        #endregion

        public string TotalSumOfRevenues 
        {
            get 
            {
                //return string.Format("{0:c2}", _budget.IncomeValues.Sum(x => x.Value) + _budget.SavingValues.Sum(x => x.BudgetValue));
                return string.Format("{0:c2}", SumOfRevenueIncomes + SumOfRevenueSavings);
            }
        }

        #region Events handling
        private void AttachEvents()
        {
            AttachToIncomeValues();
            AttachToSavingValues();
            AttachToBudgetPlanItems();
            ExpensesViewModel.AttachEvents();
        }

        private void DeatachEvents()
        {
            DetachFromSavingValues();
            DetachFromIncomeValues();
            DetachFromBudgetPlanItems();
            ExpensesViewModel.DeatachEvents();
        }

        private void BudgetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveBudget(sender as Entity);
        }
        #endregion

        #region Budget main data
        public DateTime DateFrom
        {
            get { return _budget.DateFrom; }
        }

        public DateTime DateTo
        {
            get { return _budget.DateTo; }
        }

        private void SaveBudget(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(entity);

                tx.Complete();
                if (entity is IncomeValue)
                {
                    RefreshRevenueSums();
                }
                if (entity is SavingValue)
                {
                    RefreshRevenueSums();
                }
                NotifyOfPropertyChange(() => BudgetPlanList);
            }
        }
        #endregion
    }
}
