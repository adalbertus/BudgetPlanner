using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using ILCalc;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetCalculationsViewModel : BaseViewModel
    {
        public BudgetCalculationsViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            Equations = new BindableCollectionExt<BudgetCalculatorEquation>();
            IsNewEquationVisible = true;
            Equations.PropertyChanged += (s, e) => { Save(s as Entity); };
        }

        public BudgetEquationWizardShellViewModel Wizard { get; set; }

        public BindableCollectionExt<BudgetCalculatorEquation> Equations { get; set; }
        public IEnumerable<BudgetCalculatorEquation> AvaiableEquations
        {
            get
            {
                return Equations.ToList();
            }
        }
        public Budget Budget { get; set; }
        private int LastEquationPosition { get; set; }
        
        public IEnumerable<dynamic> ValueTypes { get; private set; }
        public IEnumerable<dynamic> OperatorTypes { get; private set; }
        
        public void LoadData(Budget budget)
        {
            LoadValueTypes();
            LoadOperatorTypes();
            Budget = budget;
            if (Database.Count<BudgetCalculatorEquation>() > 0)
            {
                LastEquationPosition = Database.ExecuteScalar<int>("SELECT MAX(Position) FROM [BudgetCalculatorEquation]");
            }
            var equations = Database.Query<BudgetCalculatorEquation>("ORDER BY Id").ToList();
            equations.ForEach(eq =>
            {
                var equationItems = Database.Query<BudgetCalculatorItem>("WHERE BudgetCalculatorEquationId = @0 ORDER BY Id", eq.Id).ToList();
                eq.Items.Clear();
                eq.Items.AddRange(equationItems);
            });
            AttachEvaluators(equations);

            Equations.IsNotifying = false;
            Equations.Clear();
            Equations.AddRange(equations);
            Equations.IsNotifying = true;

            LoadWizard();
        }

        private void LoadWizard()
        {
            var wizard = IoC.Get<BudgetEquationWizardShellViewModel>();
            wizard.LoadData();
            wizard.Model.ValueTypes = ValueTypes.ToList();
            wizard.Model.OperatorTypes = OperatorTypes.ToList();
            wizard.Model.Equations = Equations.ToList();
            Wizard = wizard;
        }

        private void LoadValueTypes()
        {            
            ValueTypes = new dynamic[]
            {
                new 
                { 
                    Value = CalculatorValueType.BudgetIncomesValue,
                    Name = "Suma wszystkich dochodów",
                },
                //new 
                //{ 
                //    Value = CalculatorValueType.Operator,
                //    Name = "Działanie arytmetyczne",
                //},
                new 
                {
                    Value = CalculatorValueType.UserValue,
                    Name = "Wartość wprowadzona ręcznie",
                },
                new
                {
                    Value = CalculatorValueType.CalculatorEquationValue,
                    Name = "Wynik innego równania",
                },
            };
            NewValueTypeName = null;
        }

        private void LoadOperatorTypes()
        {
            OperatorTypes = new dynamic[]
            {
                new 
                { 
                    Value = CalculatorOperatorType.None,
                    Name = "Brak",
                },
                new 
                {
                    Value = CalculatorOperatorType.Add,
                    Name = "Dodaj",
                },
                new 
                {
                    Value = CalculatorOperatorType.Substract,
                    Name = "Odejmij",
                },
                new 
                {
                    Value = CalculatorOperatorType.Multiply,
                    Name = "Pomnóż",
                },
                new 
                {
                    Value = CalculatorOperatorType.Divide,
                    Name = "Podziel",
                },

            };
            NewOperatorTypeName = null;
        }

        private void AttachEvaluators(IEnumerable<BudgetCalculatorEquation> equations)
        {
            foreach (var equation in equations)
            {
                foreach (var item in equation.Items)
                {
                    switch (item.ValueType)
                    {
                        case CalculatorValueType.BudgetIncomesValue:
                            item.Evaluator = () => { return GetSumOfBudgetIncomes(); };
                            break;
                        case CalculatorValueType.BudgetExpensesOfFlowType:
                            var cashFlow = CachedService.GetAllCashFlows().FirstOrDefault(x => x.Id == item.ForeignId);
                            item.Evaluator = () => { return GetSumOfBudgetExpenses(cashFlow); };
                            break;
                        case CalculatorValueType.CalculatorEquationValue:
                            var calculatorEquation = equations.FirstOrDefault(x => x.Id == item.ForeignId);
                            if (calculatorEquation == null)
                            {
                                throw new NullReferenceException(string.Format("Nie udało się odnaleźć równania powiązanego z równaniem: {0}", item.Name));
                            }
                            item.Evaluator = () => { return calculatorEquation.CalculateValue(); };
                            break;
                    }
                }
            }
        }

        private string _newEquationName;
        public string NewEquationName
        {
            get { return _newEquationName; }
            set
            {
                _newEquationName = value;
                NotifyOfPropertyChange(() => NewEquationName);
            }
        }

        private bool _isNewEquationVisible;
        public bool IsNewEquationVisible
        {
            get { return _isNewEquationVisible; }
            set
            {
                _isNewEquationVisible = value;
                NotifyOfPropertyChange(() => IsNewEquationVisible);
            }
        }

        private bool _isNewEquationNameFocused;

        public bool IsNewEquationNameFocused
        {
            get { return _isNewEquationNameFocused; }
            set
            {
                _isNewEquationNameFocused = value;
                NotifyOfPropertyChange(() => IsNewEquationNameFocused);
            }
        }


        public void AddAndMoveToEquationName()
        {
            if (string.IsNullOrWhiteSpace(NewEquationName))
            {
                return;
            }

            var equation = new BudgetCalculatorEquation
            {
                Name = NewEquationName,
                IsVisible = IsNewEquationVisible,
                Position = ++LastEquationPosition,
            };

            Save(equation);

            Equations.Add(equation);

            NewEquationName = string.Empty;
            IsNewEquationVisible = true;
            IsNewEquationNameFocused = false;
            IsNewEquationNameFocused = true;
        }

        private dynamic _newValueTypeName;
        public dynamic NewValueTypeName
        {
            get { return _newValueTypeName; }
            set { 
                _newValueTypeName = value;
                NotifyOfPropertyChange(() => NewValueTypeName);
            }
        }

        private dynamic _newOperatorTypeName;
        public dynamic NewOperatorTypeName
        {
            get { return _newOperatorTypeName; }
            set
            {
                _newOperatorTypeName = value;
                NotifyOfPropertyChange(() => NewOperatorTypeName);
            }
        }

        public void AddItem(BudgetCalculatorEquation equation)
        {
            if (equation == null)
            {
                return;
            }

            NewValueTypeName = null;
            NewOperatorTypeName = null;
            
        }

        [Obsolete("Remove after VM is done", false)]
        private void InitSampleData()
        {
            if (Database.Count<BudgetCalculatorEquation>() > 0)
            {
                return;
            }

            using (var tx = Database.GetTransaction())
            {
                var eq1 = new BudgetCalculatorEquation
                {
                    Name = "Dziesięcina",
                    IsVisible = true,
                };

                eq1.AddItem("Wszystkie dochody", CalculatorValueType.BudgetIncomesValue);
                eq1.AddItem("*", CalculatorValueType.Operator, CalculatorOperatorType.Multiply);
                eq1.AddItem("0.1", CalculatorValueType.UserValue, CalculatorOperatorType.None, 0.1M);

                Database.Save(eq1);
                Database.SaveAll(eq1.Items);

                var eq2 = new BudgetCalculatorEquation
                {
                    Name = "Dziesięcina bez obiadów",
                    IsVisible = true,
                };

                eq2.AddItem("Dziesięcina", CalculatorValueType.CalculatorEquationValue, foreignId: eq1.Id);
                eq2.AddItem("-", CalculatorValueType.Operator, CalculatorOperatorType.Substract);
                var obiadyCashFlow = CachedService.GetAllCashFlows().First(x => x.Name == "Obiady");
                eq2.AddItem("Wydatki na obiady", CalculatorValueType.BudgetExpensesOfFlowType, foreignId: obiadyCashFlow.Id);

                Database.Save(eq2);
                Database.SaveAll(eq2.Items);

                tx.Complete();
            }
        }

        #region Equation evaluators
        private decimal GetSumOfBudgetIncomes(string incomeName = null)
        {
            if (string.IsNullOrWhiteSpace(incomeName))
            {
                return Budget.SumOfRevenueIncomes;
            }
            else
            {
                return Budget.IncomeValues.Where(x => x.IncomeName == incomeName).Sum(x => x.Value);
            }
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
        #endregion Equation evaluators
    }
}
