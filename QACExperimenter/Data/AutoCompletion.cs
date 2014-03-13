using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data
{
    public class AutoCompletion : IComparable<AutoCompletion>
    {
        public string QueryCompletion;

        private string _queryCompletionWithoutSpaces;
        public string QueryCompletionWithoutSpaces
        {
            get
            {
                if (_queryCompletionWithoutSpaces == null)
                    _queryCompletionWithoutSpaces = QueryCompletion.Replace(" ", "");

                return _queryCompletionWithoutSpaces;
            }
        }

        public int Rank;
        public double RankingWeight;
        public bool IsWikiWeighted;

        public string Explain;

        public AutoCompletion()
        {
            //
        }

        public AutoCompletion(string queryCompletion)
        {
            QueryCompletion = queryCompletion;
        }

        public AutoCompletion(string queryCompletion, int rank, string explain = null)
        {
            QueryCompletion = queryCompletion;
            Rank = rank;
            Explain = explain;
        }

        /// <summary>
        /// Calculates the partial percentage coverage, in characters. Used to calculate the partial reciprocal rank.
        /// </summary>
        /// <param name="matchWith">Match with string (this should already have spaces removed)</param>
        /// <param name="charactersToIgnore">The number of characters to ignore in the calculation - this is just the prefix length</param>
        /// <returns></returns>
        public double PartialMatchCoverage(string matchWith, int charactersToIgnore)
        {
            if (!matchWith.StartsWith(this.QueryCompletionWithoutSpaces) || this.QueryCompletion.Length > matchWith.Length) // Ensure there is a partial overlap
                return 0;

            int charIndex = charactersToIgnore; // Start index for matches

            int matchedCharacters = 0;

            while (charIndex < matchWith.Length && charIndex < this.QueryCompletionWithoutSpaces.Length)
            {
                if (matchWith[charIndex] == QueryCompletionWithoutSpaces[charIndex])
                {
                    matchedCharacters++;
                    charIndex++;
                }
                else
                    break;
            }

            if (matchedCharacters == 0)
                return 0.0;

            // Calculate percentage coverage
            return Convert.ToDouble(matchedCharacters) / Convert.ToDouble(matchWith.Length - charactersToIgnore);
        }

        public override string ToString()
        {
            if (Explain != null)
                return Explain; // * to denote it is wiki weighted

            return QueryCompletion + "[" + RankingWeight + "]";
        }

        public int CompareTo(AutoCompletion other)
        {
            return other.RankingWeight.CompareTo(this.RankingWeight);
        }
    }
}
