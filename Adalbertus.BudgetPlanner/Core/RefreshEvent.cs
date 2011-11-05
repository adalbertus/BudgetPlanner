using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Core
{
    public class RefreshEvent
    {
        public string Sender { get; set; }

        public RefreshEvent(string sender)
        {
            Sender = sender;
        }
    }
}
