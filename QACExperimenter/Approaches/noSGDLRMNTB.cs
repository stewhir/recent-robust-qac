using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QACExperimenter.Evaluation;
using QACExperimenter.Data;
using QACExperimenter.Data.IndexCore;
using QACExperimenter.Data.Structures;
using QACExperimenter.Data.Structures.NonTemporal;
using QACExperimenter.Approaches.SGD;

namespace QACExperimenter.Approaches
{
    /// <summary>
    /// Linear regression over multiple non-overlapping non temporal buckets.
    /// The linear regression model is trained using stochastic gradient descent (SGD) during the run.
    /// </summary>
    public class noSGDLRMNTB : BaseApproach
    {
        private int[] _multipleNs;
        private int[] _multipleMaxSingleQueryN;

        private int _trainAfterNQueriesForPrefix;

        private int _totalQueriesAcrossAllNTBs;

        // Default base NTB size (default 200)
        private int _baseNtbSize;

        private OnlineSGDNonOverlappingLinearRegressionModel _lrModel;

        // Training data stores
        private Dictionary<string, SlidingBuffer<int>> _queryCountsForPrefix = new Dictionary<string, SlidingBuffer<int>>(); // Maintain query counts for each query observed with a prefix
        private Dictionary<string, int> _queriesObservedForPrefix = new Dictionary<string, int>();
        private HashSet<string> _prefixHasStartedTraining = new HashSet<string>();
        private Dictionary<string, TrainingPackage> _prefixTrainingPackages = new Dictionary<string, TrainingPackage>();
        private List<TrainingPackage> _trainingPackagesForModel = new List<TrainingPackage>();

        /// <summary>
        /// Keep track of the NTBs created
        /// </summary>
        private HashSet<string> _ntbsCreated;

        /// <summary>
        /// Array of NTBs for each N
        /// </summary>
        private Dictionary<string, NonTemporalBucket<BaseIndexEntry>>[] _ntbs;

        /// <summary>
        /// NTB containing the counts of queries in the last _trainAfterNQueriesForPrefix, ready for the training the predictive model
        /// </summary>
        private Dictionary<string, NonTemporalBucket<BaseIndexEntry>> _trainingNtb;

        /// <summary>
        /// NTB containing the total queries stored across all window NTBs
        /// </summary>
        private Dictionary<string, NonTemporalBucket<BaseIndexEntry>> _overallNtb;

        /// <summary>
        /// Base NTB for QAC when the non overlapping windows haven't been filled
        /// </summary>
        private Dictionary<string, NonTemporalBucket<BaseIndexEntry>> _baseNtb;

        /// <summary>
        /// MultipleNs must be specified in ascending order for the NTBs
        /// </summary>
        /// <param name="multipleNs"></param>
        /// <param name="multipleMaxSingleQueryN"></param>
        /// <param name="trainAfterNQueriesForPrefix">How many queries to observe between training the ML model (i.e., predict queries in this window of N - OR: 'the prediction horizon'). Note the model won't start training until after (largest NTB size) + (trainAfter parameter) has been reached</param>
        /// <param name="autoCompleteAfterNChars"></param>
        /// <param name="evalOutput"></param>
        /// <param name="queryPrefixProfile"></param>
        public noSGDLRMNTB(int[] multipleNs, int[] multipleMaxSingleQueryN, int trainAfterNQueriesForPrefix, string queryLogFile, int autoCompleteAfterNChars, StandardEvalOutput evalOutput, PrefixProfile queryPrefixProfile, int baseNtbSize = 200)
            : base(autoCompleteAfterNChars, evalOutput, queryPrefixProfile)
        {
            if (multipleNs.Length != multipleMaxSingleQueryN.Length)
                throw new Exception("Must be the same length");

            _trainAfterNQueriesForPrefix = trainAfterNQueriesForPrefix;
            _multipleNs = multipleNs;
            _multipleMaxSingleQueryN = multipleMaxSingleQueryN;
            _ntbs = new Dictionary<string, NonTemporalBucket<BaseIndexEntry>>[multipleNs.Length];
            _trainingNtb = new Dictionary<string, NonTemporalBucket<BaseIndexEntry>>();

            _lrModel = new OnlineSGDNonOverlappingLinearRegressionModel(multipleNs.Length, _multipleNs[0], _trainAfterNQueriesForPrefix);

            for (int i = 0; i < multipleNs.Length; i++)
            {
                _ntbs[i] = new Dictionary<string, NonTemporalBucket<BaseIndexEntry>>();
            }

            _ntbsCreated = new HashSet<string>();

            // Calculate the total number of queries stored across all NTBs (max overall NTB capacity)
            _totalQueriesAcrossAllNTBs = _multipleNs.Sum();

            _overallNtb = new Dictionary<string, NonTemporalBucket<BaseIndexEntry>>();

            _baseNtb = new Dictionary<string, NonTemporalBucket<BaseIndexEntry>>();

            _baseNtbSize = baseNtbSize;
        }

