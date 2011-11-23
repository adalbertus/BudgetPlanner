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
        public BudgetPlanViewModel(IShellViewModel shell, IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            BudgetPlanList = new BindableCollectionExt<BudgetPlanItemVM>();
        }
        public BindableCollectionExt<BudgetPlanItemVM> BudgetPlanList { get; set; }

        public Budget Budget { get; private set; }

        #region Loading data

        public void LoadData(Budget budget)
        {
            Budget = budget;
            LoadBudgetPlanItems();
        }

        protected override void OnRefreshRequest(RefreshEvent refreshEvent)
        {
            if (refreshEvent.Sender == typeof(ExpensesViewModel).Name)
            {
                BudgetPlanList.ForEach(x => x.RefreshUI());
            }            
        }
        #endregion

        #region Budget main data
        #endregion

        #region Budget plan items
        
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
                .Where("BudgetPlan.BudgetId = @0", Budget.Id);
            var budgetPlans = Database.Query<BudgetPlan, Budget, CashFlow, CashFlowGroup>(sql);

            var sumOfExpenses = Database.Query<dynamic>(PetaPoco.Sql.Builder
                .Select("CashFlow.Id, SUM(ifnull(Value, 0) Sum)")
                .From("CashFlow")
                .LeftJoin("Expense")
                .On("Expense.CashFlowId = CashFlow.Id AND BudgetId = @0", Budget.Id));

            Budget.BudgetPlanItems.Clear();
            ((BindableCollectionExt<BudgetPlan>)Budget.BudgetPlanItems).AddRange(budgetPlans);
            BudgetPlanList.IsNotifying = false;
            DetachFromBudgetPlanItems();
            BudgetPlanList.Clear();

            foreach (var cashFlow in cashFlows)
            {
                var planItems = budgetPlans.Where(x => x.CashFlow.Id == cashFlow.Id);
                BudgetPlanList.Add(new BudgetPlanItemVM(Budget, cashFlow, planItems));
            }
            BudgetPlanList.IsNotifying = true;

            NotifyOfPropertyChange(() => BudgetPlanList);
            BudgetPlanList.Refresh();
            AttachToBudgetPlanItems();
        }

        private void AttachToBudgetPlanItems()
        {
            BudgetPlanList.ForEach(x =>
            {
                x.Values.CollectionChanged += BudgetPlanListCollectionChanged;
                x.Values.PropertyChanged += (s, e) => { SaveBudgetPlan(s as BudgetPlan); };
            });
        }

        private void DetachFromBudgetPlanItems()
        {
            BudgetPlanList.ForEach(x =>
            {
                x.Values.CollectionChanged -= BudgetPlanListCollectionChanged;
            });
        }

        private void SaveBudgetPlan(BudgetPlan budgetPlan)
        {
            Save(budgetPlan);
            FindBudgetPlanItemVMFor(budgetPlan).RefreshUI();
        }

        private BudgetPlanItemVM FindBudgetPlanItemVMFor(BudgetPlan budgetPlan)
        {
            var budgetPlanItem = BudgetPlanList
                    .Where(x => x.CashFlow.Id == budgetPlan.CashFlowId && x.Budget.Id == budgetPlan.BudgetId)
                    .First();
            return budgetPlanItem;
        }

        public void FocusNewValue(BudgetPlanItemVM budgetPlanItem)
        {
            budgetPlanItem.IsNewValueFocused = false;
            budgetPlanItem.IsNewValueFocused = true;
        }

        public void AddNewValueToBudgetPlan(BudgetPlanItemVM budgetPlanItem)
        {
            if (budgetPlanItem.NewValue.GetValueOrDefault(0) == 0)
            {
                FocusNewValue(budgetPlanItem);
                return;
            }
            var newPlan = budgetPlanItem.AddValue(budgetPlanItem.NewValue.GetValueOrDefault(0), budgetPlanItem.NewDescription);
            SaveBudgetPlan(newPlan);
            budgetPlanItem.NewDescription = string.Empty;
            budgetPlanItem.NewValue = null;
            FocusNewValue(budgetPlanItem);
        }

        public void DeleteBudgetPlanItem(BudgetPlan planItem)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(planItem);
                tx.Complete();
                var budgetPlanItem = FindBudgetPlanItemVMFor(planItem);
                budgetPlanItem.Values.Remove(planItem);
                budgetPlanItem.RefreshUI();
                PublishRefreshRequest(planItem);
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

        #endregion
    }
}
