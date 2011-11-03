using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.Models
{
    public abstract class Entity : PropertyChangedBase //, IEntity
    {
        [PetaPoco.Column]
        public int Id { get; set; }

        private int? requestedHashCode;

        public virtual bool Equals(Entity other)
        {
            if (null == other || !GetType().IsInstanceOfType(other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            bool otherIsTransient = Equals(other.Id, default(int));
            bool thisIsTransient = IsTransient();
            if (otherIsTransient && thisIsTransient)
            {
                return ReferenceEquals(other, this);
            }

            return other.Id.Equals(Id);
        }

        public virtual bool IsTransient()
        {
            return Equals(Id, default(int));
        }

        public override bool Equals(object obj)
        {
            var that = obj as Entity;
            return Equals(that);
        }

        public override int GetHashCode()
        {
            if (!requestedHashCode.HasValue)
            {
                requestedHashCode = IsTransient() ? base.GetHashCode() : Id.GetHashCode();
            }
            return requestedHashCode.Value;
        }
    }
}
