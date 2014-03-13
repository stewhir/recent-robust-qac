using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QACExperimenter.Evaluation;
using QACExperimenter.Data;
using QACExperimenter.Data.IndexCore;
using QACExperimenter.Data.Structures.NonTemporal;

namespace QACExperimenter.Approaches
{
    /// <summary>
    /// Non temporal bucket approach using a FIFO bucket to track query probability
    /// </summary>
    public class NonTemporalBucketApproach : BaseApproach
    {
        private int _qMaxFrequency;

        private int _qMaxSum;

        private Dictionary<string, NonTemporalBucket<BaseIndexEntry>> _indexEntries;

        public NonTemporalBucketApproach(int qMaxFrequency, int qMaxSum, int autoCompleteAfterNChars, StandardEvalOutput evalOutput, PrefixProfile queryPrefixProfile)
            : base(autoCompleteAfterNChars, evalOutput, queryPrefixProfile)
        {
            _qMaxFrequency = qMaxFrequency;
            _qMaxSum = qMaxSum;
            _indexEntries = new Dictionary<string, NonTemporalBucket<BaseIndexEntry>>();
        }

        /// <summary>
        /// Autocomplete a query after n characters
        /// </summary>
        /// <param name="queryTime"></param>
        /// <param name="partialQuery"></param>
        /// <param name="fullQuery"></param>
        /// <returns></returns>
        protected override AutoCompletionList AutoCompleteQuery(DateTime queryTime, string partialQuery, string fullQuery)
        {
            // Get the sorted entries
            IEnumerable<BaseIndexEntry> allPrefixEntries = null;

            if (_indexEntries.ContainsKey(partialQuery))
            {
                // Retrieve the existing bucket for the prefix
                allPrefixEntries = _indexEntries[partialQuery].AllBucketQueries; // Retrieve from the bucket
            }
            else
            {
                // Create the bucket for the prefix ready
                NonTemporalBucket<BaseIndexEntry> ntbForPrefix = new NonTemporalBucket<BaseIndexEntry>(_qMaxSum, _qMaxFrequency);

                _indexEntries[partialQuery] = ntbForPrefix;

                allPrefixEntries = ntbForPrefix.AllBucketQueries;
            }

            // Deal with no autocompletions
            if (allPrefixEntries.Count() == 0)
            {
                _indexEntries[partialQuery].AddQuery(fullQuery, this); // Add the query to the bucket
                return new AutoCompletionList(); // Return no autocompletions
            }

            // Create and rank the autocompletions
            AutoCompletionList autoCompletionListOutput = autoCompletionListOutput = CreateAutoCompletionList(allPrefixEntries);

            // Add the new query to the index
            _indexEntries[partialQuery].AddQuery(fullQuery, this);

            // Return the autocompletion list ready to be sent off for evaluation
            return autoCompletionListOutput;
        }
    }
}
