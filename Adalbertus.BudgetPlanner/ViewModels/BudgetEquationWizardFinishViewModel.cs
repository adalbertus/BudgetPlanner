using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardFinishViewModel : WizardPageViewModel<BudgetEquationWizardVM>
    {
        public BudgetEquationWizardFinishViewModel()
        {
            BackPageName = typeof(BudgetEquationWizardElementViewModel).Name;
            //NextPageName = typeof(BudgetEquationWizardElementViewModel).Name;            
        }

        public override void MoveBack()
        {
            base.MoveBack();
        }
    }
}
