using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QACExperimenter.Data;
using QACExperimenter.Approaches;
using QACExperimenter.Evaluation;
using System.IO;

namespace QACExperimenter
{
    class Program
    {
        static BaseApproach _completionApproach;

        static int _interleavedInputCount;
        static int _querySubmitCount;

        // Output progress to console
        static bool _debugOutput = true;
        
        /// <summary>
        /// Begin experiment in format: QACExperimenter.exe {collection:[aol|msn|sog]} {prefixLength} {experimentType[bl-a(baseline-all)|bl-w(baseline-window)|wiki(wiki-weighted)]} {startDate:yyyy-mm-dd} {param1:e.g.WindowNDays} ...
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Min 3 args
            if (args.Length < 4)
            {
                Console.WriteLine("Not enough parameters provided.");
                return;
            }

            // Determine the arguments
            string collection = args[0];
            int prefixChars = Convert.ToInt32(args[1]);
            string experimentType = args[2];
            DateTime startDate = new DateTime(Convert.ToInt32((args[3].Split('-'))[0]), Convert.ToInt32((args[3].Split('-'))[1]), Convert.ToInt32((args[3].Split('-'))[2]));

            FileInfo queryLog = new FileInfo(Utilities.DataDirectory + collection + "-queries.txt");
            FileInfo interleavedInput = new FileInfo(Utilities.DataDirectory + collection + "-interleavedinput.txt");
            if (!interleavedInput.Exists)
                interleavedInput = null; // Don't load if the interleaved input is null

            // Setup the data model and begin reading
            DataModel dm = new DataModel(queryLog, interleavedInput, startDate, 600000000); // I.e. all for now!
            dm.OnQuerySubmitted += dm_OnQuerySubmitted;
            dm.OnInterleavedInput += dm_OnInterleavedInput;

            // Setup the approach
            ApproachFactory af = new ApproachFactory(collection, prefixChars, experimentType, args);
            _completionApproach = af.Approach;

            // Begin reading form the data model
            dm.BeginReading();

            // Finish up the evaluation
            _completionApproach.WaitForFinish();
            af.EvalOutput.FinishOutput();

            // Output the final MRR
            Console.WriteLine("Final MRR: " + Evaluation.StandardEvalOutput.CURRENT_MRR.ToString("F4"));
        }

        static void dm_OnInterleavedInput(InterleavedInput interleavedInput)
        {
            _completionApproach.InterleavedInput(interleavedInput.InputDateTime, interleavedInput.InputLine);

            _interleavedInputCount++;

            if (_interleavedInputCount % 100000 == 0 && _debugOutput)
                Console.WriteLine("Interleaved input " + _interleavedInputCount + " (" + interleavedInput.InputLine + ")");
        }

        static void dm_OnQuerySubmitted(QuerySubmitted query)
        {
            _querySubmitCount++;

            if (_querySubmitCount % 1000 == 0 && _debugOutput)
            {
                Console.WriteLine("Query " + _querySubmitCount + " submitted: " + query.Query + " (" + query.QueryDateTime.ToString() + ")");
            }

            if (_querySubmitCount % 10000 == 0)
            {
                // Output the current MRR
                Console.WriteLine("Current MRR: " + Evaluation.StandardEvalOutput.CURRENT_MRR.ToString("F4"));
            }

            // Submit the query for auto-completion
            _completionApproach.SubmitQuery(query.QueryDateTime, query.Query);
        }
    }
}
