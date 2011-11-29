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
        public CashFlowDeleteConfirmationViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            CashFlows = new BindableCollectionExt<CashFlow>();
        }

        public CashFlow CashFlowToDelete { get; private set; }
        public BindableCollectionExt<CashFlow> CashFlows { get; private set; }
        private CashFlow _selectedCashFlow;
        public CashFlow SelectedCashFlow
        {
            get
            {
                return _selectedCashFlow;
            }
            set
            {
                _selectedCashFlow = value;
                NotifyOfPropertyChange(() => SelectedCashFlow);
                NotifyOfPropertyChange(() => CanClose);
            }
        }
        
        public new bool CanClose
        {
            get
            {
                if (IsUpdateCashFlowSelected)
                {
                    return SelectedCashFlow != null;
                }
                else
                {
                    return IsDeleteCashFlowSelected;
                }
            }
        }
        private bool _isUpdateCashFlowSelected;

        public bool IsUpdateCashFlowSelected
        {
            get { return _isUpdateCashFlowSelected; }
            set
            {
                _isUpdateCashFlowSelected = value;
                _isDeleteCashFlowSelected = !value;
                NotifyOfPropertyChange(() => IsUpdateCashFlowSelected);
                NotifyOfPropertyChange(() => IsDeleteCashFlowSelected);
                NotifyOfPropertyChange(() => CanClose);
            }
        }

        private bool _isDeleteCashFlowSelected;

        public bool IsDeleteCashFlowSelected
        {
            get { return _isDeleteCashFlowSelected; }
            set
            {
                _isDeleteCashFlowSelected = value;
                _isUpdateCashFlowSelected = !value;
                NotifyOfPropertyChange(() => IsDeleteCashFlowSelected);
                NotifyOfPropertyChange(() => IsUpdateCashFlowSelected);
                NotifyOfPropertyChange(() => CanClose);
            }
        }


        public override void Initialize(dynamic parameters)
        {
            CashFlowToDelete = parameters.CashFlow;
            NotifyOfPropertyChange(() => CashFlowToDelete);
            IsUpdateCashFlowSelected = true;
        }

        public override void LoadData()
        {
            CashFlows.Clear();
            var cashFlowList = CachedService.GetAllCashFlows().Where(x => x.Id != CashFlowToDelete.Id);
            cashFlowList.ForEach(x => CashFlows.Add(x));
            SelectedCashFlow = CashFlows.FirstOrDefault();
        }

        public override void Close()
        {
            if (IsUpdateCashFlowSelected)
            {
                UpdateCashFlow();
            }
            base.Close();
        }

        private void UpdateCashFlow()
        {
            using (var tx = Database.GetTransaction())
            {
                var sql = PetaPoco.Sql.Builder
                    .Append("SET CashFlowId = @0", SelectedCashFlow.Id)
                    .Where("CashFlowId = @0", CashFlowToDelete.Id);
                Database.Update<BudgetPlan>(sql);

                sql =  PetaPoco.Sql.Builder
                    .Append("SET CashFlowId = @0", SelectedCashFlow.Id)
                    .Where("CashFlowId = @0", CashFlowToDelete.Id);
                Database.Update<Expense>(sql);
                tx.Complete();
            }
        }
    }
}
