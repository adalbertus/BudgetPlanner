using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILCalc;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("BudgetCalculatorEquation")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class BudgetCalculatorEquation : Entity
    {
        private string _name;
        [PetaPoco.Column]
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        private bool _isVisible;
        [PetaPoco.Column]
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        private int _position;
        [PetaPoco.Column]
        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                NotifyOfPropertyChange(() => Position);
            }
        }

        public BindableCollectionExt<BudgetCalculatorItem> Items { get; private set; }
        public string MathString { get { return CreateStringEquation(); } }

        public decimal? Value {
            get
            {
                if (Evaluator == null)
                {
                    return null;
                }
                return Evaluator.Invoke();
            }
        }
        public Func<decimal?> Evaluator { get; set; }


        public BudgetCalculatorEquation()
        {
            Items = new BindableCollectionExt<BudgetCalculatorItem>();
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

        public override string ToString()
        {
            return Name;
        }

        public string CreateStringEquation()
        {
            var equation = new StringBuilder();
            foreach (var item in Items)
            {
                switch(item.ValueType)
                {
                    case CalculatorValueType.CalculatorEquationValue:
                        equation.AppendFormat(" Równanie({0}) ", item.Equation.Name);
                        break;
                    case CalculatorValueType.UserValue:
                        equation.AppendFormat(" {0} ", item.Value);
                        break;
                    case CalculatorValueType.Operator:
                        equation.AppendFormat(" {0} ", item.OperatorType.ToMath());
                        break;
                    default:
                        equation.AppendFormat(" {0} ", item.ValueType.ToName());
                        break;
                }
            }
            return equation.ToString();
        }

        public BudgetCalculatorEquation CreateCopy()
        {
            var copy = new BudgetCalculatorEquation
            {
                Id = this.Id,
                Name = this.Name,
                IsVisible = this.IsVisible,
                Position = this.Position,
            };

            Items.ForEach(x => copy.Items.Add(x.CreateCopy()));

            return copy;
        }

        public bool Validate()
        {
            bool isValid = false;

            return false;
        }
    }
}
