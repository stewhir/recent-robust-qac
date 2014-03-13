using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using QACExperimenter.Approaches;

namespace QACExperimenter.Data.IndexCore
{
    /// <summary>
    /// Creates a high-performance in-memory prefix index. Generic type T must extend BaseIndexEntry.
    /// This index provides the core of the query autocompletion.
    /// </summary>
    public class PrefixIndex<T> where T : BaseIndexEntry
    {
        /// <summary>
        /// Index entries - by prefix -> query -> (IndexEntry)
        /// </summary>
        private Dictionary<string, Dictionary<string, T>> _entries;

        private int _totalIndexEntries;
        /// <summary>
        /// Maintain a count of all index entries (over all prefixes)
        /// </summary>
        public int TotalIndexEntries
        {
            get { return _totalIndexEntries; }
        }

        /// <summary>
        /// Index by query
        /// </summary>
        private Dictionary<string, T> _queryIndex;

        private int _prefixLength;

        private PrefixProfile _prefixProfile;

        public PrefixIndex(int prefixLength, PrefixProfile prefixProfile)
        {
            _entries = new Dictionary<string, Dictionary<string, T>>();
            _queryIndex = new Dictionary<string, T>();
            _prefixLength = prefixLength;
            _prefixProfile = prefixProfile;
        }

        /// <summary>
        /// Add the query to the index (and then automatically sorts the cache)
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="query"></param>
        public BaseIndexEntry AddQuery(string query, BaseApproach approach = null, bool isOneOff = false)
        {
            // For one-off index entries - don't actually add to the prefix index - but still create the object for other indexes
            if (isOneOff)
            {
                T indexEntryOneOff = (T)(Activator.CreateInstance(typeof(T)));

                indexEntryOneOff.QueryLogFrequency = 1;
                indexEntryOneOff.Query = query;
                indexEntryOneOff.Approach = approach;
                return indexEntryOneOff;
            }

            // Find any existing entry (as this is not a one-off query)
            T indexEntry = null;
            _queryIndex.TryGetValue(query, out indexEntry);

            if (indexEntry == null)
            {
                indexEntry = (T)(Activator.CreateInstance(typeof(T)));

                indexEntry.QueryLogFrequency = 1;
                indexEntry.Query = query;
                indexEntry.Approach = approach;
                _queryIndex.Add(query, indexEntry); // Add new IndexEntry to query index
            }

            string prefix = Utilities.GetPrefix(query, _prefixLength);
            if (prefix == null)
                return null; // No prefix available, ignore and return null

            // Check prefix exists
            if (!_entries.ContainsKey(prefix))
            {
                _entries[prefix] = new Dictionary<string, T>();
            }

            // Add or update the query
            if (!_entries[prefix].ContainsKey(query))
            {
                _entries[prefix][query] = indexEntry;
            }
            else
            {
                // Increment the query count
                _entries[prefix][query].QueryLogFrequency += 1;
            }

            _totalIndexEntries++;

            return indexEntry;
        }

        /// <summary>
        /// Get the PrefixIndex entry for a query (used to updates weights etc). Returns null if the query is not long enough to extract a prefix from.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public BaseIndexEntry GetPrefixIndexEntry(string query)
        {
            string prefix = Utilities.GetPrefix(query, _prefixLength);

            if (prefix == null)
                return null;

            return _entries[prefix][query];
        }

        /// <summary>
        /// Returns all the entries in the index for the given prefix, excludes the prefixes that are in the OneOffQuery hashset
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IEnumerable<BaseIndexEntry> GetPrefixIndexEntries(string prefix)
        {
            if (!_entries.ContainsKey(prefix))
                return null;

            return _entries[prefix].Values;
        }

        /// <summary>
        /// Remove a query from the prefix index. With countToRemove > 0 then this will decrement query count for the query.
        /// If decrementing the queryCount means it will be less than 1, then the query will be removed altogether from prefixIndex.
        /// Remove all allows the removal of all of the query from the prefixIndex (ignores the countToRemove).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="countToRemove"></param>
        public void DeleteQuery(string query, int countToRemove = 1, bool removeAll = false)
        {
            string prefix = Utilities.GetPrefix(query, _prefixLength);
            if (prefix == null)
                return; // Nothing to remove if there is no prefix

            if (removeAll)
            {
                // Remove fully from indexes and cache
                DeleteQueryFromIndex(query, prefix);
            }
            else
            {
                BaseIndexEntry indexEntry = GetPrefixIndexEntry(query);
                indexEntry.QueryLogFrequency -= countToRemove;

                if (indexEntry.QueryLogFrequency <= 0)
                {
                    // Remove entirely as there is no evidence for the query
                    DeleteQueryFromIndex(query, prefix); // IGNORE THIS FOR NOW
                }
            }
        }

        /// <summary>
        /// Called by DeleteQuery to full remove a query from the index/caches.
        /// </summary>
        private void DeleteQueryFromIndex(string query, string prefix)
        {
            // Remove from prefixIndex
            _entries[prefix].Remove(query);

            // Remove from query index
            _queryIndex.Remove(query);
        }
    }
}
