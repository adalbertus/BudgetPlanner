using System;
using System.Collections.Generic;
using System.Linq;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using Caliburn.Micro;
using System.Linq.Expressions;
using System.Windows.Data;
using System.Text;
using Adalbertus.BudgetPlanner.Services;
using System.Diagnostics;
using System.IO;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesViewModel : BaseViewModel, IHandle<ExpensesFilterVM>
    {
        public ExpensesViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            BudgetExpenses = new BindableCollectionExt<ExpenseVM>();
            ExpensesGridCashFlows = new BindableCollectionExt<CashFlow>();
            CashFlows = new BindableCollectionExt<CashFlow>();
            Filter = new ExpensesFilterVM(EventAggregator);
            ExpensesFilteringViewModel = IoC.Get<ExpensesFilteringViewModel>();
        }

        protected override void Initialize()
        {
            base.Initialize();
            SelectedExpenseDate = DateTime.Now;
        }

        protected override void OnRefreshRequest(RefreshEvent refreshEvent)
        {
            base.OnRefreshRequest(refreshEvent);
            if (refreshEvent.ChangedEntity is Expense && refreshEvent.Sender == typeof(BudgetTemplateDialogViewModel).Name)
            {
                FilterBudgetExpenses();
            }
        }

        public ExpensesFilteringViewModel ExpensesFilteringViewModel { get; set; }

        // Workaround - need ExpensesGridCashFlows and CashFlows because without spliting
        // it was refresed only in DataGrid in ExpensesView
        public BindableCollectionExt<CashFlow> ExpensesGridCashFlows { get; private set; }
        public BindableCollectionExt<CashFlow> CashFlows { get; private set; }
        //public int BudgetId { get; private set; }

        #region Budget plan expenses
        private Budget _budget;
        public Budget Budget
        {
            get { return _budget; }
            set
            {
                _budget = value;
                NotifyOfPropertyChange(() => Budget);
            }
        }

        public BindableCollectionExt<ExpenseVM> BudgetExpenses { get; private set; }

        public int BudgetExpensesFilteredCount
        {
            get { return BudgetExpenses.Count; }
        }

        public decimal TotalExpensesValue
        {
            get
            {
                return BudgetExpenses.Sum(x => x.Value);
            }
        }

        private int _currentBudgetExpensesTotalCount;
        public int CurrentBudgetExpensesTotalCount
        {
            get { return _currentBudgetExpensesTotalCount; }
            set
            {
                _currentBudgetExpensesTotalCount = value;
                NotifyOfPropertyChange(() => CurrentBudgetExpensesTotalCount);
                NotifyOfPropertyChange(() => AreExpensesNotInBudgetScope);
            }
        }

        private int _currentBudgetExpensesFilteredCount;
        public int CurrentBudgetExpensesFilteredCount
        {
            get { return _currentBudgetExpensesFilteredCount; }
            set
            {
                _currentBudgetExpensesFilteredCount = value;
                NotifyOfPropertyChange(() => CurrentBudgetExpensesFilteredCount);
                NotifyOfPropertyChange(() => AreExpensesNotInBudgetScope);
            }
        }

        private CashFlow _selectedExpenseCashFlow;
        public CashFlow SelectedExpenseCashFlow
        {
            get
            {
                return _selectedExpenseCashFlow;
            }
            set
            {
                _selectedExpenseCashFlow = value;
                NotifyOfPropertyChange(() => SelectedExpenseCashFlow);
                NotifyOfPropertyChange(() => CanAddExpense);
            }
        }

        private DateTime _selectedExpenseDate;
        public DateTime SelectedExpenseDate
        {
            get
            {
                return _selectedExpenseDate;
            }
            set
            {
                _selectedExpenseDate = value;
                NotifyOfPropertyChange(() => SelectedExpenseDate);
                NotifyOfPropertyChange(() => CanAddExpense);
            }
        }

        private decimal _expenseValue;
        public decimal ExpenseValue
        {
            get
            {
                return _expenseValue;
            }
            set
            {
                _expenseValue = value;
                NotifyOfPropertyChange(() => ExpenseValue);
                NotifyOfPropertyChange(() => CanAddExpense);
            }
        }

        private string _expenseDescription;
        public string ExpenseDescription
        {
            get { return _expenseDescription; }
            set
            {
                _expenseDescription = value;
                NotifyOfPropertyChange(() => ExpenseDescription);
            }
        }

        public bool AreExpensesNotInBudgetScope
        {
            get { return !(CurrentBudgetExpensesTotalCount == CurrentBudgetExpensesFilteredCount && CurrentBudgetExpensesTotalCount == BudgetExpenses.Count); }
        }

        #region Focus properties
        private bool _isExpenseDescriptionFocused;

        public bool IsExpenseDescriptionFocused
        {
            get { return _isExpenseDescriptionFocused; }
            set
            {
                _isExpenseDescriptionFocused = value;
                NotifyOfPropertyChange(() => IsExpenseDescriptionFocused);
            }
        }

        private bool _isExpenseValueFocused;
        public bool IsExpenseValueFocused
        {
            get
            {
                return _isExpenseValueFocused;
            }
            set
            {
                _isExpenseValueFocused = value;
                NotifyOfPropertyChange(() => IsExpenseValueFocused);
            }
        }
        #endregion

        public void RefreshUI()
        {
            Diagnostics.Start();
            FilterBudgetExpenses();
            Diagnostics.Stop();
        }

        public void LoadData(Budget budget)
        {
            Diagnostics.Start();
            Budget = budget;

            LoadCashFlows();
            ExpensesFilteringViewModel.LoadData(budget);

            RefreshUI();
            Diagnostics.Stop();
        }

        private void LoadCashFlows()
        {
            Diagnostics.Start();
            ExpensesGridCashFlows.IsNotifying = false;
            ExpensesGridCashFlows.Clear();
            CashFlows.Clear();
            var cashFlowList = CachedService.GetAllCashFlows();
            cashFlowList.ForEach(x => ExpensesGridCashFlows.Add(x));
            cashFlowList.ForEach(x => CashFlows.Add(x));

            ExpensesGridCashFlows.IsNotifying = true;
            SelectedExpenseCashFlow = CashFlows.FirstOrDefault();

            NotifyOfPropertyChange(() => ExpensesGridCashFlows);
            Diagnostics.Stop();
        }

        public void RemoveExpense(ExpenseVM expense)
        {
            Diagnostics.Start();
            BudgetExpenses.Remove(expense);
            RefreshUI();
            Diagnostics.Stop();
        }

        public bool CanAddExpense
        {
            get
            {
                bool isCashFlowSelected = SelectedExpenseCashFlow != null;
                bool hasPositiveExpenseValue = ExpenseValue > 0;
                return (isCashFlowSelected && hasPositiveExpenseValue);
            }
        }

        private Expense _scrollToExpense;
        public Expense ScrollToExpense
        {
            get { return _scrollToExpense; }
            set
            {
                _scrollToExpense = value;
                NotifyOfPropertyChange(() => ScrollToExpense);
            }
        }


        public void AddExpense()
        {
            AddExpense(true);
        }

        public void AddExpense(bool showConfirmation)
        {
            Diagnostics.Start();
            if (showConfirmation)
            {
                if (SelectedExpenseDate < Budget.DateFrom || SelectedExpenseDate > Budget.DateTo)
                {
                    var messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine("Wstawiasz wydatek z poza poza wybranego okresu budżetowego.");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Na pewno chcesz to zrobić?");

                    Shell.ShowQuestion(messageBuilder.ToString(),
                        () => AddExpense(false),
                        null);
                    return;
                }
            }

            var expense = Budget.AddExpense(SelectedExpenseCashFlow, ExpenseValue, ExpenseDescription, SelectedExpenseDate);

            Save(expense);
            FilterBudgetExpenses();
            ExpenseValue = 0;
            ExpenseDescription = string.Empty;

            ScrollToExpense = expense;

            Diagnostics.Stop();
        }

        private void Save(Expense expense)
        {
            Diagnostics.Start();
            Diagnostics.Start("Database operations");
            bool refreshExpensesList = false;
            using (var tx = Database.GetTransaction())
            {
                int savingsDeletedCounter = 0;

                refreshExpensesList = UpdateBudgetIdIfNeeded(expense);
                Database.Save(expense);

                if (expense.Flow.Saving == null)
                {
                    savingsDeletedCounter = Database.Delete<SavingValue>("WHERE ExpenseId = @0", expense.Id);
                }

                if (expense.SavingValue != null)
                {
                    expense.SavingValue.UpdateDescription();
                    Database.Save(expense.SavingValue);
                    int savingValuesCount = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                        .Select("COUNT(*)")
                        .From("SavingValue")
                        .Where("ExpenseId = @0", expense.Id));
                    if (savingValuesCount > 1)
                    {
                        savingsDeletedCounter =
                            Database.Delete<SavingValue>("WHERE ExpenseId = @0 AND SavingId <> @1", expense.Id, expense.SavingValue.SavingId);
                    }
                }

                tx.Complete();
                CachedService.Clear(CachedServiceKeys.AllSavings);
                UpdateExpenseInCurrentBudget(expense);
            }
            Diagnostics.Stop();

            NotifyOfPropertyChange(() => TotalExpensesValue);
            PublishRefreshRequest(expense);
            if (refreshExpensesList)
            {
                FilterBudgetExpenses();
            }
            Diagnostics.Stop();
        }

        private bool UpdateBudgetIdIfNeeded(Expense expense)
        {
            var budget = BudgetViewModel.LoadOrCreateDefaultBudget(Database, expense.Date, CachedService.GetAllCashFlows());
            if (expense.Budget.Id != budget.Id)
            {
                expense.Budget = budget;
                return true;
            }
            return false;
        }

        private void UpdateExpenseInCurrentBudget(Expense expense)
        {
            var expenseToUpdate = Budget.Expenses.FirstOrDefault(x => x.Id == expense.Id);
            if (expenseToUpdate != null)
            {
                expenseToUpdate.Flow = expense.Flow;
                expenseToUpdate.Date = expense.Date;
                expenseToUpdate.Description = expense.Description;
                expenseToUpdate.Value = expense.Value;

                if (expenseToUpdate.BudgetId != expense.BudgetId)
                {
                    Budget.RemoveExpense(expenseToUpdate);
                }
            }

            if (expense.BudgetId != Budget.Id)
            {
                var expensesToDelete = Budget.Expenses.Where(x => x.BudgetId != Budget.Id).ToList();
                expensesToDelete.ForEach(x => Budget.RemoveExpense(x));
            }

            if (expenseToUpdate == null && expense.BudgetId == Budget.Id)
            {
                Budget.AddExpense(expense.Flow, expense.Value, expense.Description, expense.Date);
            }
        }

        private void Delete(Expense expense)
        {
            Diagnostics.Start();
            using (var tx = Database.GetTransaction())
            {
                Database.Delete<SavingValue>("WHERE ExpenseId = @0", expense.Id);
                Database.Delete(expense);

                tx.Complete();

                DeleteFromCurrentBudget(expense);
            }
            PublishRefreshRequest(expense);
            Diagnostics.Stop();
        }

        private void DeleteFromCurrentBudget(Expense expense)
        {
            var expenseDelete = Budget.Expenses.FirstOrDefault(x => x.Id == expense.Id);
            if (expenseDelete != null)
            {
                Budget.RemoveExpense(expenseDelete);
            }
        }

        public void MoveToExpenseValue()
        {
            Diagnostics.Start();
            IsExpenseValueFocused = false;
            IsExpenseValueFocused = true;
            Diagnostics.Stop();
        }

        public void AddAndMoveToExpenseValue()
        {
            Diagnostics.Start();
            if (CanAddExpense)
            {
                AddExpense();
            }
            MoveToExpenseValue();
            Diagnostics.Stop();
        }


        #endregion

        public ExpensesFilterVM Filter { get; set; }

        private void FilterBudgetExpenses()
        {
            Diagnostics.Start();

            #region Predicate implementation (OLD)
            //if (IsFilteringEnabled)
            //{
            //    var predicate = PredicateBuilder.True<Expense>();

            //    if (Filter.CashFlow != null || Filter.CashFlowGroup != null)
            //    {
            //        var flowPredicate = PredicateBuilder.False<Expense>();
            //        if (Filter.CashFlowGroup != null)
            //        {
            //            flowPredicate = flowPredicate.And(p => Filter.CashFlowGroup.Id == p.CashFlowGroupId);
            //        }
            //        if (Filter.CashFlow != null)
            //        {
            //            flowPredicate = flowPredicate.And(p => Filter.CashFlow.Id == p.Id);
            //        }
            //    }

            //    if (Filter.CashFlowGroup != null && !Filter.CashFlowGroup.IsTransient())
            //    {
            //        predicate = predicate.And(p => Filter.CashFlowGroup.Id == p.CashFlowGroupId);
            //    }

            //    if (Filter.CashFlow != null && !Filter.CashFlow.IsTransient())
            //    {
            //        predicate = predicate.And(p => Filter.CashFlow.Id == p.CashFlowId);
            //    }


            //    predicate = predicate.And(p => p.Date >= Filter.DateFrom && p.Date <= Filter.DateTo);
            //    if (Filter.ValueFrom.HasValue)
            //    {
            //        predicate = predicate.And(p => p.Value >= Filter.ValueFrom.Value);
            //    }
            //    if (Filter.ValueTo.HasValue)
            //    {
            //        predicate = predicate.And(p => p.Value <= Filter.ValueTo.Value);
            //    }

            //    if (!string.IsNullOrWhiteSpace(Filter.Description))
            //    {
            //        predicate = predicate.And(p => p.Description.Contains(Filter.Description, false));
            //    }
            //    BudgetExpenses = _budgetExpenses.AsQueryable().Where(predicate).ToList();
            //}
            //else
            //{
            //    BudgetExpenses = _budgetExpenses.ToList();
            //}
            #endregion Predicate implementation (OLD)

            DeatachEventsFromFilteredExpenses();
            BudgetExpenses.Clear();
            BudgetExpenses.AddRange(GetFilteredExpenses());
            AttachEventsToFilteredExpenses();
            //if (_budgetExpenses.Any())
            //{
            //    IsExpensesFiltered = BudgetExpenses.Count != _budgetExpenses.Count;
            //}
            //else
            //{
            //    IsExpensesFiltered = false;
            //}

            CurrentBudgetExpensesTotalCount = Database.Count<Expense>("BudgetId = @0", Budget.Id);
            CurrentBudgetExpensesFilteredCount = BudgetExpenses.Count(x => x.IsCurrentBudget);

            NotifyOfPropertyChange(() => BudgetExpensesFilteredCount);
            NotifyOfPropertyChange(() => TotalExpensesValue);
            NotifyOfPropertyChange(() => BudgetExpenses);
            Diagnostics.Stop();
        }

        private void AttachEventsToFilteredExpenses()
        {
            BudgetExpenses.PropertyChanged += OnFilteredBudgetExpensesPropertyChanged;
            BudgetExpenses.CollectionChanged += OnFilteredBudgetExpensesCollectionChanged;
        }

        private void OnFilteredBudgetExpensesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    Delete(((ExpenseVM)e.OldItems[0]).WrappedItem);
                    break;
            }
        }

        private void OnFilteredBudgetExpensesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var expense = (ExpenseVM)sender;
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(string.Format("Zmieniasz pozycję, która wykracza poza wybrany okres budżetowy: {0:yyyy-MM}", Budget.DateFrom));
            messageBuilder.AppendLine("Może to spowodować przeniesienie tej pozycji do innego okresu budżetowego.");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(string.Format("Grupa: {0}", expense.GroupName));
            messageBuilder.AppendLine(string.Format("Kategoria: {0}", expense.Flow.Name));
            messageBuilder.AppendLine(string.Format("Data: {0:yyyy-MM-dd}", expense.Date));
            messageBuilder.AppendLine(string.Format("Kwota: {0:C}", expense.Value));
            messageBuilder.AppendLine(string.Format("Opis: {0}", expense.Description));
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("Na pewno chcesz to zrobić?");

            if (expense.Date < expense.BudgetDateFrom || expense.Date > expense.BudgetDateTo)
            {
                Shell.ShowQuestion(messageBuilder.ToString(),
                    () => Save(expense.WrappedItem),
                    () => expense.UndoChanges());
                return;
            }

            if (expense.Date < Budget.DateFrom || expense.Date > Budget.DateTo)
            {
                Shell.ShowQuestion(messageBuilder.ToString(),
                    () => Save(expense.WrappedItem),
                    () => expense.UndoChanges());
                return;
            }

            Save(expense.WrappedItem);
        }

        private void DeatachEventsFromFilteredExpenses()
        {
            BudgetExpenses.PropertyChanged -= OnFilteredBudgetExpensesPropertyChanged;
            BudgetExpenses.CollectionChanged -= OnFilteredBudgetExpensesCollectionChanged;
        }

        private List<ExpenseVM> GetFilteredExpenses()
        {
            Diagnostics.Start();
            var sql = PetaPoco.Sql.Builder
                    .Select("*")
                    .From("Expense")
                    .InnerJoin("Budget")
                    .On("Budget.Id = Expense.BudgetId")
                    .InnerJoin("CashFlow")
                    .On("CashFlow.Id = Expense.CashFlowId")
                    .InnerJoin("CashFlowGroup")
                    .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id");

            var whereItems = new List<PetaPoco.Sql>();
            if (Filter.CashFlow != null || Filter.CashFlowGroup != null)
            {
                if (Filter.CashFlowGroup != null)
                {
                    whereItems.Add(new PetaPoco.Sql("CashFlowGroup.Id = @0", Filter.CashFlowGroup.Id));
                }
                if (Filter.CashFlow != null)
                {
                    whereItems.Add(new PetaPoco.Sql("CashFlow.Id = @0", Filter.CashFlow.Id));
                }
            }

            if (Filter.CashFlowGroup != null && !Filter.CashFlowGroup.IsTransient())
            {
                whereItems.Add(new PetaPoco.Sql("CashFlowGroup.Id = @0", Filter.CashFlowGroup.Id));
            }

            if (Filter.CashFlow != null && !Filter.CashFlow.IsTransient())
            {
                whereItems.Add(new PetaPoco.Sql("CashFlow.Id = @0", Filter.CashFlow.Id));
            }

            whereItems.Add(new PetaPoco.Sql("date(Expense.Date) BETWEEN date(@0) AND date(@1)", Filter.DateFrom, Filter.DateTo));

            if (Filter.ValueFrom.HasValue)
            {
                whereItems.Add(new PetaPoco.Sql("Expense.Value >= @0", Filter.ValueFrom.Value));
            }
            if (Filter.ValueTo.HasValue)
            {
                whereItems.Add(new PetaPoco.Sql("Expense.Value <= @0", Filter.ValueTo.Value));
            }
            if (!string.IsNullOrWhiteSpace(Filter.Description))
            {
                whereItems.Add(new PetaPoco.Sql("Expense.Description LIKE @0", string.Format("%{0}%", Filter.Description)));
            }

            if (whereItems.Any())
            {
                var whereSql = PetaPoco.Sql.Builder;
                for (int i = 0; i < whereItems.Count; i++)
                {
                    var tmpSql = whereItems[i];
                    if (i == 0)
                    {
                        whereSql = whereSql.Append("WHERE").Append(tmpSql);
                    }
                    else
                    {
                        whereSql = whereSql.Append("AND").Append(tmpSql);
                    }
                }

                sql = sql.Append(whereSql);
            }

            var expenses = Database.Query<Expense, Budget, CashFlow, CashFlowGroup>(sql).ToList();
            expenses.ForEach(x =>
            {
                x.Flow.Saving = Database.SingleOrDefault<Saving>("WHERE CashFlowId = @0", x.CashFlowId);
                x.SavingValue = Database.SingleOrDefault<SavingValue>("WHERE ExpenseId = @0", x.Id);
                if (x.SavingValue != null)
                {
                    x.SavingValue.Expense = x;
                    x.SavingValue.Saving = x.Flow.Saving;
                    x.SavingValue.Budget = x.Budget;
                }
            });
            Diagnostics.Stop();
            return expenses.Select(x => new ExpenseVM(x, Budget.Id)).ToList();
        }

        public void Export()
        {
            try
            {
                var fileService = IoC.Get<IFileService>();
                var defaultFileName = string.Format("Wydatki_{0}_{1}", Filter.DateFrom.ToString("yyyy-MM-dd"), Filter.DateTo.ToString("yyyy-MM-dd"));
                var xlsFile = fileService.SaveExcelFile(defaultFileName);
                if (!string.IsNullOrWhiteSpace(xlsFile))
                {
                    var exportService = IoC.Get<IExportService>();
                    exportService.ExportExpenses(xlsFile, "Wydatki", BudgetExpenses.Select(x => x.WrappedItem));
                    Process.Start(xlsFile);
                }
            }
            catch (IOException)
            {
                Shell.ShowMessage("Brak dostępu do pliku. Być może jest on już otwarty...", null, null, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Shell.ShowMessage(ex.Message, null, null, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        #region IHandle<ExpensesFilterVM> Members

        public void Handle(ExpensesFilterVM message)
        {
            Diagnostics.Start();
            if (Budget != null)
            {
                RefreshUI();
            }
            Diagnostics.Stop();
        }

        #endregion
    }
}
