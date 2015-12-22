using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assassination.Helpers
{
    public static class RandomExtension
    {
        public static void Shuffle<T>(this Random random, T[] array)
        {
            int length = array.Length;
            while (length > 1)
            {
                int position = random.Next(length--);
                T holder = array[length];
                array[length] = array[position];
                array[position] = holder;
            }
        }
    }
}