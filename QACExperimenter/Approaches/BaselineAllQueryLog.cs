using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QACExperimenter.Evaluation;
using QACExperimenter.Data;
using QACExperimenter.Data.IndexCore;

namespace QACExperimenter.Approaches
{
    /// <summary>
    /// Uses evidence from all previously seen queries
    /// </summary>
    public class BaselineAllQueryLog<T> : BaseApproach where T : BaseIndexEntry 
    {
        private PrefixIndex<T> _index;
        /// <summary>
        /// Prefix index (generic contained type needs to be cast correctly)
        /// </summary>
        public PrefixIndex<T> BasePrefixIndex
        {
            get { return _index; }
        }

        public BaselineAllQueryLog(int autoCompleteAfterNChars, StandardEvalOutput evalOutput, PrefixProfile queryPrefixProfile) :
            base(autoCompleteAfterNChars, evalOutput, queryPrefixProfile)
        {
            // Setup prefix index
            _index = new PrefixIndex<T>(autoCompleteAfterNChars, queryPrefixProfile);
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
            IEnumerable<BaseIndexEntry> allPrefixEntries = _index.GetPrefixIndexEntries(partialQuery);

            // Deal with no autocompletions
            if (allPrefixEntries == null)
            {
                _index.AddQuery(fullQuery, this, this.OneOffQueries.IsOneOffQuery(fullQuery));
                return new AutoCompletionList();
            }

            AutoCompletionList autoCompletionListOutput = null;

            if (BaseApproach.LearnQueriesBeforeDateTime.HasValue && queryTime < BaseApproach.LearnQueriesBeforeDateTime)
            {
                // Output empty list, as this is the learning period and doesn't matter
                autoCompletionListOutput = new AutoCompletionList();
            }
            else
            {
                // Create and rank the autocompletions
                autoCompletionListOutput = CreateAutoCompletionList(allPrefixEntries);
            }

            // Add the new query to the index
            _index.AddQuery(fullQuery, this);

            // Return the autocompletion list read to be sent off for evaluation
            return autoCompletionListOutput;
        }
    }
}
