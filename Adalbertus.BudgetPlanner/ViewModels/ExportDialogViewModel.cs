using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Services;
using System.Diagnostics;
using System.IO;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExportDialogViewModel : BaseDailogViewModel
    {
        #region Private fields
        #endregion Private fields

        #region Constructors
        public ExportDialogViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            AvaiableDates = new BindableCollectionExt<DateTimeVM>();
        }
        #endregion Constructors

        #region Properties
        public Budget Budget { get; set; }
        public BindableCollectionExt<DateTimeVM> AvaiableDates { get; set; }
        private DateTimeVM _selectedDateFrom;
        public DateTimeVM SelectedDateFrom
        {
            get { return _selectedDateFrom; }
            set
            {
                _selectedDateFrom = value;
                if (SelectedDateFrom != null && SelectedDateTo != null && SelectedDateFrom.DateTime > SelectedDateTo.DateTime)
                {
                    SelectedDateTo = SelectedDateFrom;
                }
                NotifyOfPropertyChange(() => SelectedDateFrom);
                NotifyOfPropertyChange(() => DateFrom);
                NotifyOfPropertyChange(() => SelectedExportPeriod);
            }
        }
        private DateTimeVM _selectedDateTo;
        public DateTimeVM SelectedDateTo
        {
            get { return _selectedDateTo; }
            set
            {
                _selectedDateTo = value;
                if (SelectedDateFrom != null && SelectedDateTo != null && SelectedDateTo.DateTime < SelectedDateFrom.DateTime)
                {
                    SelectedDateFrom = SelectedDateTo;
                }
                NotifyOfPropertyChange(() => SelectedDateTo);
                NotifyOfPropertyChange(() => DateTo);
                NotifyOfPropertyChange(() => SelectedExportPeriod);
            }
        }

        public DateTime DateFrom
        {
            get { return GetDefaultExportDate(SelectedDateFrom); }
        }

        public DateTime DateTo
        {
            get { return GetDefaultExportDate(SelectedDateTo); }
        }

        public string SelectedExportPeriod
        {
            get { return string.Format("{0:yyyy-MM} - {1:yyyy-MM}", DateFrom, DateTo); }
        }

        private string _selectedExportFilePath;
        public string SelectedExportFilePath
        {
            get { return _selectedExportFilePath; }
            set
            {
                _selectedExportFilePath = value;
                NotifyOfPropertyChange(() => SelectedExportFilePath);
                NotifyOfPropertyChange(() => ExportFilePath);
            }
        }
        public string ExportFilePath
        {
            get { return GetExportFilePath(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        private string _infoMessage;
        public string InfoMessage
        {
            get { return _infoMessage; }
            set
            {
                _infoMessage = value;
                NotifyOfPropertyChange(() => InfoMessage);
            }
        }
        #endregion Properties

        #region Public methods
        public override void LoadData()
        {
            AvaiableDates.Clear();
            ErrorMessage = string.Empty;
            var sql = PetaPoco.Sql.Builder.Select("date(DateFrom)").From("Budget").OrderBy("DateFrom ASC");
            var dates = Database.Query<string>(sql).ToList();
            dates.ForEach(x => AvaiableDates.Add(new DateTimeVM(x, "yyyy-MM")));
            SelectedDateFrom = null;
            SelectedDateTo = null;
        }

        public void SelectExportFilePath()
        {
            var fileService = IoC.Get<IFileService>();
            SelectedExportFilePath = fileService.SaveExcelFile(SelectedExportPeriod);
        }

        public override void Close()
        {
            try
            {
                ErrorMessage = string.Empty;
                ExportBudget();
                base.Close();
            }
            catch (IOException)
            {
                ErrorMessage = "Brak dostępu do pliku. Być może jest on już otwarty...";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        #endregion Public methods

        #region Private methods
        public override void Initialize(dynamic parameters)
        {
            Budget = parameters.CurrentBudget;
        }

        private DateTime GetDefaultExportDate(DateTimeVM selectedDate)
        {
            if (selectedDate != null)
            {
                return selectedDate.DateTime;
            }

            return Budget.DateFrom;
        }

        private string GetExportFilePath()
        {
            if (!string.IsNullOrWhiteSpace(SelectedExportFilePath))
            {
                return SelectedExportFilePath;
            }
#if DEBUG
            return string.Format(@"{0}\Budżet {1}.xlsx", @"C:\tmp", SelectedExportPeriod);
#else
            return string.Format(@"{0}\Budżet {1}.xlsx", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SelectedExportPeriod);
#endif
        }

        private void ExportBudget()
        {
            ErrorMessage = null;
            InfoMessage = null;
            Diagnostics.Start();
            var cashFlows = CachedService.GetAllCashFlows();

            var allBudgets = Database.Query<Budget>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("Budget")
                    .Where("date(Budget.DateFrom) BETWEEN date(@0) AND date(@1)", DateFrom, DateTo)
                    .OrderBy("Budget.DateFrom")).ToList();

            var sql = PetaPoco.Sql.Builder
                .Select("*")
                .From("BudgetPlan")
                .InnerJoin("Budget")
                .On("Budget.Id = BudgetPlan.BudgetId")
                .InnerJoin("CashFlow")
                .On("CashFlow.Id = BudgetPlan.CashFlowId")
                .InnerJoin("CashFlowGroup")
                .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                .Where("date(Budget.DateFrom) BETWEEN date(@0) AND date(@1)", DateFrom, DateTo);
            var budgetPlansInDB = Database.Query<BudgetPlan, Budget, CashFlow, CashFlowGroup>(sql).ToList();

            foreach (var budget in allBudgets)
            {
                foreach (var cashFlow in cashFlows)
                {
                    var definedPlans = budgetPlansInDB.Where(x => x.BudgetId == budget.Id && x.CashFlowId == cashFlow.Id).ToList();
                    if (definedPlans.Any())
                    {
                        definedPlans.ForEach(x => budget.BudgetPlanItems.Add(x));
                    }
                    else
                    {
                        budget.BudgetPlanItems.Add(new BudgetPlan
                            {
                                Budget = budget,
                                CashFlow = cashFlow,
                                Value = 0,
                                Description = string.Empty,
                            });
                    }
                }

                Diagnostics.Start(string.Format("Fetching expenses for budget id = {0}", budget.Id));
                var sqlExpense = PetaPoco.Sql.Builder
                    .Select("*")
                    .From("Expense")
                    .InnerJoin("Budget")
                    .On("Budget.Id = Expense.BudgetId")
                    .InnerJoin("CashFlow")
                    .On("CashFlow.Id = Expense.CashFlowId")
                    .InnerJoin("CashFlowGroup")
                    .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                    .Where("Budget.Id = @0", budget.Id);
                var expenses = Database.Query<Expense, Budget, CashFlow, CashFlowGroup>(sqlExpense).ToList();
                Diagnostics.Stop();
                budget.Expenses.AddRange(expenses);
            }

            var exportService = IoC.Get<IExportService>();
            exportService.ExportBudget(ExportFilePath, allBudgets);
            Diagnostics.Stop();

            Process.Start(ExportFilePath);

        }
        #endregion Private methods
    }
}
