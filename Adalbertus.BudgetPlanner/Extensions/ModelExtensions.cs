using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public static class ModelExtensions
    {
        public static string ToName(this CalculatorValueType valueType)
        {
            switch(valueType)
            {
                case CalculatorValueType.BudgetIncomesValue:
                    return "Suma wszystkich dochodów";
                case CalculatorValueType.BudgetIncomesValueOfType:
                    return "Suma dochodów typu";
                case CalculatorValueType.BudgetTotalRevenuesValue:
                    return "Suma wszystkich wpływów budżetowych";
                case CalculatorValueType.BudgetSavingsValue:
                    return "Suma wszystkich zewnętrznych źródeł";
                case CalculatorValueType.BudgetSavingsValueOfType:
                    return "Suma zewnętrznych źródłeł typu";
                case CalculatorValueType.BudgetExpensesValueOfType:
                    return "Suma wydatków określonego typu";
                case CalculatorValueType.BudgetExpensesWithDescription:
                    return "Suma wydatków zawierających opis";
                case CalculatorValueType.BudgetPlanValue:
                    return "Suma całego planu budżetowego";
                case CalculatorValueType.BudgetPlanValueOfCategory:
                    return "Suma planu budżetowego dla kategorii";
                case CalculatorValueType.BudgetPlanValueOfGroup:
                    return "Suma planu budżetowego dla grupy";
                case CalculatorValueType.CalculatorEquationValue:
                    return "Wynik innego równania";
                case CalculatorValueType.UserValue:
                    return "Wartość wprowadzona ręcznie";
                case CalculatorValueType.Operator:
                    return "Operator";
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Undefined name for: {0}", valueType.ToString()));
            }
        }
        public static string ToName(this CalculatorOperatorType operatorType)
        {
            switch (operatorType)
            {
                case CalculatorOperatorType.None:
                    return string.Empty;
                case CalculatorOperatorType.Add:
                    return "Dodaj";
                case CalculatorOperatorType.Substract:
                    return "Odejmij";
                case CalculatorOperatorType.Multiply:
                    return "Pomnóż";
                case CalculatorOperatorType.Divide:
                    return "Podziel";
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Undefined name for: {0}", operatorType.ToString()));
            }
        }
        public static string ToMath(this CalculatorOperatorType operatorType)
        {
            switch (operatorType)
            {
                case CalculatorOperatorType.None:
                    return string.Empty;
                case CalculatorOperatorType.Add:
                    return "+";
                case CalculatorOperatorType.Substract:
                    return "-";
                case CalculatorOperatorType.Multiply:
                    return "*";
                case CalculatorOperatorType.Divide:
                    return "/";
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Undefined name for: {0}", operatorType.ToString()));
            }
        }
    
    }
}
