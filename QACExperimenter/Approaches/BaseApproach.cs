using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using QACExperimenter.Data;
using QACExperimenter.Data.IndexCore;
using QACExperimenter.Evaluation;

using Amib;
using Amib.Threading;

namespace QACExperimenter.Approaches
{
    /// <summary>
    /// Base approach class to extend
    /// </summary>
    public class BaseApproach
    {
        private static DateTime? _learnQueriesBeforeDateTime;
        /// <summary>
        /// Don't sort and output autocompletions before this time (used to set a training period - for quicker execution)
        /// </summary>
        public static DateTime? LearnQueriesBeforeDateTime
        {
            get { return BaseApproach._learnQueriesBeforeDateTime; }
            set { BaseApproach._learnQueriesBeforeDateTime = value; }
        }

        private static bool _isConcurrent = true;
        /// <summary>
        /// Specify single-thread evaluation only (for debugging)
        /// </summary>
        public static bool IsConcurrent
        {
            get { return BaseApproach._isConcurrent; }
            set { BaseApproach._isConcurrent = value; }
        }

        private int _autoCompleteAfterNChars;

        public int AutoCompleteAfterNChars
        {
            get { return _autoCompleteAfterNChars; }
            set { _autoCompleteAfterNChars = value; }
        }

        private StandardEvalOutput _evalOutput;

        public StandardEvalOutput EvalOutput
        {
            get { return _evalOutput; }
            set { _evalOutput = value; }
        }

        private PrefixProfile _queryPrefixProfile;

        public PrefixProfile QueryPrefixProfile
        {
            get { return _queryPrefixProfile; }
            set { _queryPrefixProfile = value; }
        }

        private OneOffQueries _oneOffQueries;
        /// <summary>
        /// The one-off queries that can be excluded from the prefix index
        /// </summary>
        public OneOffQueries OneOffQueries
        {
            get { return _oneOffQueries; }
            set { _oneOffQueries = value; }
        }

        private SmartThreadPool _evalThreadPool;

        private int _queryCount;
        /// <summary>
        /// Count of queries submitted
        /// </summary>
        public int QueryCount
        {
            get { return _queryCount; }
        }

        public BaseApproach(int autoCompleteAfterNChars, StandardEvalOutput evalOutput, PrefixProfile queryPrefixProfile)
        {
            _autoCompleteAfterNChars = autoCompleteAfterNChars;
            _evalOutput = evalOutput;
            _queryPrefixProfile = queryPrefixProfile;

            _evalThreadPool = new SmartThreadPool(1000, 6);
            _evalThreadPool.Start(); // Setup and start the threadpool
        }

        /// <summary>
        /// Wait until all threads have finished
        /// </summary>
        public void WaitForFinish()
        {
            _evalThreadPool.WaitForIdle();
        }

        public void SubmitQuery(DateTime queryTime, string query)
        {
            _queryCount++;

            query = query.ToLower();
            
            string partialQuery = "";

            // Get characters
            if (query.Length > _autoCompleteAfterNChars)
            {
                partialQuery = query.Substring(0, _autoCompleteAfterNChars);

                // Provides a cloned large autocompletion list ready for sorting and evaluation output
                AutoCompletionList autoCompletionList = AutoCompleteQuery(queryTime, partialQuery, query);

                QueueEvaluationThread(queryTime, partialQuery, query, autoCompletionList); // Send to evaluation for ranking and output
            }
            else
            {
                QueueEvaluationThread(queryTime, partialQuery, query, new AutoCompletionList()); // Empty autocompletion list (no autocomplete options)
            }
        }

        /// <summary>
        /// Create the autocompletion list from the prefix entries ready for evaluation sorting and output
        /// </summary>
        /// <param name="allPrefixEntries"></param>
        /// <returns></returns>
        public virtual AutoCompletionList CreateAutoCompletionList(IEnumerable<BaseIndexEntry> allPrefixEntries, int defaultMinCutOff = 1)
        {
            // Min query log frequency to allow (heuristic optimisation)
            int minCutOff = defaultMinCutOff; // Absolute min cut-off - now 1, rather than 2

            if (allPrefixEntries.Count() > 500)
                minCutOff = 4;
            if (allPrefixEntries.Count() > 1000)
                minCutOff = 5;
            if (allPrefixEntries.Count() > 3000)
                minCutOff = 6;
            if (allPrefixEntries.Count() > 6000)
                minCutOff = 7;
            if (allPrefixEntries.Count() > 10000)
                minCutOff = 8;
            if (allPrefixEntries.Count() > 15000)
                minCutOff = 9;
            if (allPrefixEntries.Count() > 20000)
                minCutOff = 10;
            if (allPrefixEntries.Count() > 26000)
                minCutOff = 11;

            object listLock = new object();
            List<AutoCompletion> autoCompletions = new List<AutoCompletion>();

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = 4;

            // Make parallel
            Parallel.ForEach(allPrefixEntries, po, indexEntry =>
            {
                if (indexEntry.QueryLogFrequency > minCutOff)
                {
                    AutoCompletion ac = new AutoCompletion(indexEntry.Query);
                    ac.RankingWeight = indexEntry.RankingWeight;
                    ac.IsWikiWeighted = indexEntry.IsWikiWeighted;

                    lock (listLock)
                        autoCompletions.Add(ac);
                }
            });

            return new AutoCompletionList(autoCompletions);
        }

        /// <summary>
        /// Queue the evaluation thread
        /// </summary>
        private void QueueEvaluationThread(DateTime queryTime, string partialQuery, string query, AutoCompletionList autoCompletionList)
        {
            EvaluationOutput eo = new EvaluationOutput(queryTime, partialQuery, query, autoCompletionList, _queryCount);

            if (_isConcurrent)
                _evalThreadPool.QueueWorkItem(_evalOutput.SortAndOutput, eo);
            else
                _evalOutput.SortAndOutput(eo); // Single-threaded only
        }

        /// <summary>
        /// Overide me to handle interleaved input (e.g. event-driven query classification)
        /// </summary>
        /// <param name="changeTime"></param>
        /// <param name="articleName"></param>
        /// <param name="textChanges"></param>
        public virtual void InterleavedInput(DateTime changeTime, string inputLine)
        {
            return; // By default, do nothing
        }

        /// <summary>
        /// Needs to be overidden to do autocompletion.
        /// </summary>
        /// <param name="queryTime"></param>
        /// <param name="partialQuery"></param>
        /// <param name="fullQuery"></param>
        protected virtual AutoCompletionList AutoCompleteQuery(DateTime queryTime, string partialQuery, string fullQuery)
        {
            throw new NotImplementedException();
        }
    }
}
