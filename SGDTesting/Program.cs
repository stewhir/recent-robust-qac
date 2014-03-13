using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDTesting
{
    /// <summary>
    /// Random instance stochastic algorithm gradient testing. Useful for testing throughput, and learning/understanding how the algorithm works.
    /// Watch the squared error decrease as SGD fits the appropriate parameters with more iterations.
    /// </summary>
    class Program
    {
        /// <summary>
        /// 4 params + target (5 dimensions)
        /// </summary>
        static List<double[]> _instances = new List<double[]>();

        static double[] _linRegParams;

        /// <summary>
        /// Test Stochastic Gradient Descent for learning linear regression parameters
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 5 parameters for now - random start (parameter 0 is the intercept)
            _linRegParams = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0 };

            _instances.Add(new double[] { 1.0, 2.0, 3.0, 4.0, 4.0, 5.0 });
            _instances.Add(new double[] { 1.0, 1.0, 0, 1.0, 1.0, 0.0 });
            _instances.Add(new double[] { 1.0, 10.0, 0.0, 2.0, 0.0, 2.0 });
            _instances.Add(new double[] { 1.0, 8.0, 5.0, 8.0, 6.0, 5.0 });
            _instances.Add(new double[] { 1.0, 0.0, 0.0, 0.0, 1.0, 1.0 });
            _instances.Add(new double[] { 1.0, 8.0, 1.0, 1.0, 0.0, 1.0 });
            _instances.Add(new double[] { 1.0, 1.0, 1.0, 0.0, 0.0, 0.0 });

            double learningRateAlpha = 0.0001; // Define learning rate

            for (int i = 0; i <= 10000000; i++)
            {
                double[] instance = RandomInstance();
                double target = instance[4]; // Last dimension
                double prediction = Predict(instance);

                double squaredError = Math.Pow(Math.Abs(target - prediction), 2);

                // Do SGD step for each parameter
                for (int j = 0; j < _linRegParams.Length; j++)
                {
                    _linRegParams[j] = _linRegParams[j] - (
                        learningRateAlpha * (prediction - target) * instance[j]
                        );
                }

                // Output debug
                if (i % 10000 == 0)
                {
                    Console.WriteLine("At: " + i.ToString() + ", Params: " + String.Join(",", _linRegParams));
                    Console.WriteLine("Squared Error: " + squaredError.ToString("F6"));
                }
            }

            Console.WriteLine();
        }

        static double Predict(double[] instance)
        {
            return (_linRegParams[0] * instance[0]) +
                (_linRegParams[1] * instance[1]) +
                (_linRegParams[2] * instance[2]) +
                (_linRegParams[3] * instance[3]) +
                (_linRegParams[4] * instance[4]);
        }

        static double[] RandomInstance()
        {
            Random rand = new Random();
            int randIndex = rand.Next(0, _instances.Count());

            return _instances[randIndex];
        }
    }
}
