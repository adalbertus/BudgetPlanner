using System;
using System.Linq;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Generic;
using Microsoft.Windows.Controls;
using System.Windows;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>, IShellViewModel, IHandle<RefreshEvent>
    {
        private Timer _timer;
        private object _dialogScreen;
        public object DialogScreen
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
            SQLiteHelper.CreateDefaultDatabase();

            Database = database;
            CachedService = cachedService;
            EventAggregator = eventAggregator;
            ConfigurationManager = configurationManager;
            CurrentBudgetDate = DateTime.Now;
            DisplayName = "Budżet Domowy";
            EventAggregator.Subscribe(this);
            _timer = new Timer();
            _timer.Elapsed += delegate
            {
                CheckForUpdates(false);
            };
            CanCheckForUpdates = true;
        }

        public Budget CurrentBudget { get; set; }

        public string Version
        {
            get { return Updater.CurrentVersion; }
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
        }

        public void ShowNextBudget()
        {
            CurrentBudgetDate = CurrentBudgetDate.AddMonths(1);
        }

        public void ShowCashFlowTypes()
        {
            ActivateItem(IoC.Get<CashFlowTypesViewModel>());
        }

        public void ShowSavings()
        {
            ActivateItem(IoC.Get<SavingsViewModel>());
        }

        public void ShowIncomes()
        {
            ActivateItem(IoC.Get<IncomesViewModel>());
        }

        public void ShowBudgetCalculator()
        {
            var calculator = IoC.Get<BudgetCalculationsViewModel>();
            calculator.LoadData(CurrentBudget);
            ActivateItem(calculator);
        }

        public void ShowNotepad()
        {
            ShowDialog<NotepadViewModel>(null, null);
        }

        protected override void OnActivate()
        {
            if (!DatabaseVerification())
            {
                return;
            }
            
            CachedService.LoadAll();
            ShowCurrentBudget();
            int updateIntervalMinutes = ConfigurationManager.GetValueOrDefault(ConfigurationKeys.UpdateMinutesInterval, 15);
            _timer.Interval = updateIntervalMinutes * 60 * 1000;

            if (ConfigurationManager.GetValueOrDefault<bool>(ConfigurationKeys.IsFirstRun, true))
            {
                var message = new StringBuilder();
                message.AppendLine("Witaj w programie Budżet Domowy");
                message.AppendLine();
                message.AppendLine("W pierwszej kolejności wprowadź dane dotyczące źródeł dochodów");
                message.AppendLine("oraz ewentualnych kont oszczędnościowych itp. Aby to zrobić");
                message.AppendLine("kliknij na przycisk 'Środki zewnętrzne' na górze ekranu.");
                message.AppendLine();
                message.AppendLine("Aby dowiedzieć się jak korzystać z programu kliknij na");
                message.AppendLine("ikonkę pomocy umieszczoną w prawym górnym rogu ekranu.");
                ShowMessage(message.ToString());
                ConfigurationManager.SaveValue(ConfigurationKeys.IsFirstRun, false);
            }

            CheckForUpdates(false);
        }

        private bool _canCheckForUpdates;
        public bool CanCheckForUpdates
        {
            get { return _canCheckForUpdates; }
            set
            {
                _canCheckForUpdates = value;
                NotifyOfPropertyChange(() => CanCheckForUpdates);
            }
        }

        public void CheckForUpdates(bool showValidVersionMessage)
        {
            _timer.Stop();
            if (showValidVersionMessage)
            {
                CanCheckForUpdates = false;
            }
            Task.Factory.StartNew(() =>
                {
#if DEBUG
                    if (Updater.CheckForNewVersion("http://budzet-domowy.pietkiewicz.pl/online-update-beta"))
#else
                    if (Updater.CheckForNewVersion(ConfigurationManager.GetValueOrDefault<string>(ConfigurationKeys.UpdatePage, string.Empty)))                    
#endif
                    {
                        ShowDialog<DownloadAndUpgradeViewModel>(() => UpdateApplication(), () => CanCheckForUpdates = true);
                    }
                    else
                    {
                        if (showValidVersionMessage)
                        {
                            ShowMessage("Wersja aplikacji jest aktualna", () => _timer.Start());
                            CanCheckForUpdates = true;
                        }
                        else
                        {
                            _timer.Start();
                        }
                    }
                });
        }

        private void UpdateApplication()
        {
            Updater.RunUpdateAndExit();
        }

        public void OpenHomePage()
        {
            Process.Start(ConfigurationManager.GetValueOrDefault<string>(ConfigurationKeys.HomePage));
        }

        public void ShowHelp()
        {
            var helpPage = ConfigurationManager.GetValueOrDefault<string>(ConfigurationKeys.HelpPage);
            if (helpPage.IsNullOrWhiteSpace())
            {
                ShowMessage("Nie mogę odnaleźć adresu strony WWW z pomocą.\r\nSpróbuj skontaktować się z Twórcą aplikacji.",
                    null, null, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process.Start(helpPage);
            //var message = new StringBuilder();
            //message.AppendLine("W tej chwili jedyna dostępna pomoc to filmik pokazujący jak używać aplikacji.");
            //message.AppendLine();
            //message.AppendLine("Po kliknięciu przycisku OK zostanie otwarta przeglądarka z filmikiem");
            //ShowMessage(message.ToString(), () => Process.Start(helpPage), null, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowAbout()
        {
            var message = new StringBuilder();
            message.AppendLine("Budżet Domowy - program do budżetowania gospodarstwa domowego.");
            message.AppendLine(string.Format("Wersja: {0}", Updater.CurrentVersion));
            message.AppendLine();
            message.AppendLine(string.Format("Program Budżet Domowy jest  własnością autora: Wojciech Pietkiewicz <{0}>", ConfigurationManager.GetValueOrDefault<string>(ConfigurationKeys.AuthorEmail)));
            message.AppendLine(string.Format("Strona WWW: {0}", ConfigurationManager.GetValueOrDefault<string>(ConfigurationKeys.HomePage)));
            message.AppendLine();
            message.AppendLine("Program Budżet Domowy jest darmowy i może być użytkowany, kopiowany i przekazywany dalej, jeśli spełnione są następujące warunki: ");
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
                ConfigurationManager.Clear();
                CachedService.Clear();
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

        public void ShowDialog<TDialogViewModel>(System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog<object>
        {
            ShowDialog<TDialogViewModel, object>(new { }, okCallback, cancelCallback);
        }

        public void ShowDialog<TDialogViewModel>(dynamic initParameters, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog<object>
        {
            ShowDialog<TDialogViewModel, object>(initParameters, okCallback, cancelCallback);
        }

        public void ShowDialog<TDialogViewModel, TModel>(dynamic initParameters, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog<TModel>
        {
            var dialog = IoC.Get<TDialogViewModel>();
            dialog.Initialize(initParameters);
            dialog.LoadData();
            dialog.OKCallback = okCallback;
            dialog.CancelCallback = cancelCallback;
            dialog.AfterCloseCallback = () => { DialogScreen = null; };
            DialogScreen = dialog;
        }

        public void ShowDialog<TDialogViewModel, TModel>(TDialogViewModel instance, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog<TModel>
        {
            instance.OKCallback = okCallback;
            instance.CancelCallback = cancelCallback;
            instance.AfterCloseCallback = () => { DialogScreen = null; };
            DialogScreen = instance;
        }
        #endregion

        #region IHandle<RefreshEvent> Members

        public void Handle(RefreshEvent message)
        {
            if (message.ChangedEntity is Budget)
            {
                CurrentBudget = message.ChangedEntity as Budget;
            }
        }

        #endregion

        #region Debug
        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        private bool _isDebugLogVisible;
        public bool IsDebugLogVisible
        {
            get { return _isDebugLogVisible; }
            set
            {
                _isDebugLogVisible = value;
                NotifyOfPropertyChange(() => IsDebugLogVisible);
            }
        }

        private string _debugLog;
        public string DebugLog
        {
            get { return _debugLog; }
            set
            {
                _debugLog = value;
                NotifyOfPropertyChange(() => DebugLog);
            }
        }

        public void DebugShowLog()
        {
#if DEBUG
            DebugLog = Diagnostics.GetLogAndClear();
            IsDebugLogVisible = true;
#endif
        }

        public void DebugCleadAndHideLog()
        {
#if DEBUG
            DebugLog = string.Empty;
            IsDebugLogVisible = false;
#endif
        }
        #endregion Debug
    }
}
