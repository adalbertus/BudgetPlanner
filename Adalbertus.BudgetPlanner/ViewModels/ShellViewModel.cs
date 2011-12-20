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
using System.Diagnostics;
using System.IO;

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
        public ICachedService CachedService { get; private set; }
        public ShellViewModel(IDatabase database, IEventAggregator eventAggregator, IConfigurationManager configurationManager, ICachedService cachedService)
        {
            Database = database;
            CachedService = cachedService;
            EventAggregator = eventAggregator;
            ConfigurationManager = configurationManager;
            CurrentBudgetDate = DateTime.Now;
            DisplayName = "Domowy Budżet";
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
            //ShowCurrentBudget();
        }

        public void ShowNextBudget()
        {
            CurrentBudgetDate = CurrentBudgetDate.AddMonths(1);
            //ShowCurrentBudget();
        }

        public void ShowCashFlowTypes()
        {
            ActivateItem(IoC.Get<CashFlowTypesViewModel>());
        }

        public void ShowExternalSources()
        {
            ActivateItem(IoC.Get<ExternalSourcesViewModel>());
        }

        public void ShowBudgetCalculator()
        {
            ActivateItem(IoC.Get<BudgetCalculationsViewModel>());
        }

        protected override void OnActivate()
        {
            CachedService.LoadAll();
            if (DatabaseVerification())
            {
                ShowCurrentBudget();
            }
            if (ConfigurationManager.GetValueOrDefault<bool>(ConfigurationKeys.IsFirstRun, true))
            {
                var message = new StringBuilder();
                message.AppendLine("Witaj w programie Domowy Budżet");
                message.AppendLine();
                message.AppendLine("Aby dowiedzieć się jak korzystać z programu kliknij na");
                message.AppendLine("ikonkę pomocy umieszczoną w prawym górnym rogu ekranu.");
                ShowMessage(message.ToString());
                ConfigurationManager.SaveValue(ConfigurationKeys.IsFirstRun, false);
            }
        }

        public void ShowHelp()
        {
            var message = new StringBuilder();
            message.AppendLine("W tej chwili jedyna dostępna pomoc to filmik pokazujący jak używać aplikacji.");
            message.AppendLine();
            message.AppendLine("Po kliknięciu przycisku OK zostanie otwarta przeglądarka z filmikiem");
            ShowMessage(message.ToString(), RunFlashHelp ,null, MessageBoxButton.OK, MessageBoxImage.Information);            
        }

        private void RunFlashHelp()
        {
            var helpFile = @"Help\Domowy budżet.htm";
            if (File.Exists(helpFile))
            {
                Process.Start(helpFile);
            }
            else
            {
                Microsoft.Windows.Controls.MessageBox.Show(string.Format("Nie udało się odnaleźć pliku z pomocą: {0}", helpFile), "Brak pliku", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowAbout()
        {

            var message = new StringBuilder();
            message.AppendLine("Domowy Budżet - program do budżetowania gospodarstwa domowego.");
            message.AppendLine(string.Format("Wersja: {0}", GetVersion()));
            message.AppendLine();
            message.AppendLine("Program Domowy Budżet jest  własnością autora: Wojciech Pietkiewicz <domowe-wydatki@pietkiewicz.pl>");
            message.AppendLine();
            message.AppendLine("Program Domowy Budżet jest darmowy i może być użytkowany, kopiowany i przekazywany dalej, jeśli spełnione są następujące warunki: ");
            message.AppendLine("1. Program może być używany prywatnie i zawodowo bez ograniczeń. Nie może być jednakże sprzedawany i nie może być dołączany do innych pakietów oprogramowania bez uzyskania zgody autora.");
            message.AppendLine("2. Program może być umieszczany na bezpłatnych stronach interetowych do pobrania.");
            message.AppendLine("3. Wszystkie inne formy publikacji programu wymagają pisemnego pozwolenia autora.");
            message.AppendLine("4. Program pozostaje zawsze, również gdy jest przekazywany dla osób trzecich, własnością autora.");
            message.AppendLine("5. Program musi pozostać niezmieniony. W szczególności nazwa programu i nazwisko autora nie mogą być zmieniane.");
            message.AppendLine("6. AUTOR NIE GWARANTUJE: ŻE PROGRAM JEST BEZBLĘDNY, ŻE PRACUJE BEZ ZAWIESZEŃ, ŻE ODPOWIADA TWOIM WYMAGANIOM, ŻE BŁĘDY W PROGRAMIE BĘDĄ USUNIĘTE LUB NOWE WERSJE/POPRAWKI  ZOSTANĄ WYKONANE I UDOSTĘPNIONE. PROGRAM JEST DOSTARCZANY  „TAK JAK JEST” BEZ JAKIEJKOLWIEK GWARANCJI. UŻYTKOWNIK AKCEPTUJE, ŻE UŻYTKOWANIE PROGRAMU/ DOKUMENTACJI I JEGO SKUTKI NASTĘPUJE NA JEGO WŁASNE RYZYKO. ZA EWENTUALNE SZKODY WYNIKŁE Z UŻYTKOWANIA PROGRAMU, JAK N.P. UTRATA DANYCH, UTRATA ZAROBKU, PRZERWY W PRACY, UTRATA INFORMACJI BIZNESOWYCH LUB INNE STARTY FINANSOWE, NIEZALEŻNIE OD ICH PRZEWIDYWALNOŚCI, AUTOR NIE PRZEJMUJE ŻADNEJ ODPOWIEDZIALNOŚCI. UŻYTKOWANIE JEST WYŁĄCZNIE NA WŁASNE RYZYKO. PROGRAM JEST LICENCJONOWANY „TAK JAK JEST”.");
            message.AppendLine();
            message.AppendLine("Poprzez pobranie i instalację programu użytkownik uznaje powyższe warunki licencji."); message.AppendLine();
            ShowMessage(message.ToString(), null, null, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool DatabaseVerification()
        {
            var dbHelper = new DatabaseBackupHelper();
            int applicationDatabaseVersion = 2;
            int currentDatabaseVersion = ConfigurationManager.GetValueOrDefault<int>(ConfigurationKeys.DatabaseVersion);

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
        public void ShowMessage(string message, System.Action okCallback = null, System.Action cancelCallback = null, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information)
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
