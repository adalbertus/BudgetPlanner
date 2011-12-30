using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Database;
using ILCalc;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetCalculatorEvaluator
    {
        public Budget Budget { get; set; }
        public ICachedService CachedService { get; private set; }
        private CalcContext<decimal> _calculator;
        public IEnumerable<BudgetCalculatorEquation> Equations { get { return CachedService.GetAllEquations(); } }

        public BudgetCalculatorEvaluator(ICachedService cashedService)
        {
            CachedService = cashedService;
            _calculator = new CalcContext<decimal>();
        }

        public decimal? Calculate(BudgetCalculatorEquation equation)
        {
            StringBuilder expressionBuilder = new StringBuilder();

            foreach (var item in equation.Items)
            {
                switch (item.ValueType)
                {
                    case CalculatorValueType.UserValue:
                        expressionBuilder.Append(item.Value.GetValueOrDefault(0));
                        break;
                    case CalculatorValueType.Operator:
                        expressionBuilder.AppendFormat(" {0} ", item.OperatorType.ToMath());
                        break;
                    default:
                        if (item.Evaluator == null)
                        {
                            AttachEvaluator(item);
                        }
                        var value = item.CalculateValue();
                        if (value.HasValue)
                        {
                            expressionBuilder.Append(value.Value);
                        }
                        break;
                }
            }

            var expression = expressionBuilder.ToString().Replace(",", ".");
            try
            {
                var result = _calculator.Evaluate(expression);
                return result;
            }
            catch (SyntaxException)
            {
                return null;
            }            
        }
        public void Refresh(BudgetCalculatorEquation equation)
        {
            AttachEvaluator(equation);
            UpdateForeignDescriptions(equation);
        }

        public void AttachEvaluator(BudgetCalculatorEquation equation)
        {
            equation.Evaluator = () => Calculate(equation);
            equation.Items.ForEach(x => AttachEvaluator(x));
        }
        
        public void AttachEvaluator(BudgetCalculatorItem calculatorItem)
        {
            
            CashFlow cashFlow = null;
            CashFlowGroup cashFlowGroup = null;
            switch (calculatorItem.ValueType)
            {
                case CalculatorValueType.BudgetTotalRevenuesValue:
                    calculatorItem.Evaluator = () => Budget.TotalSumOfRevenues;
                    break;
                case CalculatorValueType.BudgetIncomesValue:
                case CalculatorValueType.BudgetIncomesValueOfType:
                    var income = CachedService.GetAllIncomes().FirstOrDefault(x => x.Id == calculatorItem.ForeignId);
                    calculatorItem.Evaluator = () => GetSumOfBudgetIncomes(income);
                    break;
                case CalculatorValueType.BudgetSavingsValue:
                case CalculatorValueType.BudgetSavingsValueOfType:
                    var saving = CachedService.GetAllSavings().FirstOrDefault(x => x.Id == calculatorItem.ForeignId);
                    calculatorItem.Evaluator = () => GetSumOfBudgetSavings(saving);
                    break;
                case CalculatorValueType.BudgetExpensesValueOfType:
                    cashFlow = CachedService.GetAllCashFlows().FirstOrDefault(x => x.Id == calculatorItem.ForeignId);
                    if (cashFlow == null)
                    {
                        throw new NullReferenceException(string.Format("Błąd obliczania równania {0}. Brak kategorii.", calculatorItem.Equation.Name));
                    }
                    calculatorItem.Evaluator = () => GetSumOfBudgetExpenses(cashFlow);
                    break;
                case CalculatorValueType.BudgetExpensesWithDescription:
                    calculatorItem.Evaluator = () => GetSumOfBudgetExpensesWithDescription(calculatorItem.Text);
                    break;
                case CalculatorValueType.CalculatorEquationValue:
                    var calculatorEquation = Equations.FirstOrDefault(x => x.Id == calculatorItem.ForeignId);
                    if (calculatorEquation == null)
                    {
                        throw new NullReferenceException(string.Format("Nie udało się odnaleźć równania powiązanego z równaniem: {0}", calculatorItem.Name));
                    }
                    calculatorItem.Evaluator = () => Calculate(calculatorEquation);
                    break;
                case CalculatorValueType.BudgetPlanValue:
                    calculatorItem.Evaluator = () => Budget.TotalBudgetPlanValue;
                    break;
                case CalculatorValueType.BudgetPlanValueOfCategory:
                    cashFlow = CachedService.GetAllCashFlows().FirstOrDefault(x => x.Id == calculatorItem.ForeignId);
                    if (cashFlow == null)
                    {
                        throw new NullReferenceException(string.Format("Błąd obliczania równania {0}. Brak kategorii.", calculatorItem.Equation.Name));
                    }
                    calculatorItem.Evaluator = () => GetSumOfBudgetPlansOfCategory(cashFlow);
                    break;
                case CalculatorValueType.BudgetPlanValueOfGroup:
                    cashFlowGroup = CachedService.GetAllCashFlowGroups().FirstOrDefault(x => x.Id == calculatorItem.ForeignId);
                    if (cashFlowGroup == null)
                    {
                        throw new NullReferenceException(string.Format("Błąd obliczania równania {0}. Brak grupy.", calculatorItem.Equation.Name));
                    }
                    calculatorItem.Evaluator = () => GetSumOfBudgetPlansOfGroup(cashFlowGroup);
                    break;
            }
        }

        public void UpdateForeignDescriptions(BudgetCalculatorEquation equation)
        {
            foreach (var item in equation.Items)
            {
                switch (item.ValueType)
                {
                    case CalculatorValueType.CalculatorEquationValue:
                        var eq = CachedService.GetAllEquations().FirstOrDefault(x => x.Id == item.ForeignId);
                        if (eq != null)
                        {
                            item.ForeignDescription = eq.Name;
                        }
                        break;
                    case CalculatorValueType.BudgetExpensesValueOfType:
                        var cf = CachedService.GetAllCashFlows().FirstOrDefault(x => x.Id == item.ForeignId);
                        if (cf != null)
                        {
                            item.ForeignDescription = cf.Name;
                        }
                        break;
                }
            }
        }

        #region Equation evaluators
        private decimal GetSumOfBudgetSavings(Saving saving)
        {
            if (saving == null)
            {
                return Budget.SumOfRevenueSavings;
            }
            return Budget.SavingValues
                .Where(x => (x.Expense != null) && (x.SavingId == saving.Id))
                .Sum(x => x.BudgetValue);
        }
        
        private decimal GetSumOfBudgetIncomes(Income income)
        {
            if (income == null)
            {
                return Budget.SumOfRevenueIncomes;
            }
            return Budget.IncomeValues.Where(x => x.IncomeId == income.Id).Sum(x => x.Value);
        }

        private decimal GetSumOfBudgetExpenses(CashFlow cashFlow = null)
        {
            if (cashFlow == null)
            {
                return Budget.TotalExpenseValue;
            }
            else
            {
                return Budget.Expenses.Where(x => x.Flow.Equals(cashFlow)).Sum(x => x.Value);
            }
        }

        private decimal GetSumOfBudgetExpensesWithDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return Budget.TotalExpenseValue;
            }
            return Budget.Expenses.Where(x => x.Description.Contains(description, false)).Sum(x => x.Value);
        }

        private decimal GetSumOfBudgetPlansOfCategory(CashFlow cashFlow)
        {
            return Budget.BudgetPlanItems.Where(x => x.CashFlowId == cashFlow.Id).Sum(x => x.Value);
        }

        private decimal GetSumOfBudgetPlansOfGroup(CashFlowGroup cashFlowGroup)
        {
            return Budget.BudgetPlanItems.Where(x => x.CashFlow.CashFlowGroupId == cashFlowGroup.Id).Sum(x => x.Value);
        }
        #endregion Equation evaluators
    }
}
