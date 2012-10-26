using Adalbertus.BudgetPlanner.Core;
using System.Windows;
namespace Adalbertus.BudgetPlanner.ViewModels
{
    public interface IShellViewModel 
    {
        void ShowDialog<TDialogViewModel>(dynamic initParameters, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog<object>;
        void ShowDialog<TDialogViewModel, TModel>(dynamic initParameters, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog<TModel>;
        void ShowDialog<TDialogViewModel, TModel>(TDialogViewModel instance, System.Action okCallback, System.Action cancelCallback) where TDialogViewModel : IDialog<TModel>;
        void ShowMessage(string message, System.Action okCallback, System.Action cancelCallback, MessageBoxButton button = MessageBoxButton.OKCancel, MessageBoxImage image = MessageBoxImage.Information);
        void ShowQuestion(string message, System.Action okCallback, System.Action cancelCallback);
    }
}
