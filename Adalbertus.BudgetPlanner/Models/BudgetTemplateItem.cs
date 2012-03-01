using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Database;

namespace Adalbertus.BudgetPlanner.Models
{
    public enum BudgetTemplateType
    {
        BudgetPlan,
        BudgetExpense
    }

    [PetaPoco.TableName("BudgetTemplateItem")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class BudgetTemplateItem : Entity
    {
        [PetaPoco.Column]
        public string TemplateTypeName
        {
            get { return TemplateType.ToString(); }
            set { TemplateType = (BudgetTemplateType)Enum.Parse(typeof(BudgetTemplateType), value); }
        }
        public BudgetTemplateType TemplateType { get; set; }

        [PetaPoco.Column]
        public int ForeignId { get; set; }

        [PetaPoco.Column]
        public decimal? Value { get; set; }

        private string _description;
        [PetaPoco.Column]
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }


        [PetaPoco.Column]
        public int MonthDay { get; set; }

        [PetaPoco.Column]
        public int RepeatInterval { get; set; }

        [PetaPoco.Column]
        public bool IsActive { get; set; }

        [PetaPoco.Column]
        public DateTime StartDate { get; set; }

        [PetaPoco.Column]
        public DateTime? LastExecutionDate { get; set; }

        public IList<BudgetTemplateHistory> HistoryItems { get; private set; }

        public BudgetTemplateItem()
        {
            HistoryItems = new List<BudgetTemplateHistory>();
        }

        public Entity ApplyToBudget(Budget budget, CashFlow cashFlow, decimal value, string description)
        {
            if (!CheckIfCanBeExecuted(budget))
            {
                return null;
            }

            Entity resultEntity = null;
            switch (TemplateType)
            {
                case BudgetTemplateType.BudgetPlan:
                    resultEntity = budget.AddPlanValue(cashFlow, value, description);
                    break;
                case BudgetTemplateType.BudgetExpense:
                    var expenseDate = budget.DateFrom.SetMonthDay(MonthDay);
                    resultEntity = budget.AddExpense(cashFlow, value, description, expenseDate);
                    break;
            }
            var itemHistory = new BudgetTemplateHistory
            {
                Budget = budget,
                BudgetTemplateItem = this,
                Date = budget.DateFrom.SetMonthDay(MonthDay)
            };
            HistoryItems.Add(itemHistory);
            return resultEntity;
        }

        public bool CheckIfCanBeExecuted(Budget budget)
        {
            if (budget.DateFrom.YearMonthToInt() < StartDate.YearMonthToInt())
            {
                return false;
            }

            var lastHistory = HistoryItems
                .Where(x => (x.Date.YearMonthToInt() >= StartDate.YearMonthToInt()) && (x.Date.YearMonthToInt() <= budget.DateFrom.YearMonthToInt()))
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();
            
            DateTime? lastDate = null;
            if(lastHistory != null)
            {
                lastDate = lastHistory.Date;
            }

            if (lastDate.HasValue)
            {
                return (budget.DateFrom.YearMonthToInt() - lastDate.Value.YearMonthToInt()) % RepeatInterval == 0;
            }
            else
            {
                return (budget.DateFrom.YearMonthToInt() - StartDate.YearMonthToInt()) % RepeatInterval == 0;                
            }

        }
    }
}
