﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public static class DateTimeExtensions
    {
        public static int YearMonthToInt(this DateTime date)
        {
            if (date == null)
            {
                return Int32.MinValue;
            }

            return Convert.ToInt32(date.ToString("yyyyMM"));
        }

        public static DateTime SetMonthDay(this DateTime date, int monthDay)
        {
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            if (monthDay > daysInMonth)
            {
                return new DateTime(date.Year, date.Month, daysInMonth);
            }
            else
            {
                return new DateTime(date.Year, date.Month, monthDay);
            }
        }

        public static DateTime ToDateOnly(this DateTime dateTime)
        {
            return DateTime.Parse(dateTime.ToShortDateString());
        }

        public static bool IsBetween(this DateTime dateTime, DateTime fromDate, DateTime toDate)
        {
            var result = (dateTime.ToDateOnly() >= fromDate.ToDateOnly() && dateTime.ToDateOnly() <= toDate.ToDateOnly());

            return result;
        }
    }
}
