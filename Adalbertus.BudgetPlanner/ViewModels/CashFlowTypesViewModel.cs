using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Collections.Specialized;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;
using System.Diagnostics;
using Microsoft.Windows.Controls;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class CashFlowTypesViewModel : BaseViewModel
    {
        public CashFlowTypesViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            _cashFlows = new BindableCollectionExt<CashFlow>();
            _cashFlows.PropertyChanged += (s, e) => 
            { 
                OnPropertyChanged(s, e);
                CachedService.Clear(CachedServiceKeys.AllCashFlows);
            };

            _cashFlowGroups = new BindableCollectionExt<CashFlowGroup>();
            _cashFlowGroups.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(s, e);

                CachedService.Clear(CachedServiceKeys.AllCashFlowGroups);
                CachedService.Clear(CachedServiceKeys.AllCashFlows);
                var cashFlowGroup = s as CashFlowGroup;
                _cashFlows.Where(x => x.CashFlowGroupId == cashFlowGroup.Id)
                    .ForEach(x => x.Group = cashFlowGroup);
                NewCashFlowGroup = null;
                NewCashFlowGroup = CashFlowGroups.First();
            };
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LoadData();
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                case "Description":
                case "Group":
                case "Position":
                    Save(sender as Entity);
                    break;
            }
        }

        public override void LoadData()
        {
            LoadCashFlowGroups();
            LoadCashFlows();
        }

        private void LoadCashFlows()
        {
            _cashFlows.IsNotifying = false;
            _cashFlows.Clear();
            var sql = PetaPoco.Sql.Builder
                    .Select("*")
                    .From("CashFlow")
                    .InnerJoin("CashFlowGroup")
                    .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                    .LeftJoin("Saving")
                    .On("Saving.CashFlowId = CashFlow.Id");
            var cashFlowList = Database.Query<CashFlow, CashFlowGroup, Saving>(sql).ToList();
            cashFlowList.ForEach(x =>
            {
                if (x.Saving.IsTransient())
                {
                    x.Saving = null;
                }
            });
            cashFlowList.ForEach(x => _cashFlows.Add(x));
            _cashFlows.IsNotifying = true;
            NotifyOfPropertyChange(() => CashFlows);
        }

        private void LoadCashFlowGroups()
        {
            _cashFlowGroups.IsNotifying = false;
            _cashFlowGroups.Clear();
            var cashFlowGroups = CachedService.GetAllCashFlowGroups();
            cashFlowGroups.ForEach(x => _cashFlowGroups.Add(x));


            _cashFlowGroups.IsNotifying = true;

            NewCashFlowGroup = CashFlowGroups.First();
            NotifyOfPropertyChange(() => CashFlowGroups);
        }

        #region View controls navigation
        public void MoveToNewDescription()
        {
            IsNewDescriptionFocused = false;
            IsNewDescriptionFocused = true;
        }

        public void MoveToNewGroupDescription()
        {
            IsNewGroupDescriptionFocused = false;
            IsNewGroupDescriptionFocused = true;
        }

        private bool _isNewNameFocused;
        public bool IsNewNameFocused
        {
            get { return _isNewNameFocused; }
            set
            {
                _isNewNameFocused = value;
                NotifyOfPropertyChange(() => IsNewNameFocused);
            }
        }

        private bool _isNewGroupNameFocused;
        public bool IsNewGroupNameFocused
        {
            get { return _isNewGroupNameFocused; }
            set
            {
                _isNewGroupNameFocused = value;
                NotifyOfPropertyChange(() => IsNewGroupNameFocused);
            }
        }

        private bool _isNewDescriptionFocused;
        public bool IsNewDescriptionFocused
        {
            get { return _isNewDescriptionFocused; }
            set
            {
                _isNewDescriptionFocused = value;
                NotifyOfPropertyChange(() => IsNewDescriptionFocused);
            }
        }

        private bool _isNewGroupDescriptionFocused;
        public bool IsNewGroupDescriptionFocused
        {
            get { return _isNewGroupDescriptionFocused; }
            set
            {
                _isNewGroupDescriptionFocused = value;
                NotifyOfPropertyChange(() => IsNewGroupDescriptionFocused);
            }
        }
        #endregion

        #region Adding new cash flow type
        private string _newName;
        public string NewName
        {
            get { return _newName; }
            set
            {
                _newName = value;
                NotifyOfPropertyChange(() => NewName);
                NotifyOfPropertyChange(() => CanAddNewCashFlowType);
            }
        }

        private string _newDescription;
        public string NewDescription
        {
            get { return _newDescription; }
            set
            {
                _newDescription = value;
                NotifyOfPropertyChange(() => NewDescription);
            }
        }

        private CashFlowGroup _newCashFlowGroup;
        public CashFlowGroup NewCashFlowGroup
        {
            get { return _newCashFlowGroup; }
            set
            {
                _newCashFlowGroup = value;
                NotifyOfPropertyChange(() => NewCashFlowGroup);
            }
        }

        public bool CanAddNewCashFlowType
        {
            get
            {
                bool isNameNotEmpty = !string.IsNullOrWhiteSpace(NewName);
                bool isGroupSelected = NewCashFlowGroup != null;
                return isNameNotEmpty && isGroupSelected;
            }
        }

        public void AddNewCashFlowType()
        {
            using (var tx = Database.GetTransaction())
            {
                var cashFlow = new CashFlow
                {
                    Name = NewName,
                    Description = NewDescription,
                    Group = NewCashFlowGroup,
                };

                Database.Save(cashFlow);
                tx.Complete();
                _cashFlows.Add(cashFlow);
            }
            CachedService.Clear();
            NewName = string.Empty;
            NewDescription = string.Empty;
            NewCashFlowGroup = CashFlowGroups.First();
            //LoadData();
            IsNewNameFocused = false;
            IsNewNameFocused = true;
            NotifyOfPropertyChange(() => CashFlows);
        }

        #endregion

        #region Adding new cash flow group
        private string _newGroupName;
        public string NewGroupName
        {
            get { return _newGroupName; }
            set
            {
                _newGroupName = value;
                NotifyOfPropertyChange(() => NewGroupName);
                NotifyOfPropertyChange(() => CanAddNewCashFlowGroup);
            }
        }

        private string _newGroupDescription;
        public string NewGroupDescription
        {
            get { return _newGroupDescription; }
            set
            {
                _newGroupDescription = value;
                NotifyOfPropertyChange(() => NewGroupDescription);
            }
        }

        public bool CanAddNewCashFlowGroup
        {
            get
            {
                return !string.IsNullOrWhiteSpace(NewGroupName);
            }
        }

        public void AddNewCashFlowGroup()
        {
            using (var tx = Database.GetTransaction())
            {
                var maxPosition = Database.ExecuteScalar<int>("SELECT MAX(Position) FROM CashFlowGroup");
                var cashFlowGroup = new CashFlowGroup
                {
                    Name = NewGroupName,
                    Description = NewGroupDescription,
                    Position = maxPosition + 1,
                };

                Database.Save(cashFlowGroup);
                tx.Complete();
                _cashFlowGroups.Add(cashFlowGroup);
            }
            CachedService.Clear();
            NewGroupName = string.Empty;
            NewGroupDescription = string.Empty;
            //LoadData();
            IsNewGroupNameFocused = false;
            IsNewGroupNameFocused = true;
            NotifyOfPropertyChange(() => CashFlowGroups);
        }

        #endregion

        #region Cash flow type list
        private BindableCollectionExt<CashFlow> _cashFlows;
        public IEnumerable<CashFlow> CashFlows
        {
            get { return _cashFlows.AsEnumerable().ToList(); }

        }
        private BindableCollectionExt<CashFlowGroup> _cashFlowGroups;
        public IEnumerable<CashFlowGroup> CashFlowGroups
        {
            get { return _cashFlowGroups.OrderBy(x => x.Position).ToList(); }
        }

        public bool CanDeleteCashFlowType(CashFlow cashFlow)
        {
            return !cashFlow.IsReadOnly;
        }

        public void DeleteCashFlowType(CashFlow cashFlow)
        {
            DeleteCashFlowType(cashFlow, false);
        }
        public void DeleteCashFlowType(CashFlow cashFlow, bool omitConfirmation)
        {
            if (!omitConfirmation)
            {
                var hasBudgetPlansDefined = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                    .Select("COUNT(*)")
                    .From("BudgetPlan")
                    .Where("CashFlowId = @0", cashFlow.Id)) > 0;

                var hasExpensesDefined = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                    .Select("COUNT(*)")
                    .From("Expense")
                    .Where("CashFlowId = @0", cashFlow.Id)) > 0;

                if (hasBudgetPlansDefined || hasExpensesDefined)
                {
                    Shell.ShowDialog<CashFlowDeleteConfirmationViewModel>(new { CashFlow = cashFlow }, () => DeleteCashFlowType(cashFlow, true), null);
                    return;
                }
            }

            using (var tx = Database.GetTransaction())
            {
                Database.Delete<BudgetPlan>("WHERE CashFlowId = @0", cashFlow.Id);
                Database.Delete<SavingValue>("WHERE ExpenseId IN (SELECT [Expense].Id FROM [Expense] WHERE CashFlowId = @0)", cashFlow.Id);
                Database.Delete<Expense>("WHERE CashFlowId = @0", cashFlow.Id);
                Database.Delete<CashFlow>(cashFlow);
                tx.Complete();
                _cashFlows.Remove(cashFlow);
            }
            CachedService.Clear();

            NotifyOfPropertyChange(() => CashFlows);
            //LoadData();
        }

        public bool CanDeleteCashFlowGroup(CashFlowGroup cashFlowGroup)
        {
            return _cashFlowGroups.Count > 0;
        }

        public void DeleteCashFlowGroup(CashFlowGroup cashFlowGroup)
        {
            DeleteCashFlowGroup(cashFlowGroup, false);
        }

        public void DeleteCashFlowGroup(CashFlowGroup cashFlowGroup, bool omitConfirmation)
        {
            if (!omitConfirmation)
            {
                var hasCashFlowsDefined = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                    .Select("COUNT(*)")
                    .From("CashFlow")
                    .Where("CashFlowGroupId = @0", cashFlowGroup.Id)) > 0;

                var hasBudgetPlansDefined = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                    .Select("COUNT(*)")
                    .From("BudgetPlan")
                    .Where("CashFlowId IN (SELECT [CashFlow].Id FROM [CashFlow] WHERE CashFlowGroupId = @0)", cashFlowGroup.Id)) > 0;

                var hasExpensesDefined = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                    .Select("COUNT(*)")
                    .From("Expense")
                    .Where("CashFlowId IN (SELECT [CashFlow].Id FROM [CashFlow] WHERE CashFlowGroupId = @0)", cashFlowGroup.Id)) > 0;

                if (hasCashFlowsDefined || hasBudgetPlansDefined || hasExpensesDefined)
                {
                    Shell.ShowDialog<CashFlowGroupDeleteConfirmationViewModel>(new { CashFlowGroup = cashFlowGroup }, () => DeleteCashFlowGroup(cashFlowGroup, true), null);
                    return;
                }
            }

            using (var tx = Database.GetTransaction())
            {
                Database.Delete(cashFlowGroup);
                tx.Complete();
                _cashFlowGroups.IsNotifying = false;
                _cashFlowGroups.Remove(cashFlowGroup);
                _cashFlowGroups.IsNotifying = true;
            }

            NewCashFlowGroup = _cashFlowGroups.First();
            CachedService.Clear();
            _cashFlows.IsNotifying = false;
            NotifyOfPropertyChange(() => CashFlowGroups);
            LoadCashFlows();
        }


        public void MoveCashFlowGroupUp(CashFlowGroup cashFlowGroup)
        {
            var previousCashFlowGroup = CashFlowGroups.LastOrDefault(x => x.Position < cashFlowGroup.Position);
            if (previousCashFlowGroup == null)
            {
                return;
            }

            SwapPositions(cashFlowGroup, previousCashFlowGroup);
        }

        public void MoveCashFlowGroupDown(CashFlowGroup cashFlowGroup)
        {
            var nextCashFlowGroup = CashFlowGroups.FirstOrDefault(x => x.Position > cashFlowGroup.Position);
            if (nextCashFlowGroup == null)
            {
                return;
            }

            SwapPositions(cashFlowGroup, nextCashFlowGroup);
        }

        private void SwapPositions(CashFlowGroup first, CashFlowGroup secound)
        {
            first.IsNotifying = false;
            secound.IsNotifying = false;
            var firstPosition = first.Position;
            first.Position = secound.Position;
            secound.Position = firstPosition;
            first.IsNotifying = true;
            secound.IsNotifying = true;

            base.Save(first);
            base.Save(secound);
            CachedService.Clear();
            NotifyOfPropertyChange(() => CashFlowGroups);
        }



        protected override void Save(Entity entity)
        {
            base.Save(entity);
            CachedService.Clear();
        }
        #endregion
    }
}
