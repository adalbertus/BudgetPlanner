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
                string executionInverval = string.Format("Każdego {0} dnia miesiąca, co {1} mcy", WrappedItem.MonthDay, WrappedItem.RepeatInterval);
                if (WrappedItem.Description.IsNullOrWhiteSpace())
                {
                    return string.Format("[{0}]", executionInverval);
                }
                return string.Format("{0} [{1}]", WrappedItem.Description, executionInverval);
            }
        }

        public string Status
        {
            get
            {
                if (WrappedItem.LastExecutionDate.HasValue)
                {
                    return string.Format("Ostatnio wykonany: {0}", WrappedItem.LastExecutionDate.Value);
                }
                return "Nigdy nie wykonany";
            }
        }


    }
}
