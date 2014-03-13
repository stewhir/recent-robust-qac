using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QACExperimenter.Approaches;

namespace QACExperimenter.Data.IndexCore
{
    public class BaseIndexEntry : IComparable<BaseIndexEntry>
    {
        public string DebugToStringOutput;

        public string Query;
        
        private List<string> _queryTerms;
        /// <summary>
        /// Extract the query terms from the query. Uses the clean term tokenizer that removes non-alphanumeric characters.
        /// </summary>
        public List<string> QueryTerms
        {
            get 
            {
                if (_queryTerms == null)
                    _queryTerms = QACExperimenter.Approaches.Text.Tokenizer.TokenizeString(Query, true);

                return _queryTerms;
            }
        }

        public bool IsWikiWeighted = false;

        /// <summary>
        /// Frequency in query log
        /// </summary>
        public double QueryLogFrequency;

        /// <summary>
        /// Approach being used by EQC experiment (can be used to get indexes directly)
        /// </summary>
        public BaseApproach Approach;

        /// <summary>
        /// A signal to refresh the query term weights as a new Wiki change has occurred which contains one or more of them.
        /// </summary>
        public bool RefreshQueryTermWeights;

        /// <summary>
        /// Override this to determine how the ranking should be performed. By default just the count.
        /// </summary>
        public virtual double RankingWeight
        {
            get { return QueryLogFrequency; }
        }

        /// <summary>
        /// Set during sorting
        /// </summary>
        public double RankingWeightForSorting;

        public virtual string RankingExplain
        {
            get { return "QL Freq: " + QueryLogFrequency; }
        }

        public BaseIndexEntry()
        {
            RefreshQueryTermWeights = true; // Default to true. Ignored by this base class, but may be used by extending classes that use Wiki change evidence.
        }

        public int CompareTo(BaseIndexEntry other)
        {
            return other.RankingWeightForSorting.CompareTo(this.RankingWeightForSorting);
        }

        public override string ToString()
        {
            if (DebugToStringOutput != null)
                return DebugToStringOutput;

            return Query + "[" + RankingWeight + "]";
        }
    }
}
