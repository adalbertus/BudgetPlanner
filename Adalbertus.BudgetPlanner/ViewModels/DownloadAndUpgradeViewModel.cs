using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class DownloadAndUpgradeViewModel : BaseDailogViewModel
    {
        public DownloadAndUpgradeViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            _webClient = new WebClient();
            _webClient.DownloadFileCompleted += OnDownloadFileCompleted;
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
        }

        private WebClient _webClient;

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                NotifyOfPropertyChange(() => ProgressValue);
            }
        }

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get { return _isProgressVisible; }
            set
            {
                _isProgressVisible = value;
                NotifyOfPropertyChange(() => IsProgressVisible);
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        public override void Initialize(dynamic parameters)
        {
            CanDownloadAndRestart = true;
            IsProgressVisible = false;
            ProgressValue = 0;
            ErrorMessage = string.Empty;
            NotifyOfPropertyChange(() => NewVersion);
            NotifyOfPropertyChange(() => Changes);
            NotifyOfPropertyChange(() => VersionDate);
        }
        public string NewVersion { get { return Updater.UpdateInfo.Version; } }
        public string Changes { get { return Updater.UpdateInfo.Changes; } }
        public DateTime VersionDate { get { return Updater.UpdateInfo.Date; } }

        private bool _canDownloadAndRestart;
        public bool CanDownloadAndRestart
        {
            get { return _canDownloadAndRestart; }
            set
            {
                _canDownloadAndRestart = value;
                NotifyOfPropertyChange(() => CanDownloadAndRestart);
            }
        }

        public override void Cancel()
        {
            if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
                IsProgressVisible = false;
                ErrorMessage = "Operacja przerwana. Kliknij ponownie przycisk 'Anuluj' aby zamknąć okno.";
            }
            else
            {
                base.Cancel();
            }
        }

        public void DownloadAndRestart()
        {
            CanDownloadAndRestart = false;
            if (Updater.UpdateInfo == null)
            {
                Cancel();
                return;
            }
            ProgressValue = 0;
            IsProgressVisible = true;            
            ErrorMessage = string.Empty;
            Task.Factory.StartNew(() => DownloadUpdate());
        }

        private string _setupFile;
        private void DownloadUpdate()
        {
            _setupFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(Updater.UpdateInfo.Url));
            _webClient.DownloadFileAsync(new Uri(Updater.UpdateInfo.Url), _setupFile);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressValue = e.ProgressPercentage;
        }

        private void OnDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            CanDownloadAndRestart = true;
            if (e.Error == null)
            {
                Updater.UpdateInfo.ExeFile = _setupFile;
                Close();
            }
            else if(!e.Cancelled)
            {               
                ErrorMessage = e.Error.ToString();                
            }
        }
    }
}
