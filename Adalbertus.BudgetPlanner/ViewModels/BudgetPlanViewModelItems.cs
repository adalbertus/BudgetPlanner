using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public partial class BudgetPlanViewModel : BaseViewModel
    {
        #region Budget plan items
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
            BudgetPlanList.Clear();

            foreach (var cashFlow in cashFlows)
            {
                //var expenseTotalValue = sumOfExpenses.Single(x => x.Id == cashFlow.Id) as decimal;
                var planItems = budgetPlans.Where(x => x.CashFlow.Id == cashFlow.Id);
                BudgetPlanList.Add(new BudgetPlanItemVM(_budget, cashFlow, planItems));
            }
            BudgetPlanList.IsNotifying = true;

            BudgetPlanList.Refresh();
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
