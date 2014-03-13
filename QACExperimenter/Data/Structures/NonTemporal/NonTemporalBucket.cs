using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QACExperimenter.Data.IndexCore;
using QACExperimenter.Approaches;

namespace QACExperimenter.Data.Structures.NonTemporal
{
    /// <summary>
    /// Contains a bucket of queries, with a FIFO-based deque to maintain at most qTotal queries in the bucket.
    /// This should handle all the queries for a single prefix. This version of the bucket does not expose temporal data points for regression-based extrapolation.
    /// </summary>
    public class NonTemporalBucket<T> where T : BaseIndexEntry
    {
        public delegate void QueryRemovedFromNTB(string query);

        /// <summary>
        /// Fired when a a query is removed from the NTB
        /// </summary>
        public event QueryRemovedFromNTB OnQueryRemovedFromNTB;

        private int _qMaxSum;
        /// <summary>
        /// Max sum of queries allowed in the bucket
        /// </summary>
        public int QMaxSum
        {
            get { return _qMaxSum; }
            set { _qMaxSum = value; }
        }

        private int _qMaxFrequency;
        /// <summary>
        /// Max frequency of a single query allowed in the bucket (must always be LESS THAN qMaxSum)
        /// </summary>
        public int QMaxFrequency
        {
            get { return _qMaxFrequency; }
            set { _qMaxFrequency = value; }
        }

        /// <summary>
        /// Manage a linked list of additions to the bucket
        /// </summary>
        private LinkedList<string> _additions;

        /// <summary>
        /// Count of all queries (including duplicates) in the bucket
        /// </summary>
        public int TotalQueriesInBucket
        {
            get { return _additions.Count; }
        }

        /// <summary>
        /// Manage the list of queries that are in
        /// </summary>
        private Dictionary<string, T> _queries;

        /// <summary>
        /// Get all the query entries in the bucket, with the count of each
        /// </summary>
        public IEnumerable<BaseIndexEntry> AllBucketQueries
        {
            get { return _queries.Values; }
        }

        /// <summary>
        /// Setting allowIndividualQueryFrequencyLookup = true allows the use of the LookupQueryFrequency() method
        /// </summary>
        /// <param name="qMaxSum"></param>
        /// <param name="qMaxFrequency"></param>
        /// <param name="allowIndividualQueryFrequencyLookup"></param>
        public NonTemporalBucket(int qMaxSum, int qMaxFrequency)
        {
            _qMaxSum = qMaxSum;
            _qMaxFrequency = qMaxFrequency;
            _queries = new Dictionary<string, T>();
            _additions = new LinkedList<string>();
        }

        /// <summary>
        /// Called by the add query event for non-overlapping NTBs
        /// </summary>
        /// <param name="query"></param>
        public void AddQueryEvent(string query)
        {
            this.AddQuery(query, null, false);
        }


        /// <summary>
        /// Add a query to the bucket, enforces all constraints
        /// </summary>
        /// <param name="query"></param>
        public void AddQuery(string query, BaseApproach approach = null, bool isOneOff = false)
        {
            // Do *NOT* ignore one-off queries as they will still need to appear in the bucket!

            // First, check adding this query won't break the qMaxFrequency
            if (_queries.ContainsKey(query) && _queries[query].QueryLogFrequency == _qMaxFrequency)
            {
                // Ignore addition, it will push it over _qMaxFrequency
                return;
            }
            
            if (_additions.Count >= _qMaxSum)
            {
                // BUCKET IS FULL, NEED TO REMOVE SOMETHING FIRST

                // First, check adding this query won't break the qMaxFrequency
                if (_queries.ContainsKey(query) && _queries[query].QueryLogFrequency == _qMaxFrequency)
                {
                    // Ignore addition, it will push it over _qMaxFrequency
                    return;
                }

                // Need to remove the last query first
                string queryToRemove = _additions.LastOrDefault();
                if (queryToRemove != null)
                {
                    _additions.RemoveLast();
                    
                    // Decrement, and remove if frequency is now 0
                    _queries[queryToRemove].QueryLogFrequency -= 1;

                    if (_queries[queryToRemove].QueryLogFrequency == 0)
                        _queries.Remove(queryToRemove); // Remove the key

                    // Fire the removed event
                    if (OnQueryRemovedFromNTB != null)
                        OnQueryRemovedFromNTB(queryToRemove);
                }
            }

            // Now add the query to the index and linked list
            if (!_queries.ContainsKey(query))
            {
                // Create the BaseIndexEntry object
                BaseIndexEntry indexEntry = (T)(Activator.CreateInstance(typeof(T)));

                indexEntry.QueryLogFrequency = 1;
                indexEntry.Query = query;
                indexEntry.Approach = approach;

                _queries[query] = (T)indexEntry; // Add new IndexEntry to query index
            }
            else
            {
                _queries[query].QueryLogFrequency += 1; // Increment
            }

            // Add to start of linked list
            _additions.AddFirst(query);
        }

        /// <summary>
        /// Get the frequency of the query in the NTB
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public double GetQueryFrequency(string query)
        {
            T indexEntry = null;
            _queries.TryGetValue(query, out indexEntry);

            if (indexEntry == null)
                return 0.0;

            return indexEntry.QueryLogFrequency;
        }
    }
}
