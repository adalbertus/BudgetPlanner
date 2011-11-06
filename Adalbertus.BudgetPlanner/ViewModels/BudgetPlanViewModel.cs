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
            ExpensesViewModel = IoC.Get<ExpensesViewModel>();
            RevenuesViewModel = IoC.Get<RevenuesViewModel>();

            BudgetPlanList = new BindableCollectionExt<BudgetPlanItemVM>();
        }

        public ExpensesViewModel ExpensesViewModel { get; private set; }
        public RevenuesViewModel RevenuesViewModel { get; private set; }

        private Budget _budget;
        public DateTime BudgetDate { get; set; }

        #region Loading data

        protected override void LoadData()
        {
            LoadOrCreateDefaultBudget();

            LoadBudgetPlanItems();
            RevenuesViewModel.LoadData(_budget);
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
                LoadBudgetPlanItems();                
            }
        }

        private void RefreshUI()
        {
            NotifyOfPropertyChange(() => DateFrom);
            NotifyOfPropertyChange(() => DateTo);
            BudgetPlanList.Refresh();
        }
        #endregion

        #region Events handling
        public override void AttachEvents()
        {
        }

        public override void DeatachEvents()
        {
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
            using (var tx = Database.GetTransaction())
            {
                Database.Save(entity);

                tx.Complete();
                NotifyOfPropertyChange(() => BudgetPlanList);
            }
        }
        #endregion

        #region Budget plan items
        public decimal TotalBudgetPlanValue
        {
            get { return BudgetPlanList.Sum(x => x.TotalValue); }
        }

        public decimal TotalExpenseValue
        {
            get { return BudgetPlanList.Sum(x => x.TotalExpenseValue); }
        }
        public decimal TotalBalanceValue
        {
            get
            {
                return TotalBudgetPlanValue - TotalExpenseValue;
            }
        }


        public decimal TotalBalanceProcentValue
        {
            get
            {
                if (TotalBudgetPlanValue == 0)
                {
                    return 0;
                }
                return (TotalExpenseValue / TotalBudgetPlanValue) * 100M;
            }
        }

        public BindableCollectionExt<BudgetPlanItemVM> BudgetPlanList { get; set; }

        private void LoadBudgetPlanItems()
        {
            var cashFlows = CachedService.GetAllCashFlows();

            var sql = PetaPoco.Sql.Builder
                .Select("*")
                .From("BudgetPlan")
                .InnerJoin("Budget")
                .On("Budget.Id = BudgetPlan.BudgetId")
                .InnerJoin("CashFlow")
                .On("CashFlow.Id = BudgetPlan.CashFlowId")
                .InnerJoin("CashFlowGroup")
                .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                .Where("BudgetPlan.BudgetId = @0", _budget.Id);
            var budgetPlans = Database.Query<BudgetPlan, Budget, CashFlow, CashFlowGroup>(sql);

            var sumOfExpenses = Database.Query<dynamic>(PetaPoco.Sql.Builder
                .Select("CashFlow.Id, SUM(ifnull(Value, 0) Sum)")
                .From("CashFlow")
                .LeftJoin("Expense")
                .On("Expense.CashFlowId = CashFlow.Id AND BudgetId = @0", _budget.Id));

            BudgetPlanList.IsNotifying = false;
            DetachFromBudgetPlanItems();
            BudgetPlanList.Clear();

            foreach (var cashFlow in cashFlows)
            {
                //var expenseTotalValue = sumOfExpenses.Single(x => x.Id == cashFlow.Id) as decimal;
                var planItems = budgetPlans.Where(x => x.CashFlow.Id == cashFlow.Id);
                BudgetPlanList.Add(new BudgetPlanItemVM(_budget, cashFlow, planItems));
            }
            BudgetPlanList.IsNotifying = true;

            NotifyOfPropertyChange(() => BudgetPlanList);
            BudgetPlanList.Refresh();
            RefreshBudgetPlanSummary();
            AttachToBudgetPlanItems();
        }

        private void AttachToBudgetPlanItems()
        {
            BudgetPlanList.ForEach(x =>
            {
                x.Values.CollectionChanged += BudgetPlanListCollectionChanged;
                x.Values.PropertyChanged += BudgetPlanListPropertyChanged;
            });
        }

        private void BudgetPlanListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveBudget(sender as Entity);
            RefreshBudgetPlanFor(sender);            
        }

        private void RefreshBudgetPlanSummary()
        {
            NotifyOfPropertyChange(() => TotalBudgetPlanValue);
            NotifyOfPropertyChange(() => TotalExpenseValue);
            NotifyOfPropertyChange(() => TotalBalanceProcentValue);
            NotifyOfPropertyChange(() => TotalBalanceValue);
        }

        private void RefreshBudgetPlanFor(object sender)
        {
            var budgetPlan = sender as BudgetPlan;
            if (budgetPlan != null)
            {
                var budgetPlanList = BudgetPlanList
                    .Where(x => x.CashFlow.Id == budgetPlan.CashFlowId && x.Budget.Id == budgetPlan.BudgetId)
                    .FirstOrDefault();
                if (budgetPlanList != null)
                {
                    budgetPlanList.Refresh();
                }
                RefreshBudgetPlanSummary();
            }
        }

        private void BudgetPlanListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var planItem = e.OldItems[0] as BudgetPlan;
                DeleteBudgetPlanItem(planItem);
            }
        }

        private void DeleteBudgetPlanItem(BudgetPlan planItem)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(planItem);
                tx.Complete();
                RefreshBudgetPlanFor(planItem);
            }
        }

        private void DetachFromBudgetPlanItems()
        {
            BudgetPlanList.ForEach(x =>
            {
                x.Values.CollectionChanged -= BudgetPlanListCollectionChanged;
                x.Values.PropertyChanged -= BudgetPlanListPropertyChanged;
                x.PropertyChanged -= BudgetPlanListPropertyChanged;
            });
        }


        #endregion
    }
}
