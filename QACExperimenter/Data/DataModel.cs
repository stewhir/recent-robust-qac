using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace QACExperimenter.Data
{
    /// <summary>
    /// Provides events for interacting with the query log and Wikipedia event data.
    /// Synchronises the chronological ordering of queries and Wiki changes over time.
    /// The query log and wiki change file must be in order for this to work correctly.
    /// 
    /// In addition, the DataModel profiles the query log (from a sample of 4 million queries) to determine the distributions of prefixes so that
    /// sorting during experimentation can be optimised with highly occurring prefixes being sorted first.
    /// </summary>
    public class DataModel
    {
        private FileInfo _queryLog;
        /// <summary>
        /// Query log file
        /// </summary>
        public FileInfo QueryLog
        {
            get { return _queryLog; }
        }

        private FileInfo _interleavedInputFile;
        /// <summary>
        /// Wiki changes file
        /// </summary>
        public FileInfo InterleavedInputFile
        {
            get { return _interleavedInputFile; }
        }

        private DateTime _startFrom;
        /// <summary>
        /// The date to start raising query and wikipedia events
        /// </summary>
        public DateTime StartFrom
        {
            get { return _startFrom; }
        }

        public delegate void QuerySubmittedEventHandler(QuerySubmitted query);
        /// <summary>
        /// Fired when a query is submitted
        /// </summary>
        public event QuerySubmittedEventHandler OnQuerySubmitted;

        public delegate void InterleavedInputEventHandler(InterleavedInput interleavedInput);
        /// <summary>
        /// Fired when a Wiki change occurs
        /// </summary>
        public event InterleavedInputEventHandler OnInterleavedInput;

        public delegate void EndOfDataEventHandler();
        /// <summary>
        /// End of data reading
        /// </summary>
        public event EndOfDataEventHandler OnEndOfData;

        /// <summary>
        /// For debugging purposes
        /// </summary>
        private int _queryCount;
        private int _maxQueryCount;

        private bool _endDataModelStream;
        /// <summary>
        /// End the data stream
        /// </summary>
        public bool EndDataModelStream
        {
            get { return _endDataModelStream; }
            set { _endDataModelStream = value; }
        }

        public DataModel(FileInfo queryLog, FileInfo interleavedInput, DateTime startFrom, int maxQueryCount = 30000000)
        {
            _queryLog = queryLog;
            _interleavedInputFile = interleavedInput;
            _startFrom = startFrom;
            _maxQueryCount = maxQueryCount;
        }

        /// <summary>
        /// Begin reading the files
        /// </summary>
        public void BeginReading()
        {
            StreamReader queryLogFile = new StreamReader(_queryLog.FullName, UTF8Encoding.UTF8);
            StreamReader interleavedInputFile = null;
            if (_interleavedInputFile != null)
                interleavedInputFile = new StreamReader(_interleavedInputFile.FullName);

            // Read lines to start date
            string queryLine = queryLogFile.ReadLine();
            while (queryLine != null)
            {
                if (_endDataModelStream)
                    return; // End the stream prematurely

                // In the format query{TAB}timestamp
                string[] rows = queryLine.Split('\t');
                DateTime queryDateTime = DateTime.ParseExact(rows[1], "yyyy-MM-dd HH:mm:ss", null);

                if (queryDateTime >= _startFrom)
                    break;

                queryLine = queryLogFile.ReadLine();
            }

            // Only read interleaved input if not null
            InterleavedInput interleavedInput = null;
            string interleavedInputLine = null;
            if (_interleavedInputFile != null)
            {
                interleavedInputLine = interleavedInputFile.ReadLine();
                while (interleavedInputLine != null)
                {
                    if (_endDataModelStream)
                        return; // End the stream prematurely

                    // In the format: timestamp{tab}value...
                    string[] rows = interleavedInputLine.Split('\t');
                    DateTime inputDateTime = DateTime.ParseExact(rows[0], "yyyy-MM-dd HH:mm:ss", null);

                    if (inputDateTime >= _startFrom)
                    {
                        interleavedInput = new InterleavedInput(inputDateTime, interleavedInputLine);
                        break;
                    }

                    interleavedInputLine = interleavedInputFile.ReadLine();
                }
            }

            // The current lines
            QuerySubmitted currentQuerySubmitted = new QuerySubmitted(queryLine);
            string currentInterleavedInputLine = interleavedInputLine;

            // Now, begin reading the files and firing events
            while (true)
            {
                if (_endDataModelStream)
                    return; // End the stream prematurely

                // Output query submitted
                if ((currentQuerySubmitted != null && interleavedInput != null
                    && (currentQuerySubmitted.QueryDateTime <= interleavedInput.InputDateTime) || interleavedInput == null)) // Output query if the query is before the changes, or there are no more wiki changes
                {
                    OnQuerySubmitted(currentQuerySubmitted);

                    _queryCount++;
                    if (_queryCount > _maxQueryCount)
                        break; // Only allow up to the max query count

                    // Read next line
                    queryLine = queryLogFile.ReadLine();
                    if (queryLine != null && queryLine.Length > 1 && queryLine != "Query\tQueryTime")
                        currentQuerySubmitted = new QuerySubmitted(queryLine);
                }

                // Output interleaved input
                if ((currentQuerySubmitted != null && interleavedInput != null
                    && (interleavedInput.InputDateTime <= currentQuerySubmitted.QueryDateTime) || currentQuerySubmitted == null)) // Output wiki change if the change is before the query, or there are no more queries submitted
                {
                    OnInterleavedInput(interleavedInput);

                    // Read next line
                    interleavedInputLine = interleavedInputFile.ReadLine();
                    if (interleavedInputLine != null && queryLine.Length > 1)
                    {
                        string[] rows = interleavedInputLine.Split('\t');
                        DateTime inputDateTime = DateTime.ParseExact(rows[0], "yyyy-MM-dd HH:mm:ss", null);

                        interleavedInput = new InterleavedInput(inputDateTime, interleavedInputLine);
                    }
                }

                if (queryLine == null && interleavedInputLine == null)
                    break; // End the loop
            }

            // Fire the final event
            if (OnEndOfData != null)
                OnEndOfData();
        }
    }
}
