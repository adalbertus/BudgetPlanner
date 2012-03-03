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
    public class NotepadViewModel : BaseDailogViewModel
    {
        public NotepadViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            AvaiableBudgetDates = new BindableCollectionExt<dynamic>();
        }

        public BindableCollectionExt<dynamic> AvaiableBudgetDates { get; private set; }
        public Budget CurrentBudget { get; set; }
        private dynamic _selectedBudget;

        public dynamic SelectedBudget
        {
            get { return _selectedBudget; }
            set
            {
                if (_selectedBudget == value)
                {
                    return;
                }

                if (_selectedBudget != null)
                {
                    SaveBudgetNote();
                }
                _selectedBudget = value;
                LoadBudgetNote();
                NotifyOfPropertyChange(() => SelectedBudget);
            }
        }

        private Note _note;
        public string Text
        {
            get { return _note.Text; }
            set
            {
                _note.Text = value;
                NotifyOfPropertyChange(() => Text);
            }
        }

        private Note _budgetNote;

        public Note BudgetNote
        {
            get { return _budgetNote; }
            set
            {
                _budgetNote = value;
                NotifyOfPropertyChange(() => BudgetNote);
            }
        }

        public override void Initialize(dynamic parameters)
        {
            CurrentBudget = parameters.CurrentBudget;
        }

        public override void LoadData()
        {
            using (var tx = Database.GetTransaction())
            {
                if (Database.Count<Note>() == 0)
                {
                    _note = new Note();
                    Database.Save(_note);
                }
                else
                {
                    _note = Database.Query<Note>().ToList().First();
                }
                tx.Complete();
            }
            LoadAvaiableBudgets();
            NotifyOfPropertyChange(() => Text);
        }

        private void LoadAvaiableBudgets()
        {
            var avaiableBudgets = Database.Query<Budget>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("[Budget]")
                    .OrderBy("[DateFrom]")).ToList();

            AvaiableBudgetDates.Clear();
            avaiableBudgets.ForEach(x => AvaiableBudgetDates.Add(new { 
                BudgetId = x.Id, 
                Name = string.Format("Budżet {0}", x.DateFrom.ToString("yyyy-MM"))
            }));

            SelectedBudget = AvaiableBudgetDates.First(x => x.BudgetId == CurrentBudget.Id);

            NotifyOfPropertyChange(() => AvaiableBudgetDates);
        }

        private void LoadBudgetNote()
        {
            if (SelectedBudget == null)
            {
                return;
            }
            if (Database.ExecuteScalar<int>("SELECT COUNT(*) FROM Note WHERE BudgetId = @0", SelectedBudget.BudgetId) == 0)
            {
                BudgetNote = new Note
                {
                    BudgetId = SelectedBudget.BudgetId,
                };
                Save(BudgetNote);
            }
            else
            {                
                BudgetNote = Database.FirstOrDefault<Note>("WHERE BudgetId = @0", SelectedBudget.BudgetId);
            }
        }

        private void SaveBudgetNote()
        {
            if (BudgetNote == null)
            {
                return;
            }
            Save(BudgetNote);
        }

        public override void Close()
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(_note);
                if (BudgetNote != null)
                {
                    Database.Save(BudgetNote);
                }
                tx.Complete();
            }
            base.Close();
        }
    }
}
