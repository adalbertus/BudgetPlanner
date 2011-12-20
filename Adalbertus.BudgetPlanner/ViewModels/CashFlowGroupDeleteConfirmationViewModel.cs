using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class CashFlowGroupDeleteConfirmationViewModel : BaseDailogViewModel
    {
        public CashFlowGroupDeleteConfirmationViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            CashFlowGroups = new BindableCollectionExt<CashFlowGroup>();
        }

        public CashFlowGroup CashFlowGroupToDelete { get; private set; }
        public BindableCollectionExt<CashFlowGroup> CashFlowGroups { get; private set; }
        private CashFlowGroup _selectedCashFlowGroup;
        public CashFlowGroup SelectedCashFlowGroup
        {
            get
            {
                return _selectedCashFlowGroup;
            }
            set
            {
                _selectedCashFlowGroup = value;
                NotifyOfPropertyChange(() => SelectedCashFlowGroup);
                NotifyOfPropertyChange(() => CanClose);
            }
        }

        public new bool CanClose {
            get
            {
                return SelectedCashFlowGroup != null;
            }
        }

        public override void Initialize(dynamic parameters)
        {
            CashFlowGroupToDelete = parameters.CashFlowGroup;
            NotifyOfPropertyChange(() => CashFlowGroupToDelete);
        }

        public override void LoadData()
        {
            CashFlowGroups.Clear();
            var cashFlowList = CachedService.GetAllCashFlowGroups().Where(x => x.Id != CashFlowGroupToDelete.Id);
            cashFlowList.ForEach(x => CashFlowGroups.Add(x));
            SelectedCashFlowGroup = CashFlowGroups.FirstOrDefault();
        }

        public override void Close()
        {
            using (var tx = Database.GetTransaction())
            {
                var sql = PetaPoco.Sql.Builder
                    .Append("SET CashFlowGroupId = @0", SelectedCashFlowGroup.Id)
                    .Where("CashFlow.CashFlowGroupId = @0", CashFlowGroupToDelete.Id);
                Database.Update<CashFlow>(sql);

                tx.Complete();
            }

            base.Close();
        }
    }

}
