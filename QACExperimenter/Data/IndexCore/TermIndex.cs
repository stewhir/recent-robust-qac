using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data.IndexCore
{
    /// <summary>
    /// Maintains an index of BaseIndexEntry objects, indexed by term.
    /// This is effectively an inverted index to the queries in the PrefixIndex.
    /// This class is NOT thread-safe.
    /// </summary>
    public class TermIndex
    {
        private Dictionary<string, HashSet<BaseIndexEntry>> _entries;

        public TermIndex()
        {
            _entries = new Dictionary<string, HashSet<BaseIndexEntry>>();
        }

        /// <summary>
        /// Index the BaseIndexEntry in the inverted index, by query term
        /// </summary>
        /// <param name="indexEntry"></param>
        public void AddBaseIndexEntry(BaseIndexEntry indexEntry)
        {
            // Extract the terms from the BaseIndexEntry query, and place in index for each term
            foreach (string term in indexEntry.QueryTerms)
            {
                // Create non-existent posting list
                if (!_entries.ContainsKey(term))
                    _entries[term] = new HashSet<BaseIndexEntry>();

                if (!_entries[term].Contains(indexEntry))
                    _entries[term].Add(indexEntry);
            }
        }

        /// <summary>
        /// Get the frequency of the term in the index. This is how many unique queries contain the term.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public double GetTermFrequencyInIndex(string term)
        {
            HashSet<BaseIndexEntry> baseIndexEntries = null;

            _entries.TryGetValue(term, out baseIndexEntries);

            if (baseIndexEntries == null)
                return 0.0;

            return Convert.ToDouble(baseIndexEntries.Count);
        }

        /// <summary>
        /// Get the BaseIndexEntries containing the provided query term.
        /// Returns null if there are no index entries.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public List<BaseIndexEntry> GetIndexEntriesForQueryTerm(string term)
        {
            if (!_entries.ContainsKey(term))
                return null;

            return _entries[term].ToList();
        }
    }
}
