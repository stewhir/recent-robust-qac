using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace QACExperimenter
{
    public class Utilities
    {
        /// <summary>
        /// Implements a sigmoid function with the parameters: y = max · k n / (k n + x n)
        /// (Returns Y value).
        /// See: http://numberlinx.org/Sigmoid%20Curves.html for visual parameter tuning examples.
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="k">Half-maximum</param>
        /// <param name="n">Steepness</param>
        /// <returns></returns>
        public static double SigmoidFunction(double x, double max, double k, double n)
        {
            return max * (Math.Pow(x, n) / (Math.Pow(k, n) + Math.Pow(x, n)));
        }

        public static int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1)).Seconds;
        }

        /// <summary>
        /// Extract the prefix from a query, returns the prefix of length prefixLength, 
        /// or null if the query is not long enough to extract a prefix from
        /// </summary>
        /// <param name="query"></param>
        /// <param name="prefixLength"></param>
        /// <returns></returns>
        public static string GetPrefix(string query, int prefixLength)
        {
            if (query.Length >= prefixLength)
                return query.Substring(0, prefixLength);

            return null;
        }

        public static int CountInstancesOfCharacters(string inString, char character)
        {
            int count = 0;

            foreach (char c in inString)
            {
                if (c == character)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Data directory, where input and output data files are kept
        /// </summary>
        public static string DataDirectory
        {
            get { return ConfigurationManager.AppSettings["DataDirectory"].ToString(); }
        }
    }
}
