using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public static class StringExtenstions
    {
        public static bool IsEqual(this string source, string target, bool isCaseSensitive = false)
        {
            bool isSourceEmpty = string.IsNullOrWhiteSpace(source);
            bool isTargetEmpty = string.IsNullOrWhiteSpace(target);

            if (isSourceEmpty && !isTargetEmpty)
            {
                return false;
            }
            
            if (!isSourceEmpty && isTargetEmpty)
            {
                return false;
            }

            if (isSourceEmpty && isTargetEmpty)
            {
                return true;
            }

            if (isCaseSensitive)
            {
                return source.Contains(target);
            }
            else
            {
                return source.ToLowerInvariant().Contains(target.ToLowerInvariant());
            }
        }
    }
}
