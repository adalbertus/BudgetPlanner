using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Extensions;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetTemplateDialogViewModel : BaseDailogViewModel
    {
        public BudgetTemplateDialogViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            CashFlows = new BindableCollectionExt<CashFlow>();
            TemplateItems = new BindableCollectionExt<BudgetTemplateItemVM>();
            TemplateItems.PropertyChanged += (s, e) =>
                {
                    var item = s as BudgetTemplateItemVM;
                    if (e.PropertyName == "IsElementActive")
                    {
                        Save(item.WrappedItem);
                    }
                };
            CurrentTemplateItem = new BudgetTemplateItem();
            CashFlowNameSelector = (cashFlowId) =>
            {
                var cashFlow = CashFlows.FirstOrDefault(x => x.Id == cashFlowId);
                if (cashFlow == null)
                {
                    return "b.d.";
                }
                return cashFlow.Name;
            };
        }

        public Func<int, string> CashFlowNameSelector { get; private set; }

        public Budget CurrentBudget { get; private set; }

        private BudgetTemplateItem _currentTemplateItem;

        public BudgetTemplateItem CurrentTemplateItem
        {
            get { return _currentTemplateItem; }
            set
            {
                _currentTemplateItem = value;
                NotifyOfPropertyChange(() => CurrentTemplateItem);
                NotifyOfPropertyChange(() => IsCurrentItemTransient);
                NotifyOfPropertyChange(() => SelectedCashFlow);

                NotifyOfPropertyChange(() => IsElementActive);
                NotifyOfPropertyChange(() => Value);
                NotifyOfPropertyChange(() => Description);
                NotifyOfPropertyChange(() => MonthDay);
                NotifyOfPropertyChange(() => RepeatInterval);
                NotifyOfPropertyChange(() => StartFromDate);
                NotifyOfPropertyChange(() => IsBudgetPlanChecked);
                NotifyOfPropertyChange(() => IsBudgetExpenseChecked);

                NotifyOfPropertyChange(() => CanAddTemplateItem);
                NotifyOfPropertyChange(() => CanSaveTemplateItem);
            }
        }

        public bool IsCurrentItemTransient { get { return CurrentTemplateItem.IsTransient(); } }

        public BindableCollectionExt<BudgetTemplateItemVM> TemplateItems { get; private set; }

        public BindableCollectionExt<CashFlow> CashFlows { get; private set; }
        private CashFlow _selectedCashFlow;
        public CashFlow SelectedCashFlow
        {
            get
            {
                return _selectedCashFlow;
            }
            set
            {
                _selectedCashFlow = value;
                CurrentTemplateItem.ForeignId = value.Id;
                NotifyOfPropertyChange(() => SelectedCashFlow);
                NotifyOfPropertyChange(() => CanAddTemplateItem);
                NotifyOfPropertyChange(() => CanSaveTemplateItem);
            }
        }

        public bool IsElementActive
        {
            get { return CurrentTemplateItem.IsActive; }
            set
            {
                CurrentTemplateItem.IsActive = value;
                NotifyOfPropertyChange(() => IsElementActive);
            }
        }

        public decimal? Value
        {
            get { return CurrentTemplateItem.Value; }
            set
            {
                CurrentTemplateItem.Value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        public string Description
        {
            get { return CurrentTemplateItem.Description; }
            set
            {
                CurrentTemplateItem.Description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public int MonthDay
        {
            get { return CurrentTemplateItem.MonthDay; }
            set
            {
                CurrentTemplateItem.MonthDay = value;
                NotifyOfPropertyChange(() => MonthDay);
                NotifyOfPropertyChange(() => CanAddTemplateItem);
                NotifyOfPropertyChange(() => CanSaveTemplateItem);
            }
        }

        public int RepeatInterval
        {
            get { return CurrentTemplateItem.RepeatInterval; }
            set
            {
                CurrentTemplateItem.RepeatInterval = value;
                NotifyOfPropertyChange(() => RepeatInterval);
                NotifyOfPropertyChange(() => CanAddTemplateItem);
                NotifyOfPropertyChange(() => CanSaveTemplateItem);
            }
        }

        public DateTime StartFromDate
        {
            get { return CurrentTemplateItem.StartDate; }
            set
            {
                CurrentTemplateItem.StartDate = value;
                NotifyOfPropertyChange(() => StartFromDate);
            }
        }

        private BudgetTemplateType _templateType;

        public bool IsBudgetPlanChecked
        {
            get { return _templateType == BudgetTemplateType.BudgetPlan; }
            set
            {
                if (value)
                {
                    _templateType = BudgetTemplateType.BudgetPlan;
                }
                else
                {
                    _templateType = BudgetTemplateType.BudgetExpense;
                }
                NotifyOfPropertyChange(() => IsBudgetPlanChecked);
                NotifyOfPropertyChange(() => IsBudgetExpenseChecked);
                NotifyOfPropertyChange(() => CanAddTemplateItem);
                NotifyOfPropertyChange(() => CanSaveTemplateItem);
            }
        }

        public bool IsBudgetExpenseChecked
        {
            get { return _templateType == BudgetTemplateType.BudgetExpense; }
            set
            {
                if (value)
                {
                    _templateType = BudgetTemplateType.BudgetExpense;
                }
                else
                {
                    _templateType = BudgetTemplateType.BudgetPlan;
                }
                NotifyOfPropertyChange(() => IsBudgetExpenseChecked);
                NotifyOfPropertyChange(() => IsBudgetPlanChecked);
                NotifyOfPropertyChange(() => CanAddTemplateItem);
                NotifyOfPropertyChange(() => CanSaveTemplateItem);
            }
        }

        public bool CanAddTemplateItem
        {
            get
            {
                return Validate();
            }
        }

        public bool CanSaveTemplateItem
        {
            get
            {
                return Validate();
            }
        }

        private string _confirmationDialogMessage;
        public string ConfirmationDialogMessage
        {
            get { return _confirmationDialogMessage; }
            set
            {
                _confirmationDialogMessage = value;
                NotifyOfPropertyChange(() => ConfirmationDialogMessage);
            }
        }

        private bool _isConfirmationDialogActive;
        public bool IsConfirmationDialogActive
        {
            get { return _isConfirmationDialogActive; }
            set
            {
                _isConfirmationDialogActive = value;
                NotifyOfPropertyChange(() => IsConfirmationDialogActive);
            }
        }

        private BudgetTemplateItemVM _itemToDelete;
        public BudgetTemplateItemVM ItemToDelete
        {
            get { return _itemToDelete; }
            set
            {
                _itemToDelete = value;
                NotifyOfPropertyChange(() => ItemToDelete);
            }
        }

        public override void Initialize(dynamic parameters)
        {
            CurrentBudget = parameters.CurrentBudget;
        }

        public override void LoadData()
        {
            IsConfirmationDialogActive = false;
            CashFlows.Clear();
            var cashFlowList = CachedService.GetAllCashFlows();
            cashFlowList.ForEach(x => CashFlows.Add(x));

            SetDefaults();
            LoadTemplateItems();
        }

        private void LoadTemplateItems()
        {
            var items = Database.Query<BudgetTemplateItem>().ToList();
            items.ForEach(x =>
            {
                var history = Database.Query<BudgetTemplateHistory>("WHERE [BudgetTemplateItemId] = @0", x.Id).ToList();
                history.ForEach(h => x.HistoryItems.Add(h));
            });
            TemplateItems.Clear();
            items.ForEach(x => TemplateItems.Add(new BudgetTemplateItemVM(x, CashFlowNameSelector)));
        }

        private void SetDefaults(BudgetTemplateItem item = null)
        {
            if (item != null)
            {
                CurrentTemplateItem = item;
                SelectedCashFlow = CashFlows.FirstOrDefault(x => x.Id == item.ForeignId);
                IsElementActive = item.IsActive;
                IsBudgetPlanChecked = item.TemplateType == BudgetTemplateType.BudgetPlan;
                Value = item.Value;
                Description = item.Description;
                MonthDay = item.MonthDay;
                RepeatInterval = item.RepeatInterval;
                StartFromDate = item.StartDate;
            }
            else
            {
                CurrentTemplateItem = new BudgetTemplateItem();
                SelectedCashFlow = CashFlows.FirstOrDefault();
                IsElementActive = true;
                IsBudgetPlanChecked = true;
                Value = null;
                Description = string.Empty;
                MonthDay = 1;
                RepeatInterval = 1;
                StartFromDate = CurrentBudget.DateFrom;
            }
        }

        private bool Validate()
        {
            if (SelectedCashFlow == null)
            {
                return false;
            }

            if (MonthDay < 0 || MonthDay > 31)
            {
                return false;
            }

            if (RepeatInterval < 0 || RepeatInterval > 12)
            {
                return false;
            }

            return true;
        }

        public void AddTemplateItem()
        {
            if (!Validate())
            {
                return;
            }

            using (var tx = Database.GetTransaction())
            {
                var templateItem = new BudgetTemplateItem
                {
                    TemplateTypeName = _templateType.ToString(),
                    ForeignId = SelectedCashFlow.Id,
                    Description = Description,
                    IsActive = IsElementActive,
                    MonthDay = MonthDay,
                    RepeatInterval = RepeatInterval,
                    Value = Value,
                    StartDate = StartFromDate
                };
                Database.Save(templateItem);
                tx.Complete();
                TemplateItems.Add(new BudgetTemplateItemVM(templateItem, CashFlowNameSelector));
                NotifyOfPropertyChange(() => CanExecute);
            }
            SetDefaults();
        }

        public void EditTemplateItem(BudgetTemplateItemVM item)
        {
            SetDefaults(item.WrappedItem);
        }

        public void DeleteTemplateItem(BudgetTemplateItemVM item)
        {
            ItemToDelete = item;
            DeleteTemplateItem(false);
        }

        public void DeleteTemplateItem(bool bypassConfirmation)
        {
            if (!bypassConfirmation)
            {
                ConfirmationDialogMessage = "Czy na pewno chcesz usunąć poniższy element?";
                IsConfirmationDialogActive = true;
                return;
            }
            Delete(ItemToDelete.WrappedItem);
            TemplateItems.Remove(ItemToDelete);
            ItemToDelete = null;
            NotifyOfPropertyChange(() => CanExecute);
        }

        public void CancelConfirmationDialog()
        {
            IsConfirmationDialogActive = false;
            ConfirmationDialogMessage = string.Empty;
            ItemToDelete = null;
        }

        public void ConfirmConfirmationDialog()
        {
            IsConfirmationDialogActive = false;
            ConfirmationDialogMessage = string.Empty;
            if (ItemToDelete != null)
            {
                DeleteTemplateItem(true);
            }
            else
            {
                Execute(true);
            }
        }

        public void CancelTemplateItem()
        {
            SetDefaults();
        }

        public void SaveTemplateItem()
        {
            if (CurrentTemplateItem != null)
            {
                Save(CurrentTemplateItem);
                var templateItem = TemplateItems.FirstOrDefault(x => x.WrappedItem.Id == CurrentTemplateItem.Id);
                if (templateItem != null)
                {
                    templateItem.Refresh();
                }
            }
            SetDefaults();
        }

        public bool CanExecute
        {
            get
            {
                return TemplateItems.Any();
            }
        }

        public void Execute()
        {
            Execute(false);
        }

        public void Execute(bool bypassConfirmation)
        {
            if (!bypassConfirmation)
            {
                if (Database.AreBudgetTemplatesApplied(CurrentBudget))
                {
                    ItemToDelete = null;
                    ConfirmationDialogMessage = "Szablon był już raz wykonany na tym budżecie.\r\nWykonać jeszcze raz?";
                    IsConfirmationDialogActive = true;
                    return;
                }
            }

            TemplateItems.Where(x => x.WrappedItem.IsActive).ForEach(x => ApplyTemplate(x.WrappedItem));

            Close();
        }

        private void ApplyTemplate(BudgetTemplateItem item)
        {
            var cashFlow = CashFlows.First(x => x.Id == item.ForeignId);
            var result = item.ApplyToBudget(CurrentBudget, cashFlow, item.Value.GetValueOrDefault(0), item.Description);
            if (result == null)
            {
                return;
            }
            using (var tx = Database.GetTransaction())
            {
                Database.Save(result);
                Database.Save(item);
                var historyToSave = item.HistoryItems.Where(x => x.IsTransient()).ToList();
                historyToSave.ForEach(x => Database.Save(x));
                tx.Complete();

                PublishRefreshRequest(result);
            }
        }
    }
}
