using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QACExperimenter.Data;

namespace QACExperimenter.Evaluation
{
    /// <summary>
    /// Contains evaluation information
    /// </summary>
    public class EvaluationOutput
    {
        public DateTime QueryTime;
        public string PartialQuery;
        public string FullQuery;

        private string _fullQueryWithoutSpaces;
        public string FullQueryWithoutSpaces
        {
            get
            {
                if (_fullQueryWithoutSpaces == null)
                    _fullQueryWithoutSpaces = FullQuery.Replace(" ", "");

                return _fullQueryWithoutSpaces;
            }
        }

        public AutoCompletionList AutoCompletionList;
        public int QueryCount;

        public double ReciprocalRank;
        public bool IsPartialMatch;
        public string RunIdentifier;

        public EvaluationOutput(DateTime queryTime, string partialQuery, string fullQuery, AutoCompletionList autoCompletionList, int queryCount)
        {
            QueryTime = queryTime;
            PartialQuery = partialQuery;
            FullQuery = fullQuery;
            AutoCompletionList = autoCompletionList;
            QueryCount = queryCount;
        }
    }
}