        /// <summary>
        /// Make prediction using linear regression
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns> 
        private double MakePrediction(BaseIndexEntry indexEntry, string partialQuery)
        {
            FeaturePackage featurePackage = BuildFeaturePackage(indexEntry, partialQuery);

            double prediction = _lrModel.MakePrediction(featurePackage.NtbFeatures);

            return prediction;
        }

        private int _currentQueryCount = 0;
        private int _trainCount = 0;
        private bool _firstTrainingHasHappened = false;

        /// <summary>
        /// Autocomplete a query after n characters
        /// </summary>
        /// <param name="queryTime"></param>
        /// <param name="partialQuery"></param>
        /// <param name="fullQuery"></param>
        /// <returns></returns>
        protected override AutoCompletionList AutoCompleteQuery(DateTime queryTime, string partialQuery, string fullQuery)
        {
            // Increment the current query count
            _currentQueryCount++;

            // Determine whether the multiple NTBs for a prefix need to be created first
            #region Setup NTBs
            // Setup the main NTBs
            if (!_ntbsCreated.Contains(partialQuery))
            {
                // Create NTBs
                for (int i = 0; i < _multipleNs.Length; i++)
                {
                    // Retrieve the existing bucket for the prefix
                    _ntbs[i][partialQuery] = new NonTemporalBucket<BaseIndexEntry>(_multipleNs[i], _multipleMaxSingleQueryN[i]);

                    // Hook up the events
                    if (i > 0)
                    {
                        _ntbs[i - 1][partialQuery].OnQueryRemovedFromNTB += _ntbs[i][partialQuery].AddQueryEvent; // Send to next NTB
                    }
                }

                // Create overall NTB
                _overallNtb[partialQuery] = new NonTemporalBucket<BaseIndexEntry>(_totalQueriesAcrossAllNTBs, _totalQueriesAcrossAllNTBs);

                // Create base NTB
                _baseNtb[partialQuery] = new NonTemporalBucket<BaseIndexEntry>(_baseNtbSize, _baseNtbSize);

                // Track the NTBs have been created for the prefix
                _ntbsCreated.Add(partialQuery);
            }

            // Setup the training NTB
            if (!_trainingNtb.ContainsKey(partialQuery))
            {
                _trainingNtb[partialQuery] = new NonTemporalBucket<BaseIndexEntry>(_trainAfterNQueriesForPrefix, _trainAfterNQueriesForPrefix); // Create NTB sized for the training horizon (in queries)
            }
            #endregion

            // Deal with the online training
            #region Handle training package building and use
            if (!_queriesObservedForPrefix.ContainsKey(partialQuery))
                _queriesObservedForPrefix[partialQuery] = 0;

            // Setup a training package for the ML if necessary, and do any training that is outstanding
            if ((_prefixHasStartedTraining.Contains(partialQuery) && _queriesObservedForPrefix[partialQuery] == _trainAfterNQueriesForPrefix)
                    || (!_prefixHasStartedTraining.Contains(partialQuery) && _queriesObservedForPrefix[partialQuery] == _totalQueriesAcrossAllNTBs)) // - start training when biggest NTB is full
            {
                // Apply existing training
                if (_prefixTrainingPackages.ContainsKey(partialQuery))
                {
                    // Train
                    TrainingPackage trainingPackage = _prefixTrainingPackages[partialQuery];
                    trainingPackage.QueryCountAtTrain = _currentQueryCount;

                    // Update the target likelihood variable for each query from the last _trainAfterNQueriesForPrefix queries
                    foreach (FeaturePackage queryFeaturePackage in trainingPackage.TrainingPackageQueries)
                    {
                        double queryCount = _trainingNtb[partialQuery].GetQueryFrequency(queryFeaturePackage.Query);
                        if (queryCount > 0)
                            queryFeaturePackage.TargetLikelihood = queryCount; // / Convert.ToDouble(_trainAfterNQueriesForPrefix);
                    }


                    // Use the training package for the model
                    UseTrainingPackage(trainingPackage);
                    _firstTrainingHasHappened = true;
                    //if (_trainCount % 200 == 0)
                    //Console.WriteLine("Training package " + _trainCount.ToString() + " used for prefix " + trainingPackage.ForPrefix + " (first? " + trainingPackage.IsFirstTrainingPackageForPrefix + ")");

                    _trainCount++;
                }


                // Build package containing exist queries to train with
                //Console.WriteLine("Build package for " + partialQuery);
                bool isFirstTrainingPackage = !_prefixHasStartedTraining.Contains(partialQuery);
                _prefixTrainingPackages[partialQuery] = BuildTrainingPackage(partialQuery, isFirstTrainingPackage);

                _prefixHasStartedTraining.Add(partialQuery); // Mark the prefix as started training

                // Reset the queries observed for the prefix, ready for next training package to run
                _queriesObservedForPrefix[partialQuery] = 0;
            }
            #endregion

            NonTemporalBucket<BaseIndexEntry> overallNTB = _overallNtb[partialQuery];

            // Get the NTB entries
            IEnumerable<BaseIndexEntry> biggestNTBPrefixEntries = null;

            // Try largest NTB first, if it doesn't have the prefix, then no others will either
            biggestNTBPrefixEntries = overallNTB.AllBucketQueries;

            // The prefix entries for output
            List<BaseIndexEntry> outputPrefixEntries = new List<BaseIndexEntry>();
            
            // Compute scores on the auto-completions
            foreach (BaseIndexEntry prefixEntry in biggestNTBPrefixEntries)
            {
                if (prefixEntry.QueryLogFrequency < 2)
                    continue; // Ignore low frequency completions - increases speed and removes junk

                BaseIndexEntry outputIndexEntry = new BaseIndexEntry();
                outputIndexEntry.Query = prefixEntry.Query;

                // Use predicted likelihood if overall NTB is full, otherwise just use frequency in the largest NTB (it probably doesn't make sense to predict using incomplete NTBs)
                if (overallNTB.TotalQueriesInBucket == overallNTB.QMaxSum)
                {
                    // Use prediction to rank suggestion
                    outputIndexEntry.QueryLogFrequency = Math.Round(2.0 + MakePrediction(prefixEntry, partialQuery), 5);
                }
                else
                {
                    // Use overall NTB count by default
                    outputIndexEntry.QueryLogFrequency = _baseNtb[partialQuery].GetQueryFrequency(outputIndexEntry.Query); // TODO: change this prefixEntry.QueryLogFrequency;
                    if (outputIndexEntry.QueryLogFrequency < 2)
                        continue;
                }

                outputPrefixEntries.Add(outputIndexEntry);
            }
            
            // Create and rank the autocompletions
            AutoCompletionList autoCompletionListOutput = CreateAutoCompletionList(outputPrefixEntries);

            // Increment the queries observed with the prefix (for knowing when to train)
            if (_queriesObservedForPrefix.ContainsKey(partialQuery))
                _queriesObservedForPrefix[partialQuery] += 1;
            else
                _queriesObservedForPrefix[partialQuery] = 1;

            // Add the new query to the NTBs
            _ntbs[0][partialQuery].AddQuery(fullQuery, null);
            _overallNtb[partialQuery].AddQuery(fullQuery, null);
            _baseNtb[partialQuery].AddQuery(fullQuery, null);

            // Add the query to the training ntb (used for computing prediction likelihood)
            _trainingNtb[partialQuery].AddQuery(fullQuery, null);

            // Return the autocompletion list ready to be sent off for evaluation
            return autoCompletionListOutput;
        }

