using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data
{
    public class QuerySubmitted
    {
        private string _query;

        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        private DateTime _queryDateTime;

        public DateTime QueryDateTime
        {
            get { return _queryDateTime; }
            set { _queryDateTime = value; }
        }

        public QuerySubmitted(string query, DateTime queryDateTime)
        {
            _query = query;
            _queryDateTime = queryDateTime;
        }

        /// <summary>
        /// Load from raw data line
        /// </summary>
        /// <param name="line"></param>
        public QuerySubmitted(string line)
        {
            string[] rows = line.Split('\t');
            _query = rows[0];
            _queryDateTime = DateTime.ParseExact(rows[1], "yyyy-MM-dd HH:mm:ss", null);

        }
    }
}
