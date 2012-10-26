using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesFilteringViewModel : BaseViewModel
    {
        public ExpensesFilteringViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            Filter = IoC.Get<ExpensesFilterVM>();            
        }

        public ExpensesFilterVM Filter { get; private set; }
        public Budget Budget { get; set; }

        public IEnumerable<CashFlow> CashFlows
        {
            get
            {
                if (SelectedCashFlowGroup == null)
                {
                    return CachedService.GetAllCashFlows();
                }
                else
                {
                    if (SelectedCashFlow != null && SelectedCashFlow.CashFlowGroupId != SelectedCashFlowGroup.Id)
                    {
                        SelectedCashFlow = null;
                    }
                    return CachedService.GetAllCashFlows().Where(x => x.CashFlowGroupId == SelectedCashFlowGroup.Id).ToList();
                }
            }
        }

        public IEnumerable<CashFlowGroup> CashFlowGroups
        {
            get { return CachedService.GetAllCashFlowGroups(); }
        }

        public CashFlow SelectedCashFlow
        {
            get { return Filter.CashFlow; }
            set
            {
                Filter.CashFlow = value;
                NotifyOfPropertyChange(() => SelectedCashFlow);
            }
        }

        public CashFlowGroup SelectedCashFlowGroup
        {
            get { return Filter.CashFlowGroup; }
            set
            {
                Filter.CashFlowGroup = value;
                NotifyOfPropertyChange(() => SelectedCashFlowGroup);
                NotifyOfPropertyChange(() => CashFlows);
            }
        }

        public DateTime DateFrom
        {
            get { return Filter.DateFrom; }
            set
            {
                Filter.DateFrom = value;
                NotifyOfPropertyChange(() => DateFrom);
            }
        }

        public DateTime DateTo
        {
            get { return Filter.DateTo; }
            set
            {
                Filter.DateTo = value;
                NotifyOfPropertyChange(() => DateTo);
            }
        }

        public decimal? ValueFrom
        {
            get { return Filter.ValueFrom; }
            set
            {
                Filter.ValueFrom = value;
                NotifyOfPropertyChange(() => ValueFrom);
            }
        }

        public decimal? ValueTo
        {
            get { return Filter.ValueTo; }
            set
            {
                Filter.ValueTo = value;
                NotifyOfPropertyChange(() => ValueTo);
            }
        }

        public string Description
        {
            get { return Filter.Description; }
            set
            {
                Filter.Description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public void LoadData(Budget budget)
        {
            Diagnostics.Start();
            Budget = budget;
            FillFilterData();
            Refresh();
            Diagnostics.Start();
        }

        private void FillFilterData()
        {
            Diagnostics.Start();
            Filter.IsNotifying = false;
            Filter.DateFrom = Budget.DateFrom;
            Filter.DateTo = Budget.DateTo;
            Filter.CashFlow = null;
            Filter.CashFlowGroup = null;
            Filter.IsNotifying = true;

            Diagnostics.Stop();
        }
    }
}
