using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("CashFlowGroup")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class CashFlowGroup : Entity
    {
        private string _name;
        [PetaPoco.Column]
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        private string _description;
        [PetaPoco.Column]
        public virtual string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        private int _position;
        [PetaPoco.Column]
        public int Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                NotifyOfPropertyChange(() => Position);
            }
        }

        [PetaPoco.Column]
        public bool IsReadOnly { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
