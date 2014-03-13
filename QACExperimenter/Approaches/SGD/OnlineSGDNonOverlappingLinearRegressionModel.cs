using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QACExperimenter.Evaluation;
using QACExperimenter.Data;
using QACExperimenter.Data.IndexCore;
using QACExperimenter.Data.Structures;
using QACExperimenter.Data.Structures.NonTemporal;

namespace QACExperimenter.Approaches.SGD
{
    /// <summary>
    /// Online training a non-overlapping linear regression model for the MNTB settings using SGD.
    /// </summary>
    class OnlineSGDNonOverlappingLinearRegressionModel
    {
        /// <summary>
        /// Whether to output training data (for external testing e.g. in Weka) to console
        /// </summary>
        private bool _outputTrainingItems = false;

        private double _learningRateAlpha = 0.01;
        /// <summary>
        /// Default constant 0.001 learning rate for now
        /// </summary>
        public double LearningRateAlpha
        {
            get { return _learningRateAlpha; }
            set { _learningRateAlpha = value; }
        }

        private int _trainingInstances;
        /// <summary>
        /// Count the number of training instances
        /// </summary>
        public int TrainingInstances
        {
          get { return _trainingInstances; }
          set { _trainingInstances = value; }
        }   

        private double[] _modelWeights;
        /// <summary>
        /// Final model weights determined by linear regression (use these to weight query frequency)
        /// </summary>
        public double[] ModelWeights
        {
            get { return _modelWeights; }
            set { _modelWeights = value; }
        }

        private double _maxNtbSize; // Max NTB size to be used as a scale factor
        private double _predictionHorizon; // Next N queries to predict for

        /// <summary>
        /// Derfault constructor
        /// </summary>
        /// <param name="trainingFeatures">Number of features to use</param>
        /// <param name="maxFeatureSize">Max size of features for scaling (assumes all NTB features are the same max size)</param>
        public OnlineSGDNonOverlappingLinearRegressionModel(int trainingFeatures, int maxNtbSize, int predictionHorizon)
        {
            Console.WriteLine("Initialising online learning SGD linear regression model");
            
            _modelWeights = new double[trainingFeatures];
            for (int i = 0; i < _modelWeights.Length; i++)
            {
                _modelWeights[i] = 0.5; // Default to random 0.5 value at first - this will soon converge
            }

            _maxNtbSize = Convert.ToDouble(maxNtbSize);
            _predictionHorizon = Convert.ToDouble(predictionHorizon);
        }

        /// <summary>
        /// Internal prediction method
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private double Predict(double[] instance)
        {
            double prediction = 0;

            for (int i = 0; i < _modelWeights.Length; i++)
            {
                prediction += (_modelWeights[i] * (instance[i])); // Sum the linear parameter weights
            }

            return prediction;
        }

        /// <summary>
        /// Publicly make prediction, as a query count (rather than scaled)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public double MakePrediction(double[] instanceIn)
        {
            double[] instance = new double[instanceIn.Length];

            for (int i = 0; i < instance.Length; i++)
                instance[i] = instanceIn[i] / _maxNtbSize;

            double prediction = Predict(instance);

            double outputPrediction = prediction * _predictionHorizon;

            if (outputPrediction < 0)
                return 0;

            return outputPrediction;
        }

        private  double _totalSqrdError = 0;

        /// <summary>
        /// Train the model with a FeaturePackage for a query
        /// </summary>
        /// <param name="features"></param>
        /// <param name="target"></param>
        public void TrainModel(FeaturePackage fp)
        {
            double[] instance = fp.NtbFeatures; //new double[fp.NtbFeatures.Length + 1];
            // Prepend the 1.0 intercept parameter to the features
            //instance[0] = 1.0;
            //for (int i = 0; i < fp.NtbFeatures.Length; i++)
              //  instance[i + 1] = fp.NtbFeatures[i]; // Set the rest of the features

            // Set scale factors
            for (int i = 0; i < instance.Length; i++)
                instance[i] = instance[i] / _maxNtbSize;

            double target = fp.TargetLikelihood / _predictionHorizon; // Predict in next 100 - so use 100 scaling factor
            double prediction = Predict(instance);

            double squaredError = Math.Pow(target - prediction, 2);
            _totalSqrdError += squaredError;

            // Iterate using SGD for each parameter
            for (int j = 0; j < _modelWeights.Length; j++)
            {
                double error = (prediction - target);

                _modelWeights[j] = _modelWeights[j] - (
                    _learningRateAlpha * error * instance[j]
                    );
            }




            _trainingInstances++;

            if (_trainingInstances % 2000 == 0)
            {
                string modelStr = "";
                foreach (double weight in _modelWeights)
                    modelStr += weight.ToString("F8") + ", ";

                // Output
                Console.WriteLine("SGD LR Training instances at " + _trainingInstances.ToString() + ", avg sqrd error: " + (_totalSqrdError / Convert.ToDouble(_trainingInstances)).ToString() + ", model: " + modelStr);
            }
        }
    }
}
