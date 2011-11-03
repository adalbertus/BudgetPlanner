using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.Models
{
    [PetaPoco.TableName("CashFlow")]
    [PetaPoco.PrimaryKey("Id")]
    [PetaPoco.ExplicitColumns]
    public class CashFlow : Entity
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
        
        public virtual Saving Saving { get; set; }

        [PetaPoco.Column]
        public int CashFlowGroupId { get { return Group.Id; } set { } }
        public string GroupName { get { return Group.ToString(); } }
        private CashFlowGroup _group;
        public CashFlowGroup Group 
        { 
            get { return _group; }
            set {
                _group = value; 
                NotifyOfPropertyChange(() => Group);
            }
        }

        public virtual bool IsReadOnly { get { return Saving != null; } }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                return Name;
            }
            return string.Format("{0} [{1}]", Name, Description);
        }

        [Obsolete("Moved to Saving object", true)]
        public static CashFlow CreateForSaving(Saving saving)
        {
            return new CashFlow
            {
                Name = saving.Name,
                Description = "Oszczędności",
                Saving = saving
            };
        }
    }
}
