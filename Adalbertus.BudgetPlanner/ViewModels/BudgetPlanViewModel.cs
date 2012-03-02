﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;
using Caliburn.Micro;
using System.Diagnostics;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public partial class BudgetPlanViewModel : BaseViewModel
    {
        public BudgetPlanViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            AllBudgetPlanList = new BindableCollectionExt<BudgetPlanItemVM>();
            BudgetPlanListGrouped = new BindableCollectionExt<BudgetPlanGroupItemVM>();
        }

        public BindableCollectionExt<BudgetPlanItemVM> AllBudgetPlanList { get; private set; }

        public BindableCollectionExt<BudgetPlanGroupItemVM> BudgetPlanListGrouped { get; private set; }

        public Budget Budget { get; private set; }

        #region Loading data

        public void LoadData(Budget budget)
        {
            Budget = budget;
            LoadBudgetPlanItems();
        }

        protected override void OnRefreshRequest(RefreshEvent refreshEvent)
        {
            Diagnostics.Start();
            if (refreshEvent.Sender == typeof(ExpensesViewModel).Name)
            {
                AllBudgetPlanList.ForEach(x => x.RefreshUI());
            }

            if (refreshEvent.ChangedEntity is BudgetPlan && refreshEvent.Sender == typeof(BudgetTemplateDialogViewModel).Name)
            {
                var budgetPlanItem = refreshEvent.ChangedEntity as BudgetPlan;
                var budgetPlan = AllBudgetPlanList.First(x => x.CashFlow.Id == budgetPlanItem.CashFlowId);
                budgetPlan.Values.Add(budgetPlanItem);
            }

            if (refreshEvent.ChangedEntity is IEnumerable<BudgetPlan>)
            {
                var changedPlans = refreshEvent.ChangedEntity as IEnumerable<BudgetPlan>;
                var gropedChangedPlans = changedPlans
                    .GroupBy(x => x.CashFlowId, (cashFlowId, budgetPlanItems) => new { CashFlowId = cashFlowId, BudgetPlanItems = budgetPlanItems })
                    .ToList();
                gropedChangedPlans.ForEach(x => 
                {
                    var budgetPlan = AllBudgetPlanList.First(y => y.CashFlow.Id == x.CashFlowId);
                    budgetPlan.Values.AddRange(x.BudgetPlanItems);
                });
            }

            Diagnostics.Stop();
        }
        #endregion

        #region Budget plan items
        
        private void LoadBudgetPlanItems()
        {
            Diagnostics.Start();
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
            var budgetPlans = Database.Query<BudgetPlan, Budget, CashFlow, CashFlowGroup>(sql).ToList();
            
            Budget.BudgetPlanItems.Clear();
            ((BindableCollectionExt<BudgetPlan>)Budget.BudgetPlanItems).AddRange(budgetPlans);

            DetachFromBudgetPlanItems();
            AllBudgetPlanList.Clear();            
            foreach (var cashFlow in cashFlows)
            {
                var planItems = budgetPlans.Where(x => x.CashFlow.Id == cashFlow.Id).ToList();
                AllBudgetPlanList.Add(new BudgetPlanItemVM(Budget, cashFlow, planItems));
            }

            BudgetPlanListGrouped.Clear();
            cashFlows.GroupBy(x => x.GroupName).ForEach(cf =>
            {
                var groupBudgetPlanItems = AllBudgetPlanList.Where(x => x.GroupName == cf.Key).ToList();
                var groupedItem = new BudgetPlanGroupItemVM(groupBudgetPlanItems)
                {
                    GroupName = cf.Key,
                };
                BudgetPlanListGrouped.Add(groupedItem);
            });
                      
            AttachToBudgetPlanItems();
            Diagnostics.Stop();
        }

        private void AttachToBudgetPlanItems()
        {
            Diagnostics.Start();
            AllBudgetPlanList.ForEach(x =>
            {
                x.Values.CollectionChanged += BudgetPlanListCollectionChanged;
                x.Values.PropertyChanged += (s, e) => { SaveBudgetPlan(s as BudgetPlan); };
            });
            Diagnostics.Stop();
        }

        private void DetachFromBudgetPlanItems()
        {
            Diagnostics.Start();
            AllBudgetPlanList.ForEach(x =>
            {
                x.Values.CollectionChanged -= BudgetPlanListCollectionChanged;
            });
            Diagnostics.Stop();
        }

        private void SaveBudgetPlan(BudgetPlan budgetPlan)
        {
            Diagnostics.Start();
            Save(budgetPlan);
            Diagnostics.Stop();
        }

        private BudgetPlanItemVM FindBudgetPlanItemVMFor(BudgetPlan budgetPlan)
        {
            Diagnostics.Start();
            var budgetPlanItem = AllBudgetPlanList
                    .Where(x => x.CashFlow.Id == budgetPlan.CashFlowId && x.Budget.Id == budgetPlan.BudgetId)
                    .First();
            Diagnostics.Stop();
            return budgetPlanItem;
        }

        public void FocusNewValue(BudgetPlanItemVM budgetPlanItem)
        {
            Diagnostics.Start();
            budgetPlanItem.IsNewValueFocused = false;
            budgetPlanItem.IsNewValueFocused = true;
            Diagnostics.Stop();
        }

        public void AddNewValueToBudgetPlan(BudgetPlanItemVM budgetPlanItem)
        {
            Diagnostics.Start();
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
            Diagnostics.Stop();
        }

        public void DeleteBudgetPlanItem(BudgetPlan planItem)
        {
            Diagnostics.Start();
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(planItem);
                tx.Complete();
                var budgetPlanItem = FindBudgetPlanItemVMFor(planItem);
                budgetPlanItem.Values.Remove(planItem);
                PublishRefreshRequest(planItem);
            }
            Diagnostics.Stop();
        }

        private void BudgetPlanListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Diagnostics.Start();
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var planItem = e.OldItems[0] as BudgetPlan;
                DeleteBudgetPlanItem(planItem);
            }
            Diagnostics.Stop();
        }

        #endregion

        public void CopyFromPreviousPlan()
        {
            Shell.ShowDialog<BudgetPlanCopyDialogViewModel>(new { CurrentBudget = Budget }, null, null);
        }
    }
}
