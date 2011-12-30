using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Core
{
    public enum WizardStatus
    {
        OK,
        Cancel
    }

    public class WizardEvent<TModel>
    {
        public WizardStatus Status { get; set; }
        public TModel Model { get; set; }
    }
}
