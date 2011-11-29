using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using System.Windows;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public abstract class BaseDailogViewModel : BaseViewModel, IDialog
    {
        public BaseDailogViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            State = DialogState.None;
        }

        public bool IsQuestionImageVisible
        {
            get { return Image == MessageBoxImage.Question; }
        }

        public bool IsInformationImageVisible
        {
            get { return Image == MessageBoxImage.Information; }
        }

        public bool IsWarningImageVisible
        {
            get { return Image == MessageBoxImage.Warning; }
        }

        public bool IsErrorImageVisible
        {
            get { return Image == MessageBoxImage.Error; }
        }

        public bool IsCancelButtonVisible
        {
            get
            {
                return (Button == MessageBoxButton.OKCancel) || (Button == MessageBoxButton.YesNoCancel);
            }
        }

        public bool IsOKButtonVisible
        {
            get
            {
                return (Button == MessageBoxButton.OKCancel) || (Button == MessageBoxButton.OK);
            }
        }

        private MessageBoxButton _button;

        public MessageBoxButton Button
        {
            get { return _button; }
            set
            {
                _button = value;
                NotifyOfPropertyChange(() => IsCancelButtonVisible);
                NotifyOfPropertyChange(() => IsOKButtonVisible);
            }
        }

        private MessageBoxImage _image;

        public MessageBoxImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyOfPropertyChange(() => IsQuestionImageVisible);
                NotifyOfPropertyChange(() => IsInformationImageVisible);
                NotifyOfPropertyChange(() => IsWarningImageVisible);
                NotifyOfPropertyChange(() => IsErrorImageVisible);
            }
        }

        public DialogState State { get; private set; }

        public object Result { get; private set; }
        public System.Action OKCallback { get; set; }
        public System.Action CancelCallback { get; set; }
        public System.Action AfterCloseCallback { get; set; }

        public virtual void Initialize(dynamic parameters)
        {

        }

        protected void CleanUp()
        {
            OnDeactivate(true);
            if (AfterCloseCallback != null)
            {
                Execute.OnUIThread(AfterCloseCallback);
            }
        }

        public virtual void Close()
        {
            State = DialogState.OK;
            if (OKCallback != null)
            {
                Execute.OnUIThread(OKCallback);
            }
            CleanUp();
        }

        public virtual void Cancel()
        {
            State = DialogState.Cancel;
            if (CancelCallback != null)
            {
                Execute.OnUIThread(CancelCallback);
            }
            CleanUp();
        }
    }
}
