using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public enum CaluculatorOperator
    {
        Add,
        Substract,
        Multiply,
        Divide,
        Result
    }

    public class CalculatorItemVM : PropertyChangedBase
    {
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

        public bool IsAddOperator
        {
            get
            {
                return _operator == CaluculatorOperator.Add;
            }
            set
            {
                Operator = CaluculatorOperator.Add;
            }
        }

        public bool IsSubstractOperator
        {
            get
            {
                return _operator == CaluculatorOperator.Substract;
            }
            set
            {
                Operator = CaluculatorOperator.Substract;
            }
        }

        public bool IsDivideOperator
        {
            get
            {
                return _operator == CaluculatorOperator.Divide;
            }
            set
            {
                Operator = CaluculatorOperator.Divide;
            }
        }

        public bool IsMultiplyOperator
        {
            get
            {
                return _operator == CaluculatorOperator.Multiply;
            }
            set
            {
                Operator = CaluculatorOperator.Multiply;
            }
        }

        public bool IsResultOperator
        {
            get
            {
                return _operator == CaluculatorOperator.Result;
            }
            set
            {
                Operator = CaluculatorOperator.Result;
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
                NotifyOfPropertyChange(() => IsAddOperator);
                NotifyOfPropertyChange(() => IsSubstractOperator);
                NotifyOfPropertyChange(() => IsDivideOperator);
                NotifyOfPropertyChange(() => IsMultiplyOperator);
                NotifyOfPropertyChange(() => IsResultOperator);
            }
        }

        public CalculatorItemVM()
        {
            Operator = CaluculatorOperator.Add;
        }
    }
}
