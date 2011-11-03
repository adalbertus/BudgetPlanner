using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BaseViewModel : Screen
    {
        public IDatabase Database { get; private set; }
        public IConfiguration Configuration { get; private set; }

        public BaseViewModel(IDatabase database, IConfiguration configuration)
        {            
            Initialize();
            Database = database;
            Configuration = configuration;
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            if (Database != null)
            {
                Database.Dispose();
            }
        }
    }
}
