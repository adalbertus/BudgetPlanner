using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Extensions;

namespace Adalbertus.BudgetPlanner.Models
{
    public enum CalculatorValueType
    {
        BudgetIncomesValue,
        BudgetIncomesValueOfType,
        BudgetTotalRevenuesValue,
        BudgetSavingsValue,
        BudgetSavingsValueOfType,
        BudgetExpensesValueOfType,
        BudgetExpensesWithDescription,
        BudgetPlanValue,
        BudgetPlanValueOfGroup,
        BudgetPlanValueOfCategory,
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
        public string ValueTypeName
        {
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

        public string Description
        {
            get
            {
                string baseDescription = string.Empty;
                if (ValueType == CalculatorValueType.Operator)
                {
                    baseDescription = OperatorType.ToName();
                }
                else
                {
                    baseDescription = ValueType.ToName();
                }

                //if (!string.IsNullOrWhiteSpace(ForeignDescription))
                //{
                //    return string.Format("{0}: {1}", baseDescription, ForeignDescription);
                //}
                return baseDescription;

            }
        }

        [PetaPoco.Column]
        public int ForeignId { get; set; }
        public string ForeignDescription { get; set; }

        [PetaPoco.Column]
        public decimal? Value { get; set; }

        [PetaPoco.Column]
        public string Text { get; set; }

        [PetaPoco.Column]
        public int BudgetCalculatorEquationId { get { return Equation.Id; } set { } }
        public BudgetCalculatorEquation Equation { get; set; }

        public decimal? CalculatedValue
        {
            get { return CalculateValue();}
        }
        public Func<decimal?> Evaluator;

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

        public BudgetCalculatorItem CreateCopy()
        {
            var copy = new BudgetCalculatorItem
            {
                Id = this.Id,
                Name = this.Name,
                Position = this.Position,
                ValueType = this.ValueType,
                OperatorType = this.OperatorType,
                ForeignId = this.ForeignId,
                Value = this.Value,
                Text = this.Text,
                Equation = this.Equation,
            };
            return copy;
        }
    }
}
