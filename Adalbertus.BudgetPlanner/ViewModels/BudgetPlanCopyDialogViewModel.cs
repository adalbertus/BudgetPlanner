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

        }

        public List<BudgetPlanCopyVM> Items { get; private set; }

        public Budget CurrentBudget { get; set; }

        public override void Initialize(dynamic parameters)
        {
            CurrentBudget = parameters.CurrentBudget;
            Items = new List<BudgetPlanCopyVM>();
        }

        public override void LoadData()
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
                .Where("BudgetPlan.BudgetId = @0", CurrentBudget.Id);
            var budgetPlans = Database.Query<BudgetPlan, Budget, CashFlow, CashFlowGroup>(sql).ToList();

            var result = cashFlows.GroupBy(x => x.GroupName, (name, id) => new { Name = name, Id = id }).ToList();

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
                            Id = v.Id,
                            Name = string.Format("{0} {1}", v.Value.ToString("C2"), v.Description),
                            IsSelected = true
                        };
                        child.AddChild(planValue);                        
                    });
                });
                Items.Add(planCopy);
            });


            Diagnostics.Stop();
        }
    }
}
