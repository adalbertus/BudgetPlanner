using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILCalc;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("BudgetCalculatorEquation")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class BudgetCalculatorEquation : Entity
    {
        [PetaPoco.Column]
        public string Name { get; set; }
        [PetaPoco.Column]
        public bool IsVisible { get; set; }
        [PetaPoco.Column]
        public int Position { get; set; }
        public BindableCollectionExt<BudgetCalculatorItem> Items { get; private set; }
        public decimal Value { get; private set; }

        private CalcContext<decimal> _calculator;

        public BudgetCalculatorEquation()
        {
            Items = new BindableCollectionExt<BudgetCalculatorItem>();
            _calculator = new CalcContext<decimal>();
        }

        public decimal CalculateValue()
        {
            StringBuilder expressionBuilder = new StringBuilder();
            var itemsQueue = new Queue<BudgetCalculatorItem>(Items);

            foreach (var item in Items)
            {
                switch (item.ValueType)
                {
                    case CalculatorValueType.BudgetExpenses:
                    case CalculatorValueType.BudgetExpensesOfFlowType:
                    case CalculatorValueType.BudgetExpensesOfFlowTypeAndDescription:
                    case CalculatorValueType.BudgetIncomesValue:
                    case CalculatorValueType.BudgetIncomesValueOfType:
                    case CalculatorValueType.UserValue:
                    case CalculatorValueType.CalculatorEquationValue:
                        var value = item.CalculateValue();
                        if (value.HasValue)
                        {
                            expressionBuilder.Append(value.Value);
                        }
                        break;
                    case CalculatorValueType.Operator:
                        switch (item.OperatorType)
                        {
                            //case CalculatorOperatorType.Percent:
                            //    break;
                            case CalculatorOperatorType.Add:
                                expressionBuilder.Append(" + ");
                                break;
                            case CalculatorOperatorType.Substract:
                                expressionBuilder.Append(" - ");
                                break;
                            case CalculatorOperatorType.Multiply:
                                expressionBuilder.Append(" * ");
                                break;
                            case CalculatorOperatorType.Divide:
                                expressionBuilder.Append(" / ");
                                break;
                        }
                        break;
                }
            }

            var expression = expressionBuilder.ToString().Replace(",", ".");
            Value = _calculator.Evaluate(expression);
            return Value;
        }

        public BudgetCalculatorItem AddItem(string name, CalculatorValueType valueType, CalculatorOperatorType operatorType = CalculatorOperatorType.None, decimal? value = null, int foreignId = 0)
        {
            var item = new BudgetCalculatorItem
            {
                Name = name,
                ValueType = valueType,
                OperatorType = operatorType,
                ForeignId = foreignId,
                Value = value,
                Equation = this,
            };
            Items.Add(item);
            return item;
        }
    }
}
