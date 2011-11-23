using System;
using System.Linq;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Generic;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>, IShellViewModel
    {
        private IDialog _dialogScreen;
        public IDialog DialogScreen
        {
            get { return _dialogScreen; }
            set
            {
                _dialogScreen = value;
                NotifyOfPropertyChange(() => DialogScreen);
            }
        }

        public IDatabase Database { get; private set; }
        public IEventAggregator EventAggregator { get; private set; }
        public ShellViewModel(IDatabase database, IEventAggregator eventAggregator)
        {
            Database = database;
            EventAggregator = eventAggregator;
            CurrentBudgetDate = DateTime.Now;

            EventAggregator.Subscribe(this);
            SQLiteHelper.CreateDefaultDatabase();
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
            BudgetViewModel budgetViewModel = IoC.Get<BudgetViewModel>();
            budgetViewModel.BudgetDate = CurrentBudgetDate;

            if (ActiveItem != null && ActiveItem is BudgetViewModel)
            {
                DeactivateItem(ActiveItem, false);
            }
            ActivateItem(budgetViewModel);
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

        #region Dialog methods
        #region Dialog methods
        public void ShowMessage(string message, System.Action okCallback, System.Action cancelCallback)
        {
            var messageDialog = IoC.Get<MessageBoxViewModel>();
            DialogScreen = messageDialog;
            (DialogScreen as MessageBoxViewModel).Message = message;
            DialogScreen.OKCallback = okCallback;
            DialogScreen.CancelCallback = cancelCallback;
            DialogScreen.AfterCloseCallback = () => { DialogScreen = null; };
        }
        #endregion

        public void ShowDialog<TDialogViewModel>(dynamic initParameters, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog
        {
            var dialog = IoC.Get<TDialogViewModel>();
            dialog.Initialize(initParameters);
            dialog.LoadData();
            dialog.OKCallback = okCallback;
            dialog.CancelCallback = cancelCallback;
            dialog.AfterCloseCallback = () => { DialogScreen = null; };
            DialogScreen = dialog;
        }

        #endregion
    }
}
