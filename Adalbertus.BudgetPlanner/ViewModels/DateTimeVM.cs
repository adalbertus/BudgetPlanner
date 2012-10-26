using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class DateTimeVM : PropertyChangedBase
    {
        #region Private fields
        #endregion Private fields

        #region Constructors
        public DateTimeVM()
            : this(DateTime.Now, "yyyy-MM-dd")
        {
        }
        
        public DateTimeVM(string dateTime)
            : this(DateTime.Parse(dateTime), "yyyy-MM-dd")
        {
        }

        public DateTimeVM(DateTime dateTime)
            : this(dateTime, "yyyy-MM-dd")
        {
            
        }

        public DateTimeVM(string dateTime, string format)
            : this(DateTime.Parse(dateTime), format)
        {
        }

        public DateTimeVM(DateTime dateTime, string format)
        {
            DateTime = dateTime;
            Format = format;
        }
        #endregion Constructors

        #region Properties
        private DateTime _dateTime;
        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                NotifyOfPropertyChange(() => DateTime);
                NotifyOfPropertyChange(() => DateTimeFormatted);
            }
        }

        public string Format { get; set; }

        public string DateTimeFormatted { 
            get
            {
                if (string.IsNullOrWhiteSpace(Format))
                {
                    return DateTime.ToString();
                }
                return DateTime.ToString(Format);
            }
        }
        #endregion Properties

        #region Public methods
        public override string ToString()
        {
            return DateTimeFormatted;
        }

        public DateTimeVM AddMonths(int months)
        {
            return new DateTimeVM(DateTime.AddMonths(months), Format);
        }
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
