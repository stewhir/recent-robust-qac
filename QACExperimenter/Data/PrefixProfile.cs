using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Amib.Threading;

namespace QACExperimenter.Data
{
    /// <summary>
    /// Maintains a profile of all the prefixes such that their frequency corresponds to their importance.
    /// </summary>
    public class PrefixProfile
    {
        private Dictionary<string, WorkItemPriority> _prefixPriority;

        private int _maxQueriesToSample = 4000000;

        private FileInfo _queryLog;
        private int _prefixLength;

        public PrefixProfile(FileInfo queryLog, int prefixLength)
        {
            _prefixPriority = new Dictionary<string, WorkItemPriority>();

            _queryLog = queryLog;
            _prefixLength = prefixLength;

            ParseQueryLog();
        }

        /// <summary>
        /// Parse the query prefixes out of the query log
        /// </summary>
        private void ParseQueryLog()
        {
            Console.WriteLine("Profiling " + _maxQueriesToSample + " queries from query log for prefix distribution.");

            StreamReader queryLogFile = new StreamReader(_queryLog.FullName);
            int queryCount = 0;
            Dictionary<string, int> prefixCounts = new Dictionary<string, int>();

            string queryLine = queryLogFile.ReadLine();
            while (queryLine != null)
            {
                // In the format query{TAB}timestamp
                string[] rows = queryLine.Split('\t');

                string prefix = Utilities.GetPrefix(rows[0], _prefixLength);
                if (prefix == null)
                {
                    queryLine = queryLogFile.ReadLine();
                    continue;
                }

                // Update the count
                if (!prefixCounts.ContainsKey(prefix))
                    prefixCounts[prefix] = 1;
                else
                    prefixCounts[prefix] += 1;

                queryCount++;

                queryLine = queryLogFile.ReadLine();

                if (queryCount > _maxQueriesToSample)
                    break;
            }
            Console.WriteLine("Finished gathering queries.");

            // Now sort and select the prefix for each priority
            List<PrefixCount> sortedPrefixCounts = new List<PrefixCount>();

            foreach (KeyValuePair<string, int> kvp in prefixCounts)
            {
                PrefixCount sortedPrefixCount = new PrefixCount(kvp.Key, kvp.Value);
                sortedPrefixCounts.Add(sortedPrefixCount);
            }

            Console.WriteLine("Sorting prefix counts.");
            // Sort descending
            sortedPrefixCounts.Sort();

            // Determine priority for each threshold
            double[] thresholds = new double[] { 0.05, 0.2, 0.5, 0.8, 1.0 }; // Cumulative thresholds, total to 1.
            WorkItemPriority[] threadPriorities = new WorkItemPriority[] { WorkItemPriority.Highest, WorkItemPriority.AboveNormal, WorkItemPriority.Normal, WorkItemPriority.BelowNormal, WorkItemPriority.Lowest };
            
            int currentThreshold = 0;

            Console.WriteLine("Applying thresholds.");
            // Apply thresholds
            for (int i = 0; i < sortedPrefixCounts.Count; i++)
            {
                if (Convert.ToDouble(i) / Convert.ToDouble(sortedPrefixCounts.Count) > thresholds[currentThreshold])
                    currentThreshold++;

                _prefixPriority[sortedPrefixCounts[i].Prefix] = threadPriorities[currentThreshold];
            }

            Console.WriteLine("Finished profiling.");
        }

        /// <summary>
        /// Get the priority for a prefix
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public WorkItemPriority GetPrefixThreadPriority(string prefix)
        {
            WorkItemPriority priority = WorkItemPriority.Lowest; // Default to lowest, as if it isn't in the dictionary from the sample it will definitely be very low
            
            _prefixPriority.TryGetValue(prefix, out priority);

            return priority;
        }
    }
    
    /// <summary>
    /// Contains a sorted prefix count
    /// </summary>
    public class PrefixCount : IComparable<PrefixCount>
    {
        public int Count;
        public string Prefix;

        public PrefixCount(string prefix, int count)
        {
            Count = count;
            Prefix = prefix;
        }

        public int CompareTo(PrefixCount other)
        {
            return other.Count.CompareTo(this.Count);
        }
    }
}
