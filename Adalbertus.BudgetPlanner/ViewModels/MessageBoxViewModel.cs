using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class MessageBoxViewModel : BaseDailogViewModel
    {
        public MessageBoxViewModel(IShellViewModel shell, IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
        }

        public string Message { get; set; }
    }
}
