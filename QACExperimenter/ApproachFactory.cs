using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using QACExperimenter.Approaches;
using QACExperimenter.Data;
using QACExperimenter.Data.IndexCore;
using QACExperimenter.Evaluation;

namespace QACExperimenter
{
    /// <summary>
    /// Creates the approach and all arguments. Arguments are provided in the format:
    /// {collection:[aol|msn|sog]} {prefixLength} {experimentType[bl-a(baseline-all)|bl-w(baseline-window)|ntb(non-temporal-bucket-maxQsum-maxQfrequency)|qlextrapolation(qlextrapolation)|mlreg]} {startDate:yyyy-mm-dd} {param1:e.g.WindowNDays, or comma-separated sliding window model for qlextrapolation}
    /// The time window extrapolation model needs to be set in the code already for qlextrapolation
    /// </summary>
    public class ApproachFactory
    {
        private string _runId;
        /// <summary>
        /// Run ID
        /// </summary>
        public string RunId
        {
            get { return _runId; }
        }

        private BaseApproach _approach;
        /// <summary>
        /// The approach made by the factory
        /// </summary>
        public BaseApproach Approach
        {
            get { return _approach; }
        }

        private StandardEvalOutput _evalOutput;
        /// <summary>
        /// The eval output being used
        /// </summary>
        public StandardEvalOutput EvalOutput
        {
            get { return _evalOutput; }
        }

        public ApproachFactory(string collection, int prefixLength, string expType, string[] allArgs, bool isDebug = false)
        {
            // Determine which approach to create
            _runId = collection + '-' + expType;

            if (expType == "bl-w")
                _runId += allArgs[4]; // Append the number of days the window is over

            if (expType == "ntb")
                _runId += allArgs[4] + "-" + allArgs[5]; // Append the non temporal bucket parameters

            if (expType == "sgdlrnomntb") // Multiple non-overlapping NTBs with stochastic gradient descent linear regression, args are comma separated
                _runId += allArgs[4] + "-" + allArgs[5] + "-t" + allArgs[6]; // Append the non temporal bucket parameters along with train between queries last parameter (format: aol 2 mntb 2006-03-01 500,1000 500,1000 100)

            // Ensure the run output file doesn't already exist
            if (!isDebug && File.Exists(Utilities.DataDirectory + prefixLength + "chars-" + _runId + ".txt"))
            {
                Console.WriteLine("Output file already exists, exiting.");
                Environment.Exit(0); // Exit now
            }

            // Setup the approach
            if (!isDebug)
                _evalOutput = new StandardEvalOutput(new FileInfo(Utilities.DataDirectory + prefixLength + "chars-" + _runId + ".txt"), _runId, prefixLength); // new StandardEvalOutput(new FileInfo("aol_wiki_all_history_" + prefixChars + "_baseline_charprefix.txt"), runName, prefixChars);
            else
                _evalOutput = new StandardEvalOutput(null, _runId, prefixLength, true); // Debug for event-based output rather than file output

            if (expType == "bl-a")
                _approach = new BaselineAllQueryLog<BaseIndexEntry>(prefixLength, _evalOutput, null);
            else if (expType == "bl-w")
                _approach = new BaselineWindowQueryLog<BaseIndexEntry>(Convert.ToInt32(allArgs[4]), prefixLength, _evalOutput, null);
            else if (expType == "ntb")
            {
                _approach = new NonTemporalBucketApproach(Convert.ToInt32(allArgs[4]), Convert.ToInt32(allArgs[5]), prefixLength, _evalOutput, null);
            }
            else if (expType == "sgdlrnomntb")
            {
                _approach = new noSGDLRMNTB(
                    allArgs[4].Split(',').Select(s => int.Parse(s)).ToArray(),
                    allArgs[5].Split(',').Select(s => int.Parse(s)).ToArray(),
                    Convert.ToInt32(allArgs[6]),
                    Utilities.DataDirectory + collection + "-queries.txt",
                    prefixLength,
                    _evalOutput,
                    null,
                    Convert.ToInt32(allArgs[7]));
            }
            else
            {
                Console.WriteLine("Invalid experiment type, must be bl-a, bl-w, ntb or sgdlrnomntb.");
                Environment.Exit(0);
            }

            // Load the one-off queries for optimisation in some cases
            OneOffQueries ofq = new OneOffQueries(new FileInfo(Utilities.DataDirectory + collection + "-oneoffqueries.txt"));
            _approach.OneOffQueries = ofq;
        }
    }
}
