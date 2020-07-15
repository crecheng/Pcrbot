using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.pcrbot._1.Code
{
    public static class MyRandom
    {
        public static int GetRandom()
        {
            Random random = new Random(GetGuidHashCode());
            int r = random.Next();
            return r;
        }

        public static int Next()
        {
            return GetRandom();
        }

        public static int NextTo(int max)
        {
            return GetRandom() % max;
        }

        public static int NextBetween(int min, int max)
        {
            return (GetRandom() % (max - min)) + min;
        }

        public static double Nextdouble()
        {
            Random random = new Random(GetGuidHashCode());
            return random.NextDouble();
        }

        static int GetGuidHashCode()
        {
            return Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
        }
    }
}
