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
            NotifyOfPropertyChange(() => Text);
        }

        public override void Close()
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(_note);
                tx.Complete();
            }
            base.Close();
        }
    }
}
