using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data
{
    /// <summary>
    /// Contains past query evidence from the database
    /// </summary>
    public class QueryEvidence
    {
        public string TermString;
        public int TermStringFrequency;

        public QueryEvidence(string termString, int termStringFrequency)
        {
            TermString = termString;
            TermStringFrequency = termStringFrequency;
        }

        public override string ToString()
        {
            return TermString + ": " + TermStringFrequency;
        }
    }
}
