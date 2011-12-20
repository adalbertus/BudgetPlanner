using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardShellViewModel : WizardShellViewModel<BudgetEquationWizardVM>
    {
        protected override void LoadModel()
        {
            Model = new BudgetEquationWizardVM();
        }

        protected override void LoadPages()
        {
            Items.Add(IoC.Get<BudgetEquationWizardStartViewModel>());
            Items.Add(IoC.Get<BudgetEquationWizardElementViewModel>());
            Items.Add(IoC.Get<BudgetEquationWizardFinishViewModel>());
        }
    }
}
