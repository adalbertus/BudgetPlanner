using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.ObjectModel;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class CashFlowDeleteConfirmationViewModel : BaseDailogViewModel
    {
        public CashFlowDeleteConfirmationViewModel(IShellViewModel shell, IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            Connections = new ObservableCollection<dynamic>();
        }

        public CashFlow CashFlowToDelete { get; private set; }
        public ObservableCollection<dynamic> Connections { get; set; }

        public override void Initialize(dynamic parameters)
        {
            CashFlowToDelete = parameters.CashFlow;
        }

        public override void LoadData()
        {
            var sql = PetaPoco.Sql.Builder
                .Select("*")
                .From("Budget")
                .InnerJoin("BudgetPlan")
                .On("[Budget].Id = [BudgetPlan].BudgetId")
                .Where("[BudgetPlan].CashFlowId = @0", CashFlowToDelete.Id);
            var budgetList = Database.Query<Budget>(sql).ToList();

            Connections.Clear();
            budgetList.ForEach(x => { Connections.Add(x.DateFrom.ToString("yyyy-MM")); });
        }

    }
}
