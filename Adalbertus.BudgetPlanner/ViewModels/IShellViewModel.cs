using Adalbertus.BudgetPlanner.Core;
namespace Adalbertus.BudgetPlanner.ViewModels
{
    public interface IShellViewModel 
    {
        void ShowDialog<TDialogViewModel>(dynamic initParameters, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog;
        void ShowMessage(string message, System.Action okCallback, System.Action cancelCallback);
    }
}
