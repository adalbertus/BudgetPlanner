using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class CalculatorViewModel : Screen
    {
        public BindableCollectionExt<CalculatorItemVM> Items { get; set; }

        public CalculatorViewModel()
        {
            Items = new BindableCollectionExt<CalculatorItemVM>();
            Items.PropertyChanged += delegate { NotifyOfPropertyChange(() => TotalValue); };
        }

        public decimal TotalValue { get { return CalculateValue(); } }

        private decimal _value;

        public decimal Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        private CaluculatorOperator _operator;

        public CaluculatorOperator Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
                NotifyOfPropertyChange(() => Operator);
            }
        }

        private decimal CalculateValue()
        {
            decimal result = 0;
            var calulatorItems = new Queue<CalculatorItemVM>(Items);

            if (!calulatorItems.Any())
            {
                return result;
            }

            var leftItem = calulatorItems.Dequeue();
            result = leftItem.Value;
            while (calulatorItems.Any())
            {
                var rightItem = calulatorItems.Dequeue();
                switch (leftItem.Operator)
                {
                    case CaluculatorOperator.Add:
                        result += rightItem.Value;
                        break;
                    case CaluculatorOperator.Substract:
                        result -= rightItem.Value;
                        break;
                    case CaluculatorOperator.Multiply:
                        result *= rightItem.Value;
                        break;
                    case CaluculatorOperator.Divide:
                        if (rightItem.Value != 0)
                        {
                            result -= rightItem.Value;
                        }
                        break;
                    case CaluculatorOperator.Result:
                        return result;
                }

                leftItem = rightItem;
            }
            return result;
        }

        public void AddToCalculator(string operatorName)
        {
            var calculatorOperator = CaluculatorOperator.Add;

            switch (operatorName)
            {
                case "+":
                    calculatorOperator = CaluculatorOperator.Add;
                    break;
                case "-":
                    calculatorOperator = CaluculatorOperator.Substract;
                    break;
                case "/":
                    calculatorOperator = CaluculatorOperator.Divide;
                    break;
                case "*":
                    calculatorOperator = CaluculatorOperator.Multiply;
                    break;
                case "=":
                    calculatorOperator = CaluculatorOperator.Result;
                    break;
            }
            Items.Add(new CalculatorItemVM { Value = this.Value, Operator = calculatorOperator });
            NotifyOfPropertyChange(() => TotalValue);
        }

        public void ClearCalculator()
        {
            Items.Clear();
        }
    }
}