        /// <summary>
        /// Apply the training package to the model
        /// </summary>
        /// <param name="trainingPackage"></param>
        private void UseTrainingPackage(TrainingPackage trainingPackage)
        {
            foreach (FeaturePackage fp in trainingPackage.TrainingPackageQueries)
            {
                _lrModel.TrainModel(fp); // Pass to model for training
            }
        }

        /// <summary>
        /// Build the training package for a query prefix
        /// </summary>
        /// <param name="partialQuery"></param>
        /// <returns></returns>
        private TrainingPackage BuildTrainingPackage(string partialQuery, bool isFirstTrainingPackageForPrefix)
        {
            TrainingPackage trainingPackage = new TrainingPackage();
            trainingPackage.QueryCountAtCreation = _currentQueryCount; // Set the query count at package creation
            trainingPackage.IsFirstTrainingPackageForPrefix = isFirstTrainingPackageForPrefix;
            trainingPackage.ForPrefix = partialQuery;

            trainingPackage.TrainingPackageQueries = new List<FeaturePackage>();

            Dictionary<string, FeaturePackage> trainingPackageQueriesLookup = new Dictionary<string, FeaturePackage>(); // Index lookup by query

            NonTemporalBucket<BaseIndexEntry> biggestNTB = _ntbs[_ntbs.Length - 1][partialQuery];

            // Efficiently build the features for each query
            foreach (BaseIndexEntry prefixEntry in biggestNTB.AllBucketQueries)
            {
                FeaturePackage tpq = BuildFeaturePackage(prefixEntry, partialQuery);

                trainingPackage.TrainingPackageQueries.Add(tpq);
            }

            return trainingPackage;
        }

