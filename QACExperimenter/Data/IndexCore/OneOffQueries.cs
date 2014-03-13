using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace QACExperimenter.Data.IndexCore
{
    /// <summary>
    /// Maintains a hashset of one-off queries to exclude from the prefix index (as they'll never be suggested - they just slow things down)
    /// </summary>
    public class OneOffQueries
    {
        private HashSet<string> _oneOffQueries;

        public OneOffQueries(FileInfo queryFile)
        {
            _oneOffQueries = new HashSet<string>();

            using (StreamReader reader = new StreamReader(queryFile.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    _oneOffQueries.Add(line);
                }
            }
        }

        /// <summary>
        /// Determine whether a query is a one-off query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool IsOneOffQuery(string query)
        {
            return _oneOffQueries.Contains(query);
        }
    }
}
