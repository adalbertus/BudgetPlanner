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
        public IConfiguration Configuration { get; private set; }
        public IEventAggregator EventAggregator { get; private set; }
        public ICachedService CachedService { get; private set; }

        public BaseViewModel(IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
        {
            Initialize();

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

        protected virtual void LoadData()
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

        protected void Save(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(entity);
                tx.Complete();
            }
            PublishRefreshRequest(entity);
        }

        protected void Delete(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(entity);
                tx.Complete();
            }
            PublishRefreshRequest(entity);
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
