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
    /// Uses evidence from all previously seen queries that are within a specified window (of days)
    /// </summary>
    public class BaselineWindowQueryLog<T> : BaselineAllQueryLog<T> where T : BaseIndexEntry 
    {
        private int _windowDays;

        /// <summary>
        /// History of queries being added
        /// </summary>
        private Journal _queryHistoryJournal;

        public BaselineWindowQueryLog(int windowDays, int autoCompleteAfterNChars, StandardEvalOutput evalOutput, PrefixProfile queryPrefixProfile) :
            base(autoCompleteAfterNChars, evalOutput, queryPrefixProfile)
        {
            _windowDays = windowDays;

            _queryHistoryJournal = new Journal();
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
            // Remove any historic queries prior to the window
            DateTime windowStart = queryTime.AddDays(-1 * _windowDays);
            IEnumerable<JournalEntry> entriesToRemove = _queryHistoryJournal.GetEntriesBeforeDateAndDelete(windowStart);
            foreach (JournalEntry journalEntry in entriesToRemove)
            {
                // Only delete query if it's not in the one-off queries list
                if (!this.OneOffQueries.IsOneOffQuery(journalEntry.Query))
                    BasePrefixIndex.DeleteQuery(journalEntry.Query, 1, false);
                
            }

            // Call base class to reuse logic
            AutoCompletionList toReturn = base.AutoCompleteQuery(queryTime, partialQuery, fullQuery);
            
            // Add the journal entry for this query
            JournalEntry queryJournalEntry = new JournalEntry();
            queryJournalEntry.EntryDateTime = queryTime;
            queryJournalEntry.EntryType = JournalEntry.JournalEntryType.QueryAdded;
            queryJournalEntry.Query = fullQuery;
            _queryHistoryJournal.AddEntry(queryJournalEntry);

            return toReturn;
        }
    }
}
