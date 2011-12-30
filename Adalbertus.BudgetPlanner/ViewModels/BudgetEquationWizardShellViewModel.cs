using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardShellViewModel : WizardShellViewModel<BudgetEquationWizardVM>
    {
        public BudgetEquationWizardShellViewModel(IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(database, configuration, cashedService, eventAggregator)
        {

        }

        public IEnumerable<BudgetCalculatorItem> EquationElements
        {
            get
            {
                if (Model != null)
                {
                    return Model.Equation.Items.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        public string EquationName
        {
            get
            {
                if (Model == null)
                {
                    return string.Empty;
                }
                return Model.EquationName;
            }
        }

        public decimal? EquationValue
        {
            get
            {
                if (Model == null)
                {
                    return null;
                }
                //Model.BudgetCalculatorEvaluator.AttachEvaluator(Model.Equation);
                return Model.EquationValue;
            }
        }

        protected override void LoadPages()
        {
            Items.Add(IoC.Get<BudgetEquationWizardStartViewModel>());
            Items.Add(IoC.Get<BudgetEquationWizardElementViewModel>());
            Items.Add(IoC.Get<BudgetEquationWizardFinishViewModel>());
        }

        public override void LoadData(BudgetEquationWizardVM model)
        {
            base.LoadData(model);
            if (Model != null)
            {
                Model.Equation.Items.CollectionChanged += delegate
                {
                    NotifyOfPropertyChange(() => CanFinish);
                    NotifyOfPropertyChange(() => EquationElements);
                };
                Model.Equation.Items.PropertyChanged += delegate { NotifyOfPropertyChange(() => EquationElements); };
            }
        }

        public override void RefreshNavigationUI()
        {
            base.RefreshNavigationUI();
            NotifyOfPropertyChange(() => EquationElements);
            NotifyOfPropertyChange(() => EquationName);
            NotifyOfPropertyChange(() => EquationValue);
        }

        public override bool CanFinish
        {
            get
            {
                bool canFinish = base.CanFinish;
                if (string.IsNullOrWhiteSpace(EquationName))
                {
                    return false;
                }
                return canFinish;
            }
        }

        public override void Initialize(dynamic parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(string.Format("Parametr 'parameters' cannot be null"));
            }

            var equations = (parameters.Equations as IEnumerable<BudgetCalculatorEquation>)
                                .Where(x => x.Id != parameters.Equation.Id).ToList();
            var model = BudgetEquationWizardVM.CreateInstance();
            model.ValueTypes = parameters.ValueTypes;
            model.OperatorTypes = parameters.OperatorTypes;
            model.CashFlows = parameters.CashFlows;
            model.CashFlowGroups = parameters.CashFlowGroups;
            model.Incomes = parameters.Incomes;
            model.Savings = parameters.Savings;
            model.Equations = equations;
            model.BudgetCalculatorEvaluator = parameters.BudgetCalculatorEvaluator;
            model.Clear(parameters.Equation.CreateCopy());
            LoadData(model);

            RefreshNavigationUI();
        }
    }
}
