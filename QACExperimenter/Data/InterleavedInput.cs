using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data
{
    public class InterleavedInput
    {
        private DateTime _inputDateTime;
        /// <summary>
        /// Date time of input
        /// </summary>
        public DateTime InputDateTime
        {
            get { return _inputDateTime; }
            set { _inputDateTime = value; }
        }

        private string _inputLine;
        /// <summary>
        /// Raw input line (TODO: parse out)
        /// </summary>
        public string InputLine
        {
            get { return _inputLine; }
            set { _inputLine = value; }
        }

        public InterleavedInput(DateTime inputDateTime, string inputLine)
        {
            _inputDateTime = inputDateTime;
            _inputLine = inputLine;
        }
    }
}
