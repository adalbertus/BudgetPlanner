using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public abstract class BaseDailogViewModel : BaseViewModel, IDialog
    {
        public BaseDailogViewModel(IShellViewModel shell, IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            State = DialogState.None;
        }

        public DialogState State { get; private set; }
        public object Result { get; private set; }
        public System.Action OKCallback { get; set; }
        public System.Action CancelCallback { get; set; }
        public System.Action AfterCloseCallback { get; set; }

        public virtual void Initialize(dynamic parameters)
        {

        }

        protected void CleanUp()
        {
            OnDeactivate(true);
            if (AfterCloseCallback != null)
            {
                Execute.OnUIThread(AfterCloseCallback);
            }
        }

        public virtual void Close()
        {
            State = DialogState.OK;
            if (OKCallback != null)
            {
                Execute.OnUIThread(OKCallback);
            }
            CleanUp();
        }

        public virtual void Cancel()
        {
            State = DialogState.Cancel;
            if (CancelCallback != null)
            {
                Execute.OnUIThread(CancelCallback);
            }
            CleanUp();
        }
    }
}
