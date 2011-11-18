﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Core;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExpensesFilterVM : PropertyChangedBase
    {
        public BindableCollectionExt<ExpensesFilterCashFlowVM> CashFlows { get; set; }

        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set
            {
                _dateFrom = value;
                NotifyOfPropertyChange(() => DateFrom);
            }
        }

        private DateTime _dateTo;
        public DateTime DateTo
        {
            get { return _dateTo; }
            set
            {
                _dateTo = value;
                NotifyOfPropertyChange(() => DateTo);
            }
        }

        private decimal? _valueFrom;

        public decimal? ValueFrom
        {
            get { return _valueFrom; }
            set
            {
                _valueFrom = value;
                NotifyOfPropertyChange(() => ValueFrom);
            }
        }

        private decimal? _valueTo;

        public decimal? ValueTo
        {
            get { return _valueTo; }
            set
            {
                _valueTo = value;
                NotifyOfPropertyChange(() => ValueTo);
            }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public ExpensesFilterVM()
        {
            CashFlows = new BindableCollectionExt<ExpensesFilterCashFlowVM>();
        }

    }
}