        /// <summary>
        /// Build the feature package including NTB likelihoods for the specified query. Doesn't include the target likelihood.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="partialQUery"></param>
        /// <returns></returns>
        private FeaturePackage BuildFeaturePackage(BaseIndexEntry prefixEntry, string partialQuery, bool includeQueryCountsFeature = false)
        {
            FeaturePackage tpq = new FeaturePackage();
            tpq.Query = prefixEntry.Query;
            tpq.NtbFeatures = new double[_ntbs.Length];

            // Set the NTB counts
            for (int i = 0; i < _ntbs.Length; i++)
            {
                double queryFrequency = _ntbs[i][partialQuery].GetQueryFrequency(prefixEntry.Query);

                // Calculate probability and apply
                tpq.NtbFeatures[i] = queryFrequency; // TODO: don't normalise for now / Convert.ToDouble(ntbN);

                // Include query counts feature if necessary
                if (includeQueryCountsFeature)
                {
                    // IGNORE THIS FOR NOW
                    //tpq.QueriesSinceLastTrain = _currentQueryCount - _queryCountsForPrefix[partialQuery].OldestItem();
                }
            }

            return tpq;
        }

        private double minProbability = 0;

        public override AutoCompletionList CreateAutoCompletionList(IEnumerable<BaseIndexEntry> allPrefixEntries, int defaultMinCutOff = 1)
        {
            // Min query log frequency to allow (heuristic optimisation)
            double minCutOff = minProbability; // Absolute min cut-off - now 1, rather than 2

            /*if (allPrefixEntries.Count() > 500)
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
            */

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
                    ac.RankingWeight = 2 + indexEntry.RankingWeight; // Will either be the NTB count or predicted likelihood

                    lock (listLock)
                        autoCompletions.Add(ac);
                }
            });

            return new AutoCompletionList(autoCompletions);
        }
    }
}
