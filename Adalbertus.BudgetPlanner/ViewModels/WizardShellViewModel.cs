using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public abstract class WizardShellViewModel<TModel> : Conductor<Screen>.Collection.OneActive, IDialog<TModel>
    {
        public TModel Model { get; set; }
        public IDatabase Database { get; private set; }
        public IConfigurationManager Configuration { get; private set; }
        public IEventAggregator EventAggregator { get; private set; }
        public ICachedService CachedService { get; private set; }

        public WizardShellViewModel(IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
        {
            Database = database;
            Configuration = configuration;
            EventAggregator = eventAggregator;
            CachedService = cashedService;
        }

        public WizardPageViewModel<TModel> CurrentPage
        {
            get { return ActiveItem as WizardPageViewModel<TModel>; }
        }

        public string CurrentPageTitle
        {
            get
            {
                if (CurrentPage != null)
                {
                    return CurrentPage.Title;
                }
                return string.Empty;
            }
        }

        public bool CanMoveNext
        {
            get
            {
                var activePage = ActiveItem as WizardPageViewModel<TModel>;
                if (activePage == null)
                {
                    return false;
                }

                return activePage.CanMoveNext;
            }
        }

        public bool CanMoveBack
        {
            get
            {
                var activePage = ActiveItem as WizardPageViewModel<TModel>;
                if (activePage == null)
                {
                    return false;
                }

                return activePage.CanMoveBack;
            }
        }

        public virtual bool CanFinish
        {
            get
            {
                var activePage = ActiveItem as WizardPageViewModel<TModel>;
                if (activePage == null)
                {
                    return false;
                }

                return activePage.CanFinish;
            }
        }

        public virtual void RefreshNavigationUI()
        {
            NotifyOfPropertyChange(() => CanMoveNext);
            NotifyOfPropertyChange(() => CanMoveBack);
            NotifyOfPropertyChange(() => CanFinish);
            NotifyOfPropertyChange(() => CurrentPageTitle);
        }

        public virtual void LoadData(TModel model)
        {
            base.OnActivate();

            Model = model;
            LoadPages();

            if (Items.Any())
            {
                Items.ForEach(x =>
                {
                    (x as WizardPageViewModel<TModel>).LoadModel(Model);
                    x.Refresh();
                });
                ActivateItem(Items.First());
            }
        }

        protected abstract void LoadPages();

        public void MoveBack()
        {
            if (CurrentPage.CanMoveBack)
            {
                CurrentPage.MoveBack();
                MoveToPageName(CurrentPage.BackPageName);
            }
        }

        public void MoveNext()
        {
            if (CurrentPage.CanMoveNext)
            {
                CurrentPage.MoveNext();
                MoveToPageName(CurrentPage.NextPageName);
            }
        }

        private void MoveToPageName(string name)
        {
            var moveToPage = Items.First(x => (x as WizardPageViewModel<TModel>).Name == name) as WizardPageViewModel<TModel>;
            moveToPage.OnActivating();
            ChangeActiveItem(moveToPage, false);
            moveToPage.OnActivated();
            RefreshNavigationUI();
        }

        protected virtual void CleanUp()
        {
            OnDeactivate(true);
            if (AfterCloseCallback != null)
            {
                Execute.OnUIThread(AfterCloseCallback);
            }
        }
        public virtual void Cancel()
        {
            EventAggregator.Publish(new WizardEvent<TModel>
                {
                    Model = this.Model,
                    Status = WizardStatus.Cancel
                });
            State = DialogState.Cancel;
            if (CancelCallback != null)
            {
                Execute.OnUIThread(CancelCallback);
            }
            CleanUp();
        }

        public virtual void Finish()
        {
            if (CanFinish)
            {
                CurrentPage.Finish();
                EventAggregator.Publish(new WizardEvent<TModel>
                {
                    Model = this.Model,
                    Status = WizardStatus.OK
                });
            }
            State = DialogState.OK;
            if (OKCallback != null)
            {
                Execute.OnUIThread(OKCallback);
            }
            CleanUp();
        }

        #region IDialog Members

        public DialogState State { get; private set; }

        public TModel Result { get; private set; }
        public System.Action OKCallback { get; set; }
        public System.Action CancelCallback { get; set; }
        public System.Action AfterCloseCallback { get; set; }

        public virtual void Initialize(dynamic parameters)
        {
        }

        public virtual void LoadData()
        {
        }

        #endregion
    }
}
