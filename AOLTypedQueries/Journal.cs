using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOLTypedQueries
{
    /// <summary>
    /// Maintains a chronological journal of the additions/changes to the IndexEntry. Journal entries are stored as a linked list, with the oldest entry starting from OldestEntry
    /// </summary>
    public class Journal
    {
        private JournalEntry _oldestEntry;
        /// <summary>
        /// Oldest journal entry
        /// </summary>
        internal JournalEntry OldestEntry
        {
            get { return _oldestEntry; }
            set { _oldestEntry = value; }
        }

        private JournalEntry _newestEntry;

        private int _entryCount;
        /// <summary>
        /// Count of entries in the journal
        /// </summary>
        public int EntryCount
        {
            get { return _entryCount; }
            set { _entryCount = value; }
        }

        /// <summary>
        /// Initialise an empty journal
        /// </summary>
        public Journal()
        {
            //
        }

        /// <summary>
        /// Add an entry to the journal
        /// </summary>
        /// <param name="entry"></param>
        public void AddEntry(JournalEntry entry)
        {
            if (_oldestEntry == null)
            {
                // No existing entries
                _oldestEntry = entry;
                _newestEntry = entry;
            }
            else
            {
                _newestEntry.NextEntry = entry; // Append to existing entries
                _newestEntry = entry;
            }

            _entryCount++;
        }

        /// <summary>
        /// Iterator for entries before the provided date. After the entry has been returned it will be removed from the
        /// journal. (THIS METHOD IS *NOT* THREAD-SAFE).
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public IEnumerable<JournalEntry> GetEntriesBeforeDateAndDelete(DateTime beforeDateTime)
        {
            List<JournalEntry> toReturnList = new List<JournalEntry>();

            while (_oldestEntry != null && _oldestEntry.EntryDateTime < beforeDateTime)
            {
                // Now remove entry from the journal
                toReturnList.Add(_oldestEntry);

                _oldestEntry = _oldestEntry.NextEntry;

                _entryCount--;
            }

            return toReturnList;
        }
    }
}
