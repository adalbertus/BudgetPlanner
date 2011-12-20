using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Models
{
    public enum CalculatorValueType
    {
        BudgetExpenses,
        BudgetExpensesOfFlowType,
        BudgetExpensesOfFlowTypeAndDescription,
        BudgetIncomesValue,
        BudgetIncomesValueOfType,
        UserValue,
        CalculatorEquationValue,
        Operator,
    }

    public enum CalculatorOperatorType
    {
        Add,
        Substract,
        Multiply,
        Divide,
        //Percent,
        None,
    }

    [PetaPoco.TableName("BudgetCalculatorItem")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class BudgetCalculatorItem : Entity
    {
        [PetaPoco.Column]
        public string Name { get; set; }
        [PetaPoco.Column]
        public int Position { get; set; }

        [PetaPoco.Column]
        public string ValueTypeName {
            get { return ValueType.ToString(); }
            set { ValueType = (CalculatorValueType)Enum.Parse(typeof(CalculatorValueType), value); }
        }
        public CalculatorValueType ValueType { get; set; }

        [PetaPoco.Column]
        public string OperatorTypeName
        {
            get { return OperatorType.ToString(); }
            set { OperatorType = (CalculatorOperatorType)Enum.Parse(typeof(CalculatorOperatorType), value); }
        }
        public CalculatorOperatorType OperatorType { get; set; }
        
        [PetaPoco.Column]
        public int ForeignId { get; set; }
        
         [PetaPoco.Column]
        public decimal? Value { get; set; }

        [PetaPoco.Column]
        public int BudgetCalculatorEquationId { get { return Equation.Id; } set { } }
        public BudgetCalculatorEquation Equation { get; set; }

        public Func<decimal> Evaluator;

        public decimal? CalculateValue()
        {
            if ((ValueType != CalculatorValueType.Operator) && (OperatorType == CalculatorOperatorType.None))
            {
                if (Evaluator != null)
                {
                    return Evaluator.Invoke();
                }
                else
                {
                    return Value;
                }
            }
            return null;
        }

        public BudgetCalculatorItem()
        {
            ValueType = CalculatorValueType.UserValue;
            OperatorType = CalculatorOperatorType.None;
        }
    }
}
