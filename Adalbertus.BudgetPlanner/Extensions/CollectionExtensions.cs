using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static T GetOrDefaultByIndex<T>(this IEnumerable<T> items, int index)
        {
            if (!items.Any())
            {
                return default(T);
            }

            if (index + 1 > items.Count())
            {
                return default(T);
            }

            return items.ToArray()[index];
        }
    }
}
