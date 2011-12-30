using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Extensions;
using ILCalc;
using Microsoft.Windows.Controls;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetCalculationsViewModel : BaseViewModel, IHandle<WizardEvent<BudgetEquationWizardVM>>
    {
        public BudgetCalculationsViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            SuppressEvent = false;
            Equations = new BindableCollectionExt<BudgetCalculatorEquation>();
            BudgetCalculatorEvaluator = IoC.Get<BudgetCalculatorEvaluator>();
            Equations.PropertyChanged += (s, e) => { if (!SuppressEvent) { Save(s as Entity); } };
            EventAggregator.Subscribe(this);
        }

        private bool SuppressEvent { get; set; }

        public BindableCollectionExt<BudgetCalculatorEquation> Equations { get; set; }
        public IEnumerable<BudgetCalculatorEquation> AvaiableEquations
        {
            get
            {
                return Equations.Where(x => x.IsVisible).OrderBy(x => x.Position).ToList();
            }
        }
        public Budget Budget { get; set; }
        public BudgetCalculatorEvaluator BudgetCalculatorEvaluator { get; private set; }
        private int LastEquationPosition { get; set; }
        public BudgetCalculatorEquation EquationToEdit { get; set; }
        
        public IEnumerable<ComboItemVM<CalculatorValueType>> ValueTypes { get; private set; }
        public IEnumerable<ComboItemVM<CalculatorOperatorType>> OperatorTypes { get; private set; }
        
        public void LoadData(Budget budget)
        {
            BudgetCalculatorEvaluator.Budget = budget;
            LoadValueTypes();
            LoadOperatorTypes();
            Budget = budget;
            if (Database.Count<BudgetCalculatorEquation>() > 0)
            {
                LastEquationPosition = Database.ExecuteScalar<int>("SELECT MAX(Position) FROM [BudgetCalculatorEquation]");
            }
            var equations = CachedService.GetAllEquations();            
            equations.ForEach(x => 
            {                
                BudgetCalculatorEvaluator.Refresh(x);
            });

            Equations.IsNotifying = false;
            Equations.Clear();
            Equations.AddRange(equations);
            Equations.IsNotifying = true;
            Equations.Refresh();
        }

        private void LoadValueTypes()
        {            
            ValueTypes = new ComboItemVM<CalculatorValueType>[]
            {
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetIncomesValue,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetIncomesValueOfType,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetSavingsValue,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetSavingsValueOfType,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetTotalRevenuesValue,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetExpensesValueOfType,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetExpensesWithDescription,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetPlanValue,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetPlanValueOfGroup,
                },
                new ComboItemVM<CalculatorValueType>
                { 
                    Value = CalculatorValueType.BudgetPlanValueOfCategory,
                },
                new ComboItemVM<CalculatorValueType>
                {
                    Value = CalculatorValueType.UserValue,
                },
                new ComboItemVM<CalculatorValueType>
                {
                    Value = CalculatorValueType.CalculatorEquationValue,
                },
            };
            ValueTypes.ForEach(x => x.Name = x.Value.ToName());
        }

        private void LoadOperatorTypes()
        {
            OperatorTypes = new ComboItemVM<CalculatorOperatorType>[]
            {
                new ComboItemVM<CalculatorOperatorType>
                { 
                    Value = CalculatorOperatorType.None,
                    Name = "Brak",
                },
                new ComboItemVM<CalculatorOperatorType>
                {
                    Value = CalculatorOperatorType.Add,
                    Name = "Dodaj",
                },
                new ComboItemVM<CalculatorOperatorType>
                {
                    Value = CalculatorOperatorType.Substract,
                    Name = "Odejmij",
                },
                new ComboItemVM<CalculatorOperatorType>
                {
                    Value = CalculatorOperatorType.Multiply,
                    Name = "Pomnóż",
                },
                new ComboItemVM<CalculatorOperatorType>
                {
                    Value = CalculatorOperatorType.Divide,
                    Name = "Podziel",
                },

            };
        }

        public void Create()
        {
            EquationToEdit = new BudgetCalculatorEquation { IsVisible = true };
            ShowWizard();
        }

        public void Edit(BudgetCalculatorEquation equation)
        {
            EquationToEdit = equation;
            ShowWizard();
        }

        public void Delete(BudgetCalculatorEquation equation)
        {
            Delete(equation, false);
        }
        public void Delete(BudgetCalculatorEquation equation, bool omitConfirmation)
        {
            if (!omitConfirmation)
            {
                var isRequiredForOthers = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                    .Select("COUNT(*)")
                    .From("BudgetCalculatorItem")
                    .Where("ForeignId = @0 AND ValueTypeName = 'CalculatorEquationValue'", equation.Id)) > 0;
                if (isRequiredForOthers)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat(string.Format("Równanie {0} jest wykorzystywane w innych równaniach.", equation.Name));
                    sb.AppendLine();
                    sb.AppendLine("Usunięcie go spowoduje usunięcie wsztkich równań, które są od niego zależne.");
                    sb.AppendLine();
                    sb.AppendLine("Na pewno chcesz wykonać operację?");
                    
                    Shell.ShowMessage(sb.ToString(), () => Delete(equation, true), null, System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Question);
                    return;
                }
            }

            DeleteEquation(equation);
            var equationsToDelete = Equations.Where(eq => 
            {
                if (eq.Id == equation.Id)
                {
                    return true;
                }
                else
                {
                    return eq.Items.Any(x => x.ForeignId == equation.Id && x.ValueType == CalculatorValueType.CalculatorEquationValue);
                }
            }).ToList();

            Equations.RemoveRange(equationsToDelete);
        }

        private void ShowWizard()
        {
            Shell.ShowDialog<BudgetEquationWizardShellViewModel, BudgetEquationWizardVM>(
                new
                {
                    ValueTypes = ValueTypes.ToList(),
                    OperatorTypes = OperatorTypes.ToList(),
                    Incomes = CachedService.GetAllIncomes().ToList(),
                    Savings = CachedService.GetAllSavings().ToList(),
                    CashFlows = CachedService.GetAllCashFlows().ToList(),
                    CashFlowGroups = CachedService.GetAllCashFlowGroups().ToList(),
                    Equations = Equations.ToList(),
                    Equation = EquationToEdit,
                    BudgetCalculatorEvaluator = BudgetCalculatorEvaluator,
                },
                null,
                null);
        }

        #region IHandle<WizardEvent<BudgetEquationWizardVM>> Members

        public void Handle(WizardEvent<BudgetEquationWizardVM> message)
        {
            if (EquationToEdit != null && message.Status == WizardStatus.OK)
            {
                SuppressEvent = true;
                SaveEquation(message.Model.Equation);
                var equation = Equations.FirstOrDefault(x => x.Id == message.Model.Equation.Id);
                if (equation != null)
                {
                    equation.Name = message.Model.Equation.Name;
                    equation.IsVisible = message.Model.Equation.IsVisible;
                    equation.Items.Clear();
                    equation.Items.AddRange(message.Model.Equation.Items);
                    BudgetCalculatorEvaluator.Refresh(equation);
                    equation.Refresh();
                }
                else
                {
                    Equations.Add(message.Model.Equation);
                }

                BudgetCalculatorEvaluator.Refresh(message.Model.Equation);
                SuppressEvent = false;
            }
        }

        private void SaveEquation(BudgetCalculatorEquation budgetCalculatorEquation)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(budgetCalculatorEquation);
                var itemsToDelete = EquationToEdit.Items.Where(x => !budgetCalculatorEquation.Items.Any(y => x.Id == y.Id));
                itemsToDelete.ForEach(x => Database.Delete(x));
                Database.SaveAll(budgetCalculatorEquation.Items);
                tx.Complete();
                CachedService.Clear(CachedServiceKeys.AllEquations);
                PublishRefreshRequest(budgetCalculatorEquation);
            }
        }

        private void DeleteEquation(BudgetCalculatorEquation equation)
        {
            using (var tx = Database.GetTransaction())
            {
                // delete all related equations
                var equationsDeletedCounter = Database.Execute("DELETE FROM BudgetCalculatorEquation WHERE Id IN (SELECT BudgetCalculatorEquationId FROM BudgetCalculatorItem WHERE ForeignId = @0 AND ValueTypeName = 'CalculatorEquationValue')", equation.Id);
                Database.Execute("DELETE FROM BudgetCalculatorItem WHERE ForeignId = @0 AND ValueTypeName = 'CalculatorEquationValue'", equation.Id);
                Database.Execute("DELETE FROM BudgetCalculatorItem WHERE BudgetCalculatorEquationId = @0", equation.Id);
                Database.Delete(equation);                
                tx.Complete();
                CachedService.Clear(CachedServiceKeys.AllEquations);
                PublishRefreshRequest(equation);
            }
        }

        #endregion
    }
}
