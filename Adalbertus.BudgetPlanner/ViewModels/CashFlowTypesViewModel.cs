﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Collections.Specialized;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;
using System.Diagnostics;
using Microsoft.Windows.Controls;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class CashFlowTypesViewModel : BaseViewModel
    {
        public CashFlowTypesViewModel(IShellViewModel shell, IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            CashFlows = new BindableCollectionExt<CashFlow>();
            CashFlows.PropertyChanged += (s, e) => { OnPropertyChanged(s, e); };

            CashFlowGroups = new BindableCollectionExt<CashFlowGroup>();
            CashFlowGroups.PropertyChanged += (s, e) => { OnPropertyChanged(s, e); };
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
            CashFlowGroups.IsNotifying = false;
            CashFlowGroups.Clear();
            var cashFlowGroups = CachedService.GetAllCashFlowGroups();
            cashFlowGroups.ForEach(x => CashFlowGroups.Add(x));

            CashFlows.IsNotifying = false;
            CashFlows.Clear();
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
            cashFlowList.ForEach(x => CashFlows.Add(x));

            CashFlowGroups.IsNotifying = true;
            CashFlowGroups.Refresh();

            CashFlows.IsNotifying = true;
            CashFlows.Refresh();

            NewCashFlowGroup = CashFlowGroups.First();
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
            }
            CachedService.Clear();
            NewName = string.Empty;
            NewDescription = string.Empty;
            NewCashFlowGroup = CashFlowGroups.First();
            LoadData();
            IsNewNameFocused = false;
            IsNewNameFocused = true;
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
                var cashFlowGroup = new CashFlowGroup
                {
                    Name = NewGroupName,
                    Description = NewGroupDescription
                };

                Database.Save(cashFlowGroup);
                tx.Complete();
            }
            CachedService.Clear();
            NewGroupName = string.Empty;
            NewGroupDescription = string.Empty;
            LoadData();
            IsNewGroupNameFocused = false;
            IsNewGroupNameFocused = true;
        }

        #endregion

        #region Cash flow type list
        public BindableCollectionExt<CashFlow> CashFlows { get; private set; }
        public BindableCollectionExt<CashFlowGroup> CashFlowGroups { get; private set; }
        
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
            }
            CachedService.Clear();

            LoadData();
        }

        public bool CanDeleteCashFlowGroup(CashFlowGroup cashFlowGroup)
        {
            return CashFlowGroups.Count > 1;
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
            }
            CachedService.Clear();
            LoadData();
        }

        protected override void Save(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Update(entity);
                tx.Complete();
                if (entity is CashFlowGroup)
                {
                    LoadData();
                }
            }            
            CachedService.Clear();
        }
        #endregion
    }
}
