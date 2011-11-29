using System;
using System.Linq;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Generic;
using Microsoft.Windows.Controls;
using System.Windows;
using System.Text;
using System.Reflection;

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
        public IConfigurationManager ConfigurationManager { get; private set; }
        public ShellViewModel(IDatabase database, IEventAggregator eventAggregator, IConfigurationManager configurationManager)
        {
            Database = database;
            EventAggregator = eventAggregator;
            ConfigurationManager = configurationManager;
            CurrentBudgetDate = DateTime.Now;
            DisplayName = "Domowe Wydatki";
            EventAggregator.Subscribe(this);
            SQLiteHelper.CreateDefaultDatabase();
#if DEBUG
            //CreateDefaultData();
#endif
        }

        public Budget CurrentBudget { get; set; }

        public string Version
        {
            get { return GetVersion(); }
        }

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

        private string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
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
            if (DatabaseVerification())
            {
                ShowCurrentBudget();
            }
        }

        public void ShowHelp()
        {
            ShowMessage("Niestety jeszcze nie gotowe :(", null, null, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowAbout()
        {
            var message = new StringBuilder();
            message.AppendLine("Domowe Wydatki - program do budżetowania gospodarstwa domowego.");
            message.AppendLine(string.Format("Wersja: {0}", GetVersion()));
            message.AppendLine();
            message.AppendLine("Autor: Wojciech Pietkiewicz [wojciech@pietkiewicz.pl] (c) 2011");
            ShowMessage(message.ToString(), null, null, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool DatabaseVerification()
        {
            var dbHelper = new DatabaseBackupHelper();
            int applicationDatabaseVersion = 1;
            int currentDatabaseVersion = ConfigurationManager.GetGetValueOrDefault<int>(ConfigurationKeys.DatabaseVersion);

            if (currentDatabaseVersion == applicationDatabaseVersion)
            {
                dbHelper.CreateBackup();
                return true;
            }
            try
            {
                dbHelper.CreateBackup();
                DatabaseUpdateHelper.UpdateIfNeeded(Database, currentDatabaseVersion);
                return true;
            }
            catch (Exception ex)
            {
                var message = new StringBuilder();
                message.AppendLine(string.Format("Błąd aktualizacji bazy danych do wersji {0}.", applicationDatabaseVersion));
                message.AppendLine();
                message.AppendLine(ex.Message);
                ShowMessage(message.ToString(), () => { TryClose(); }, null, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #region Dialog methods
        public void ShowMessage(string message, System.Action okCallback, System.Action cancelCallback, MessageBoxButton button = MessageBoxButton.OKCancel, MessageBoxImage image = MessageBoxImage.Information)
        {
            var messageDialog = IoC.Get<MessageBoxViewModel>();
            messageDialog.Image = image;
            messageDialog.Button = button;
            messageDialog.Message = message;
            messageDialog.OKCallback = okCallback;
            messageDialog.CancelCallback = cancelCallback;
            messageDialog.AfterCloseCallback = () => { DialogScreen = null; };
            DialogScreen = messageDialog;

        }


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
