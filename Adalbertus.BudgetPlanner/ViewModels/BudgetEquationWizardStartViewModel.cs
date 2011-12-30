﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class BudgetEquationWizardStartViewModel : WizardPageViewModel<BudgetEquationWizardVM>
    {
        public BudgetEquationWizardStartViewModel()
        {
            NextPageName = typeof(BudgetEquationWizardElementViewModel).Name;
            Title = "Podstawowe informacje";
        }

        public string EquationName
        {
            get { return Model.EquationName; }
            set
            {
                Model.EquationName = value;
                Parent.RefreshNavigationUI();
            }
        }

        public bool IsVisible {
            get { return Model.IsVisible; }
            set
            {
                Model.IsVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        protected override void OnModelLoaded()
        {
            if (Model != null)
            {
                Model.PropertyChanged += (s, e) => { NotifyOfPropertyChange(e.PropertyName); };
            }
        }

        protected override bool ValidateMoveNext()
        {
            if (string.IsNullOrWhiteSpace(EquationName))
            {
                return false;
            }

            return base.ValidateMoveNext();
        }
    }
}
