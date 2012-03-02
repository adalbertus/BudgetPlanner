using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Extensions;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetPlanCopyDialogViewModel : BaseDailogViewModel
    {
        public BudgetPlanCopyDialogViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            AvaiableBudgetDates = new BindableCollection<string>();
            Items = new BindableCollectionExt<BudgetPlanCopyVM>();
        }

        public BindableCollectionExt<BudgetPlanCopyVM> Items { get; private set; }

        public BindableCollection<string> AvaiableBudgetDates { get; private set; }
        private string _selectedBudgetDate;

        public string SelectedBudgetDate
        {
            get { return _selectedBudgetDate; }
            set
            {
                _selectedBudgetDate = value;
                LoadBudgetPlans();
                NotifyOfPropertyChange(() => SelectedBudgetDate);
            }
        }

        public Budget CurrentBudget { get; set; }


        public override void Initialize(dynamic parameters)
        {
            CurrentBudget = parameters.CurrentBudget;
        }

        public override void LoadData()
        {
            Diagnostics.Start();

            LoadAvaiableBudgets();

            Diagnostics.Stop();
        }

        private void LoadAvaiableBudgets()
        {
            var avaiableBudgets = Database.Query<DateTime>(PetaPoco.Sql.Builder
                .Select("DateFrom")
                .From("[Budget]")
                .OrderBy("[DateFrom]")).ToList();

            AvaiableBudgetDates.Clear();
            avaiableBudgets.ForEach(x => AvaiableBudgetDates.Add(x.ToString("yyyy-MM")));

            var currentBudgetIndex = AvaiableBudgetDates.IndexOf(CurrentBudget.DateFrom.ToString("yyyy-MM"));
            if (currentBudgetIndex > 1)
            {
                SelectedBudgetDate = AvaiableBudgetDates[currentBudgetIndex - 1];
            }
            else
            {
                SelectedBudgetDate = AvaiableBudgetDates.FirstOrDefault();
            }

        }

        private void LoadBudgetPlans()
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
                .Where("Budget.DateFrom = datetime(@0)", string.Format("{0}-01", SelectedBudgetDate));
            var budgetPlans = Database.Query<BudgetPlan, Budget, CashFlow, CashFlowGroup>(sql).ToList();

            var result = cashFlows.GroupBy(x => x.GroupName, (name, id) => new { Name = name, Id = id }).ToList();

            Items.Clear();
            cashFlows.GroupBy(x => x.GroupName, (name, cfs) => new { Name = name, CashFlows = cfs }).ForEach(x =>
            {
                var planCopy = new BudgetPlanCopyVM { Name = x.Name };
                x.CashFlows.ForEach(c =>
                {
                    var child = planCopy.AddChild(c.Name, c.Id);
                    budgetPlans.Where(bp => bp.CashFlowId == child.Id).ForEach(v =>
                    {
                        var planValue = new BudgetPlanCopyVM
                        {
                            Id = v.CashFlowId,
                            Name = v.Description,
                            Value = v.Value
                        };
                        child.AddChild(planValue);
                    });
                });
                Items.Add(planCopy);
            });
        }

        public void Copy()
        {
            if (!Items.Any(x => x.IsSelected))
            {
                Close();
                return;
            }

            var cashFlows = CachedService.GetAllCashFlows();
            var copiedPlanItems = new List<BudgetPlan>();
            Items.Where(x => x.IsSelected).ForEach(groupItem =>
            {
                groupItem.Children.Where(y => y.IsSelected).ForEach(categoryItem =>
                {
                    categoryItem.Children.Where(z => z.IsSelected).ForEach(valueItem => 
                    {
                        var cashFlow = cashFlows.First(c => c.Id == valueItem.Id);
                        var planItem = CurrentBudget.AddPlanValue(cashFlow, valueItem.Value.GetValueOrDefault(0), valueItem.Name);
                        copiedPlanItems.Add(planItem);
                    });
                });
            });

            using (var tx = Database.GetTransaction())
            {
                Database.SaveAll<BudgetPlan>(copiedPlanItems);
                tx.Complete();
                PublishRefreshRequest(copiedPlanItems);
            }
            Close();
        }
    }
}
