using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using QACExperimenter.Data;
using QACExperimenter.Data.Structures;

namespace QACExperimenter.Evaluation
{
    /// <summary>
    /// Standard evaluation outputter (outputs in format ready for EQC_Eval tool to run)
    /// </summary>
    public class StandardEvalOutput
    {
        private FileInfo _outputFile;
        private string _runIdentifier;
        private int _prefixLength;

        private StreamWriter _outputFileStream;

        private bool _isDebug;

        public delegate void EvaluationOutputEventHandler(EvaluationOutput eo);

        public event EvaluationOutputEventHandler OnEvaluationOutput;

        /// <summary>
        /// Public static MRR for output
        /// </summary>
        public static double CURRENT_MRR
        {
            get { return (_totalRR / Convert.ToDouble(_evalQueryCount)); }
        }

        public static double _totalRR;

        public static int _evalQueryCount;

        /// <summary>
        /// Default constructor. If isDebug is true, then no file output will be written, instead the OnEvaluationOutput will be raised.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="runIdentifier"></param>
        /// <param name="prefixLength"></param>
        /// <param name="isDebug"></param>
        public StandardEvalOutput(FileInfo outputFile, string runIdentifier, int prefixLength, bool isDebug = false)
        {
            _isDebug = isDebug;
            _outputFile = outputFile;
            _runIdentifier = runIdentifier;
            _prefixLength = prefixLength;

            if (!isDebug)
                _outputFileStream = new StreamWriter(_outputFile.FullName);
        }

        /// <summary>
        /// Use some heuristics to reduce the number of autocompletions that need to be sorted.
        /// Relies on the long tail and the max of the RankingWeights.
        /// </summary>
        /// <param name="autoCompletionList"></param>
        /// <returns></returns>
        public IEnumerable<AutoCompletion> GetTopAutoCompletions(AutoCompletionList autoCompletionList, int minQueryFrequency = 2)
        {
            TopNStructure<AutoCompletion> topNAutocompletions = new TopNStructure<AutoCompletion>(4); // Up to 4 completions suggested

            if (autoCompletionList.Count <= 4)
            {
                autoCompletionList.Sort();
                return autoCompletionList;
            }

            foreach (AutoCompletion ac in autoCompletionList)
            {
                if (ac.RankingWeight < minQueryFrequency)
                    continue; // Ignore;

                topNAutocompletions.Add(ac);
            }

            return topNAutocompletions;
        }

        /// <summary>
        /// First sorts the AutoCompletionList, then performs evaluation.
        /// </summary>
        /// <param name="queryTime"></param>
        /// <param name="partialQuery"></param>
        /// <param name="fullQuery"></param>
        /// <param name="autoCompletionList"></param>
        public void SortAndOutput(EvaluationOutput eo)
        {
            // Try and minimise the dataset first
            eo.AutoCompletionList = new AutoCompletionList(GetTopAutoCompletions(eo.AutoCompletionList)); // Sort the reduced list
            
            // Changed 9/4/2013 - No longer a highly inefficient sort
            //eo.AutoCompletionList.Sort();

            eo.AutoCompletionList = new AutoCompletionList(eo.AutoCompletionList.Where(x => x != null).Take(4)); // Take the 4 top autocompletions that aren't null TODO: fix null problem

            // Set the ranks on the AutoCompletionList
            eo.AutoCompletionList.SetRanks();

            // Output in the format: runId|prefixLength|queryCount|fullQuery|partialQuery|queryTime|autoCompletionCount|hitRank|isPartialMatch|reciprocalRank|partialReciprocalRank|completionList

            int hitRank = 0;

            // Find the hit rank, if any
            foreach (AutoCompletion autoCompletion in eo.AutoCompletionList)
            {
                if (autoCompletion.QueryCompletion == eo.FullQuery)
                {
                    hitRank = autoCompletion.Rank;
                    break;
                }
            }

            double reciprocalRank = 0;
            if (hitRank > 0)
                reciprocalRank = 1.0 / hitRank;

            eo.ReciprocalRank = reciprocalRank;
            eo.RunIdentifier = _runIdentifier;

            // Update the live MRR
            _evalQueryCount++;
            _totalRR += reciprocalRank;

            if (!_isDebug)
            {
                lock (_outputFileStream)
                    _outputFileStream.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}", _runIdentifier, _prefixLength, eo.QueryCount.ToString(), eo.FullQuery, eo.PartialQuery, eo.QueryTime.ToString("yyyy-MM-dd HH:mm:ss"), eo.AutoCompletionList.Count.ToString(), hitRank.ToString(), "null", reciprocalRank, reciprocalRank, String.Join(";", eo.AutoCompletionList));
            }
            else
            {
                // Debug output
                if (OnEvaluationOutput != null)
                    OnEvaluationOutput(eo);
            }
        }

        /// <summary>
        /// Call once evaluation output has finished, to close the output file stream
        /// </summary>
        public void FinishOutput()
        {
            _outputFileStream.Close();
        }
    }
}
