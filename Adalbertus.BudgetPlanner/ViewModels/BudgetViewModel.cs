using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        public BudgetViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            ExpensesViewModel   = IoC.Get<ExpensesViewModel>();
            RevenuesViewModel   = IoC.Get<RevenuesViewModel>();
            BudgetPlanViewModel = IoC.Get<BudgetPlanViewModel>();
            BudgetCalculatorViewModel = IoC.Get<BudgetCalculationsViewModel>();
        }

        public ExpensesViewModel ExpensesViewModel { get; private set; }
        public RevenuesViewModel RevenuesViewModel { get; private set; }
        public BudgetPlanViewModel BudgetPlanViewModel { get; private set; }
        public BudgetCalculationsViewModel BudgetCalculatorViewModel { get; private set; }

        public Budget Budget { get; private set; }
        public DateTime BudgetDate { get; set; }

        public override void LoadData()
        {
            Diagnostics.Start();
            LoadOrCreateDefaultBudget();
            BudgetPlanViewModel.LoadData(Budget);
            RevenuesViewModel.LoadData(Budget);
            ExpensesViewModel.LoadData(Budget);
            BudgetCalculatorViewModel.LoadData(Budget);

            RefreshUI();
            Diagnostics.Stop();
        }

        private void LoadOrCreateDefaultBudget()
        {
            Diagnostics.Start();
            using (var tx = Database.GetTransaction())
            {
                var cashFlowList = CachedService.GetAllCashFlows();

                var sql = PetaPoco.Sql.Builder
                                .Select("*")
                                .From("Budget")
                                .Where("@0 BETWEEN DateFrom AND DateTo", BudgetDate.Date);
                Budget = Database.FirstOrDefault<Budget>(sql);
                if (Budget == null)
                {
                    Budget = Budget.CreateEmptyForDate(BudgetDate, cashFlowList);
                    Database.Save(Budget);
                }

                tx.Complete();
                PublishRefreshRequest(Budget);
            }

            Budget.PropertyChanged += (s, e) => 
            { 
                Save(s as Budget);
            };
            Diagnostics.Stop();
        }

        protected override void OnRefreshRequest(RefreshEvent refreshEvent)
        {
            Diagnostics.Start();
            TypeSwitch.Do(refreshEvent.ChangedEntity,
                TypeSwitch.Case<Budget>(() => RefreshBudgetSummary()),
                TypeSwitch.Case<IncomeValue>(() => RefreshBudgetSummary()),
                TypeSwitch.Case<Expense>(() => RefreshBudgetSummary()),
                TypeSwitch.Case<BudgetPlan>(() => RefreshBudgetSummary()),
                TypeSwitch.Case<SavingValue>(() => RefreshBudgetSummary()));
            Diagnostics.Stop();
        }

        public IEnumerable<BudgetCalculatorEquation> BudgetEquations { 
            get
            {
                return BudgetCalculatorViewModel.AvaiableEquations;
            }
        }

        public bool IsBudgetEquationsEmpty
        {
            get
            {
                return !BudgetEquations.Any();
            }
        }

        private void RefreshUI()
        {
            Diagnostics.Start();
            NotifyOfPropertyChange(() => DateFrom);
            NotifyOfPropertyChange(() => DateTo);
            NotifyOfPropertyChange(() => TransferedValue);
            RefreshBudgetSummary();
            Diagnostics.Stop();
        }        

        public DateTime DateFrom
        {
            get { return Budget.DateFrom; }
        }

        public DateTime DateTo
        {
            get { return Budget.DateTo; }
        }

        public decimal TransferedValue
        {
            get { return Budget.TransferedValue; }
            set
            {
                Budget.TransferedValue = value;
                NotifyOfPropertyChange(() => TransferedValue);
                RefreshBudgetSummary();
            }
        }

        public decimal RealBudgetBilans
        {
            get { return Budget.RealBudgetBilans; }
        }

        public decimal TotalBudgetValue
        {
            get { return Budget.TotalBudgetValue; }
        }

        public decimal TotalSumOfRevenues
        {
            get { return Budget.TotalSumOfRevenues; }
        }

        public decimal SumOfRevenueIncomes
        {
            get { return Budget.SumOfRevenueIncomes; }
        }

        public decimal SumOfRevenueSavings
        {
            get { return Budget.SumOfRevenueSavings; }
        }

        public decimal TotalBudgetBilans
        {
            get { return Budget.TotalBudgetBilans; }
        }

        public decimal TotalBudgetPlanValue
        {
            get { return Budget.TotalBudgetPlanValue; }
        }

        public decimal TotalExpenseValue
        {
            get { return Budget.TotalExpenseValue; }
        }
        public decimal TotalBalanceValue
        {
            get
            {
                return TotalBudgetPlanValue - TotalExpenseValue;
            }
        }

        public decimal TotalBalanceProcentValue
        {
            get
            {
                if (TotalBudgetPlanValue == 0)
                {
                    return 0;
                }
                return (TotalExpenseValue / TotalBudgetPlanValue) * 100M;
            }
        }

        private void RefreshBudgetSummary()
        {
            Diagnostics.Start();
            NotifyOfPropertyChange(() => RealBudgetBilans);
            NotifyOfPropertyChange(() => TotalBudgetValue);
            NotifyOfPropertyChange(() => TotalSumOfRevenues);
            NotifyOfPropertyChange(() => SumOfRevenueIncomes);
            NotifyOfPropertyChange(() => SumOfRevenueSavings);

            NotifyOfPropertyChange(() => TotalBudgetBilans); 
            NotifyOfPropertyChange(() => TotalBudgetPlanValue);
            NotifyOfPropertyChange(() => TotalExpenseValue);
            NotifyOfPropertyChange(() => TotalBalanceProcentValue);
            NotifyOfPropertyChange(() => TotalBalanceValue);

            NotifyOfPropertyChange(() => BudgetEquations);
            NotifyOfPropertyChange(() => IsBudgetEquationsEmpty);
            Diagnostics.Stop();
        }
    }
}
