using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardFinishViewModel : WizardPageViewModel<BudgetEquationWizardVM>
    {
        public BudgetEquationWizardFinishViewModel()
        {
            BackPageName = typeof(BudgetEquationWizardElementViewModel).Name;
            //NextPageName = typeof(BudgetEquationWizardElementViewModel).Name;            
        }

        public IEnumerable<BudgetCalculatorItem> Items { get { return Model.Items; } }

        public override void OnActivated()
        {
            base.OnActivated();
            Model.BudgetCalculatorEvaluator.Refresh(Model.Equation);
            Refresh();
        }

        public override void MoveBack()
        {
            base.MoveBack();
        }

        public string StringEquation { get { return string.Empty; } }
    }
}
