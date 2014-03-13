using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Approaches.SGD
{
    /// <summary>
    /// Training package for regression tree
    /// </summary>
    class TrainingPackage
    {
        private string _forPrefix;
        /// <summary>
        /// Training from prefix
        /// </summary>
        public string ForPrefix
        {
            get { return _forPrefix; }
            set { _forPrefix = value; }
        }

        private bool _isFirstTrainingPackageForPrefix;
        /// <summary>
        /// Is the first training package for the prefix
        /// </summary>
        public bool IsFirstTrainingPackageForPrefix
        {
            get { return _isFirstTrainingPackageForPrefix; }
            set { _isFirstTrainingPackageForPrefix = value; }
        }

        private List<FeaturePackage> _trainingPackageQueries;
        /// <summary>
        /// Queries contained in the training package. Features are based on these.
        /// </summary>
        public List<FeaturePackage> TrainingPackageQueries
        {
            get { return _trainingPackageQueries; }
            set { _trainingPackageQueries = value; }
        }

        private int _queryCountAtCreation;

        public int QueryCountAtCreation
        {
            get { return _queryCountAtCreation; }
            set { _queryCountAtCreation = value; }
        }

        private int _queryCountAtTrain;

        public int QueryCountAtTrain
        {
            get { return _queryCountAtTrain; }
            set { _queryCountAtTrain = value; }
        }

        /// <summary>
        /// How many queries have passed since the last training (indicates how popular a prefix is - smaller is more popular)
        /// </summary>
        public int QueriesSinceLastTrain
        {
            get { return QueryCountAtTrain - QueryCountAtCreation; }
        }

        public TrainingPackage()
        {
            //
        }
    }
}
