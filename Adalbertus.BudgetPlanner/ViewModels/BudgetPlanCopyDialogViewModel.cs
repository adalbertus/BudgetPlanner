using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetPlanCopyDialogViewModel : BaseDailogViewModel
    {
        public BudgetPlanCopyDialogViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {

        }
    }
}
