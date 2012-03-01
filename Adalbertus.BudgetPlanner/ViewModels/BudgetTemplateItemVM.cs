using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Extensions;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetTemplateItemVM : PropertyChangedBase
    {
        public BudgetTemplateItem WrappedItem { get; private set; }
        public Func<int, string> CashFlowNameSelector { get; private set; }
        public BudgetTemplateItemVM(BudgetTemplateItem item, Func<int, string> cashFlowNameSelector)
        {
            WrappedItem = item;
            CashFlowNameSelector = cashFlowNameSelector;
        }

        public bool IsElementActive
        {
            get { return WrappedItem.IsActive; }
            set
            {
                WrappedItem.IsActive = value;
                NotifyOfPropertyChange(() => IsElementActive);
            }
        }

        public string TypeName
        {
            get
            {
                switch (WrappedItem.TemplateType)
                {
                    case BudgetTemplateType.BudgetExpense:
                        return "Realizacja planu";
                    case BudgetTemplateType.BudgetPlan:
                        return "Plan budżetowy";
                    default:
                        throw new IndexOutOfRangeException("BudgetTemplateItemVM: WrappedItem.TemplateType");
                }
            }
        }

        public string CashFlowName { get { return CashFlowNameSelector(WrappedItem.ForeignId); } }

        public decimal? Value
        {
            get { return WrappedItem.Value; }
            set
            {
                WrappedItem.Value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        public string DetailedDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Od {0}, każdego {1} dnia ",
                        WrappedItem.StartDate.ToShortDateString(),
                        WrappedItem.MonthDay);
                if (WrappedItem.RepeatInterval == 1)
                {
                    sb.AppendFormat("miesiąca");
                }
                else
                {
                    sb.AppendFormat(", co {0} miesiące", WrappedItem.RepeatInterval);
                }

                if (WrappedItem.Description.IsNullOrWhiteSpace())
                {
                    return sb.ToString();
                }
                return string.Format("{0}\r\n{1}", WrappedItem.Description, sb.ToString());
            }
        }

        public string Status
        {
            get
            {
                if (WrappedItem.HistoryItems.Any())
                {
                    var lastDate = WrappedItem.HistoryItems.OrderByDescending(x => x.Date).First().Date;
                    
                    return string.Format("Wykonany {0}", lastDate.ToShortDateString());
                    
                }
                return "Nigdy nie wykonany";
            }
        }


    }
}
