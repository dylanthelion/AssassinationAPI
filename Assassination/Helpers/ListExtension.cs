using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assassination.Helpers
{
    public static class ListExtension
    {
        public static T Next<T>(this List<T> queryResults, T currentEntry)
        {
            return queryResults[(queryResults.IndexOf(currentEntry) + 1) == queryResults.Count ? 0 : (queryResults.IndexOf(currentEntry) + 1)];
        }
    }
}