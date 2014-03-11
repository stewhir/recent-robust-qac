using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNTypedQueries
{
    /// <summary>
    /// Contains an entry in the journal
    /// </summary>
    public class JournalEntry
    {
        public DateTime EntryDateTime;

        public double WeightAddition;

        public string Query;

        public string Term;

        public JournalEntry NextEntry;

        public JournalEntryType EntryType;

        public enum JournalEntryType { QueryAdded, QueryReweighted, WikiChange };
    }
}
