using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public abstract class WizardPageViewModel<TModel> : Screen
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public new WizardShellViewModel<TModel> Parent
        {
            get
            {
                return base.Parent as WizardShellViewModel<TModel>;
            }
            set
            {
                base.Parent = value;
            }
        }
        
        public TModel Model { get; protected set; }
        public void LoadModel(TModel model)
        {
            Model = model;
            OnModelLoaded();
        }

        protected virtual void OnModelLoaded()
        {
        }

        public string Name { get; protected set; }
        
        public bool CanMoveNext
        {
            get { return ValidateMoveNext(); }
        }

        public bool CanMoveBack
        {
            get { return ValidateMoveBack(); }
        }

        public bool CanFinish
        {
            get { return ValidateFinish(); }
        }

        private string _nextPageName;
        public string NextPageName
        {
            get { return _nextPageName; }
            set
            {
                _nextPageName = value;
                NotifyOfPropertyChange(() => NextPageName);
                NotifyOfPropertyChange(() => CanMoveNext);
            }
        }

        private string _backPageName;
        public string BackPageName
        {
            get { return _backPageName; }
            set
            {
                _backPageName = value;
                NotifyOfPropertyChange(() => BackPageName);
                NotifyOfPropertyChange(() => CanMoveBack);
            }
        }

        public WizardPageViewModel()
        {
            Name = GetType().Name;
        }

        public virtual void MoveBack()
        {
        }

        public virtual void MoveNext()
        {
        }

        public virtual void Finish()
        {
        }

        protected virtual bool ValidateMoveNext()
        {
            return !string.IsNullOrWhiteSpace(NextPageName);
        }

        protected virtual bool ValidateMoveBack()
        {
            return !string.IsNullOrWhiteSpace(BackPageName);
        }

        protected virtual bool ValidateFinish()
        {
            return true;
        }

        public virtual void OnActivating()
        {
        }

        public virtual void OnActivated()
        {
        }
    }
}
