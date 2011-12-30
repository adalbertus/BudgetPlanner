using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Core
{
    public enum DialogState
    {
        None,
        OK,
        Cancel,
    }

    public interface IDialog<TModel>
    {
        DialogState State { get; }
        TModel Result { get; }
        Action OKCallback { get; set; }
        Action CancelCallback { get; set; }
        Action AfterCloseCallback { get; set; }

        void Initialize(dynamic parameters);
        void LoadData();
    }
}
