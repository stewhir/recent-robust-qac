using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data.IndexCore
{
    /// <summary>
    /// Maintains an index of term weights.
    /// </summary>
    public class TermWeightIndex
    {
        private ConcurrentDictionary<string, double> _entries;

        private double _totalWeight;
        /// <summary>
        /// Total weight of the term weight index (useful for probability normalisation etc)
        /// </summary>
        public double TotalWeight
        {
            get { return _totalWeight; }
        }

        public TermWeightIndex()
        {
            _entries = new ConcurrentDictionary<string, double>();
            _totalWeight = 0;
        }

        /// <summary>
        /// Add term weight
        /// </summary>
        /// <param name="indexEntry"></param>
        public void AddTermWeight(string term, double weight)
        {
            if (_entries.ContainsKey(term))
                _entries[term] += weight;
            else
                _entries[term] = weight;

            // Increment the total weight
            _totalWeight += weight;
        }

        /// <summary>
        /// Subtract term weight
        /// </summary>
        /// <param name="term"></param>
        /// <param name="weight"></param>
        public void SubtractTermWeight(string term, double weight)
        {
            if (_entries.ContainsKey(term))
                _entries[term] -= weight;

            _totalWeight -= weight;
        }

        /// <summary>
        /// Get the term weight, returns 0 if none
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public double GetTermWeight(string term)
        {
            double termWeight = 0;

            _entries.TryGetValue(term, out termWeight);

            return termWeight;
        }
    }
}
