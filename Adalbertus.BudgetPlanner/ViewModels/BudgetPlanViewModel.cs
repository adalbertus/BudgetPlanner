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
        private void AttachEvents()
        {
            AttachToBudgetPlanItems();
            RevenuesViewModel.AttachEvents();
            ExpensesViewModel.AttachEvents();
        }

        private void DeatachEvents()
        {
            DetachFromBudgetPlanItems();
            RevenuesViewModel.DeatachEvents();
            ExpensesViewModel.DeatachEvents();
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
    }
}
