using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BaseViewModel : Screen, IHandle<RefreshEvent>
    {
        public IDatabase Database { get; private set; }
        public IConfigurationManager Configuration { get; private set; }
        public IEventAggregator EventAggregator { get; private set; }
        public ICachedService CachedService { get; private set; }
        public IShellViewModel Shell { get; set; }

        public BaseViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
        {
            Initialize();

            Shell           = shell;
            Database        = database;
            Configuration   = configuration;
            EventAggregator = eventAggregator;
            CachedService   = cashedService;

            EventAggregator.Subscribe(this);
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public virtual void AttachEvents()
        {
        }

        public virtual void DeatachEvents()
        {
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LoadData();
            AttachEvents();
        }

        public virtual void LoadData()
        {
        }

        protected override void OnDeactivate(bool close)
        {
            DeatachEvents();

            base.OnDeactivate(close);
            if (Database != null)
            {
                Database.Dispose();
            }
        }

        protected virtual void Save(Entity entity)
        {
            Diagnostics.Start(entity.GetType().FullName);
            using (var tx = Database.GetTransaction())
            {
                Database.Save(entity);
                tx.Complete();
            }
            PublishRefreshRequest(entity);
            Diagnostics.Stop();
        }

        protected virtual void Delete(Entity entity)
        {
            Diagnostics.Start(entity.GetType().FullName);
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(entity);
                tx.Complete();
            }
            PublishRefreshRequest(entity);
            Diagnostics.Stop();
        }

        #region IHandle<RefreshEvent> Members

        public void Handle(RefreshEvent message)
        {
            OnRefreshRequest(message);
        }

        protected virtual void OnRefreshRequest(RefreshEvent refreshEvent)
        {
        }

        protected void PublishRefreshRequest(Entity entity)
        {
            EventAggregator.Publish(new RefreshEvent(this.GetType().Name, entity));
        }

        #endregion
    }
}
