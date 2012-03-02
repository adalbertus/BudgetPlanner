using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Core
{
    public class RefreshEvent
    {
        public string Sender { get; set; }
        public object ChangedEntity { get; set; }

        public RefreshEvent(string sender, object changedEntity)
        {
            Sender        = sender;
            ChangedEntity = changedEntity;
        }
    }
}
