using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;

namespace Adalbertus.BudgetPlanner.Controls
{
    /// <summary>
    /// Use only with AutoCompleteBox!
    /// </summary>
    public class AutoCompleteBoxPopup : Popup
    {
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            // workaround of tab order problem - when user select listbox item byc KeyDown and then press tab
            // listbox (and popup) lost focus and next tab was first control in main window - this workaround
            // focuses AutoCompleteTextBox instead.
            if (!this.IsOpen)
            {
                var ui = this.Parent as UIElement;
                if (ui != null)
                {
                    ui.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                }
            }
        }

    }
}
