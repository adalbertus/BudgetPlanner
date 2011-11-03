using System;
using System.Linq;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>
    {
        public IDatabase Database { get; private set; }
        public ShellViewModel(IDatabase database, IConfiguration configuration)
        {
            Database = database;
            SQLiteHelper.CreateDefaultDatabase();
            CurrentBudgetDate = DateTime.Now;
#if DEBUG
            //CreateDefaultData();
#endif
        }

        public Budget CurrentBudget { get; set; }

        private DateTime _currentBudgetDate;
        public DateTime CurrentBudgetDate
        {
            get { return _currentBudgetDate; }
            set
            {
                _currentBudgetDate = value;
                NotifyOfPropertyChange(() => CurrentBudgetDate);
            }
        }

        public void ShowCurrentBudget()
        {
            BudgetPlanViewModel budgetPlanViewModel = IoC.Get<BudgetPlanViewModel>();
            budgetPlanViewModel.BudgetDate = CurrentBudgetDate;

            if (ActiveItem != null && ActiveItem is BudgetPlanViewModel)
            {
                DeactivateItem(ActiveItem, false);
            }
            ActivateItem(budgetPlanViewModel);
        }

        public void ShowPreviousBudget()
        {
            CurrentBudgetDate = CurrentBudgetDate.AddMonths(-1);
            ShowCurrentBudget();
        }

        public void ShowNextBudget()
        {
            CurrentBudgetDate = CurrentBudgetDate.AddMonths(1);
            ShowCurrentBudget();
        }

        public void ShowCashFlowTypes()
        {
            ActivateItem(IoC.Get<CashFlowTypesViewModel>());
        }

        public void ShowExternalSources()
        {
            ActivateItem(IoC.Get<ExternalSourcesViewModel>());
        }

        protected override void OnActivate()
        {
            ShowCurrentBudget();
        }
    }
}
