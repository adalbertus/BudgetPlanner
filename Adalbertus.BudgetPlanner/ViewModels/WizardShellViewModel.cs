using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Extensions;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public abstract class WizardShellViewModel<TModel> : Conductor<Screen>.Collection.OneActive
    {
        public TModel Model { get; set; }

        public WizardShellViewModel()
        {
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

        public void RefreshNavigationUI()
        {
            NotifyOfPropertyChange(() => CanMoveNext);
            NotifyOfPropertyChange(() => CanMoveBack);
            NotifyOfPropertyChange(() => CurrentPageTitle);
        }

        public void LoadData()
        {
            base.OnActivate();

            LoadModel();
            LoadPages();

            if (Items.Any())
            {
                Items.ForEach(x => (x as WizardPageViewModel<TModel>).LoadModel(Model));
                ActivateItem(Items.First());
            }
        }

        protected abstract void LoadModel();
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
            var moveToPage = Items.First(x => (x as WizardPageViewModel<TModel>).Name == name);
            ChangeActiveItem(moveToPage, false);
            RefreshNavigationUI();
        }

        public virtual void Cancel()
        {
        }

        public virtual void Finish()
        {
        }
    }
}
