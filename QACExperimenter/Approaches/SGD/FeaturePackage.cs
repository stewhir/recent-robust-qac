using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Approaches.SGD
{
    /// <summary>
    /// A query with features contained in the training package. May or may not include the target likelihood (depending on whether it is for training or prediction)
    /// </summary>
    class FeaturePackage
    {
        private string _query;
        /// <summary>
        /// Query
        /// </summary>
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        private double[] _ntbFeatures;
        /// <summary>
        /// NTB likelihoods
        /// </summary>
        public double[] NtbFeatures
        {
            get { return _ntbFeatures; }
            set { _ntbFeatures = value; }
        }

        private int _queriesSinceLastTrain;
        /// <summary>
        /// Number of queries since last training package from this prefix (specifies query prefix popularity)
        /// </summary>
        public int QueriesSinceLastTrain
        {
            get { return _queriesSinceLastTrain; }
            set { _queriesSinceLastTrain = value; }
        }

        private double _targetLikelihood;
        /// <summary>
        /// Target query likelihood in the last N queries
        /// </summary>
        public double TargetLikelihood
        {
            get { return _targetLikelihood; }
            set { _targetLikelihood = value; }
        }

        private double _predictedLikelihood;
        /// <summary>
        /// The predicted likelihood in the next N queries
        /// </summary>
        public double PredictedLikelihood
        {
            get { return _predictedLikelihood; }
            set { _predictedLikelihood = value; }
        }

        public string FeaturesAsString()
        {
            return String.Join(",", _ntbFeatures) + ',' + _targetLikelihood;
        }
    }
}
