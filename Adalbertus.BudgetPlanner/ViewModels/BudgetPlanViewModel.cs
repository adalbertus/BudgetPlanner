using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public partial class BudgetPlanViewModel : BaseViewModel
    {
        public BudgetPlanViewModel(IDatabase database, IConfiguration configuration)
            : base(database, configuration)
        {
            BudgetPlanList = new BindableCollectionExt<BudgetPlanItemVM>();
            ExpensesGridCashFlows = new BindableCollectionExt<CashFlow>();
            CashFlows = new BindableCollectionExt<CashFlow>();
            _availableIncomes = new BindableCollectionExt<Income>();
            _availableSavings = new BindableCollectionExt<Saving>();
        }

        private Budget _budget;
        public DateTime BudgetDate { get; set; }

        // Workaround - need ExpensesGridCashFlows and CashFlows because without spliting
        // it was refresed only in DataGrid in ExpensesView
        public BindableCollectionExt<CashFlow> ExpensesGridCashFlows { get; private set; }
        public BindableCollectionExt<CashFlow> CashFlows { get; private set; }

        #region Loading data
        protected override void OnActivate()
        {
            base.OnActivate();
            LoadData();
            AttachEvents();
        }

        private void LoadData()
        {
            LoadCashFlows();
            LoadOrCreateDefaultBudget();
            LoadAvailableIncomes();
            LoadAvailableSavings();
        }

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

        private void LoadOrCreateDefaultBudget()
        {
            using (var tx = Database.GetTransaction())
            {
                var sql = PetaPoco.Sql.Builder
                                .Select("*")
                                .From("Budget")
                                .Where("@0 BETWEEN DateFrom AND DateTo", BudgetDate.Date);
                _budget = Database.SingleOrDefault<Budget>(sql);
                if (_budget == null)
                {
                    _budget = Budget.CreateEmptyForDate(BudgetDate, ExpensesGridCashFlows);
                    Database.Save(_budget);
                }
                tx.Complete();
            }
            LoadBudgetPlanItems();
            LoadExpenses();
            RefreshUI();
        }

        private void RefreshUI()
        {
            NotifyOfPropertyChange(() => DateFrom);
            NotifyOfPropertyChange(() => DateTo);
            //NotifyOfPropertyChange(() => BudgetPlanList);
            BudgetPlanList.Refresh();
            NotifyOfPropertyChange(() => BudgetExpenses);

            RefreshRevenueSums();
        }

        private void RefreshRevenueSums()
        {
            NotifyOfPropertyChange(() => TotalSumOfRevenues);
            NotifyOfPropertyChange(() => SumOfRevenueSavings);
            NotifyOfPropertyChange(() => SumOfRevenueIncomes);
        }
        #endregion

        protected override void OnDeactivate(bool close)
        {
            DeatachEvents();
            base.OnDeactivate(close);
        }

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
            AttachToExpenses();
            AttachToBudgetPlanItems();
        }

        private void DeatachEvents()
        {
            DetachFromExpenses();
            DetachFromSavingValues();
            DetachFromIncomeValues();
            DetachFromBudgetPlanItems();
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
            if (entity is Expense)
            {
                SaveExpense(entity as Expense);
                return;
            }

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
