using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("Budget")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class Budget : Entity
    {
        [PetaPoco.Column]        
        public virtual DateTime DateFrom { get; set; }
        [PetaPoco.Column]
        public virtual DateTime DateTo { get; set; }

        private decimal _transferedValue;
        [PetaPoco.Column]       
        public decimal TransferedValue
        {
            get { return _transferedValue; }
            set
            {
                _transferedValue = value;
                NotifyOfPropertyChange(() => TransferedValue);
            }
        }

        public virtual IList<IncomeValue> IncomeValues { get; private set; }
        public virtual IList<SavingValue> SavingValues { get; private set; }
        public virtual IList<BudgetPlan> BudgetPlanItems { get; private set; }
        public virtual BindableCollectionExt<Expense> Expenses { get; private set; }

        public decimal TotalBudgetValue
        {
            get { return TransferedValue + TotalSumOfRevenues; }
        }

        public decimal RealBudgetBilans
        {
            get { return TotalBudgetValue - TotalExpenseValue; }
        }

        public decimal TotalBudgetBilans
        {
            get { return TotalBudgetValue - TotalBudgetPlanValue; }
        }

        public decimal TotalSumOfRevenues
        {
            get { return SumOfRevenueIncomes + SumOfRevenueSavings; }
        }

        public decimal SumOfRevenueIncomes
        {
            get { return IncomeValues.Sum(x => x.Value); }
        }

        public decimal SumOfRevenueSavings
        {
            get { return SavingValues.Where(x => x.Expense != null).Sum(x => x.BudgetValue); }
        }

        public decimal TotalBudgetPlanValue
        {
            get { return BudgetPlanItems.Sum(x => x.Value); }
        }

        public decimal TotalExpenseValue
        {
            get { return Expenses.Sum(x => x.Value); }
        }

        public Budget()
        {
            IncomeValues = new BindableCollectionExt<IncomeValue>();
            SavingValues = new BindableCollectionExt<SavingValue>();
            BudgetPlanItems = new BindableCollectionExt<BudgetPlan>();
            Expenses = new BindableCollectionExt<Expense>();
        }

        public static Budget CreateEmptyForDate(DateTime contextDate, IEnumerable<CashFlow> cashFlows)
        {
            DateTime dateFrom = new DateTime(contextDate.Year, contextDate.Month, 1);
            DateTime dateTo = dateFrom.AddMonths(1).AddDays(-1);
            Budget budget = new Budget
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
            };

            return budget;
        }

        public virtual void RemoveBudgetPlanItems(CashFlow cashFlow)
        {
            BudgetPlanItems.Clear();
        }

        public virtual IncomeValue AddIncomeValue(Income income, decimal value, DateTime date, string description)
        {
            IncomeValue IncomeValue = income.AddIncomeValue(this, value, date, description);
            IncomeValues.Add(IncomeValue);
            return IncomeValue;
        }

        public virtual void RemoveIncomeValue(IncomeValue IncomeValue)
        {
            IncomeValues.Remove(IncomeValue);
        }

        public virtual SavingValue WithdrawSavingValue(Saving saving, decimal value, DateTime date, string description)
        {
            SavingValue SavingValue = saving.Withdraw(value, date, description, this);
            SavingValues.Add(SavingValue);
            return SavingValue;
        }

        public virtual void CancelWithdrawSavingValue(SavingValue savingValue)
        {
            savingValue.Saving.Values.Remove(savingValue);
            SavingValues.Remove(savingValue);
        }

        public virtual Expense AddExpense(CashFlow flow, decimal value, string description, DateTime date)
        {
            Expense expense = Expense.CreateExpense(this, flow, value, description, date);
            Expenses.Add(expense);
            return expense;
        }

        public virtual void RemoveExpense(Expense expense)
        {
            Expenses.Remove(expense);
        }

        public BudgetPlan AddPlanValue(CashFlow cashFlow, decimal value, string description)
        {
            var budgetPlan = new BudgetPlan
                        {
                            Budget = this,
                            CashFlow = cashFlow,
                            Value = value,
                            Description = description
                        };
            BudgetPlanItems.Add(budgetPlan);
            return budgetPlan;
        }
    }
}
