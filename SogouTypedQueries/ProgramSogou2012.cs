using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.IO;

namespace SogouTypedQueries
{
    class ProgramSogou2012
    {
        /// <summary>
        /// Extracts the typed queries from Sogou 2012
        /// </summary>
        /// <param name="args"></param>
        public static void MainSogou2012(string[] args)
        {
            HashSet<string> lookupPastQueries = new HashSet<string>();

            Journal journal = new Journal();

            StreamWriter fsOutput = new StreamWriter(new FileStream(@"D:\Query log data\SogouQ.2012.full\Sogou2012-queries.txt", FileMode.CreateNew), Encoding.UTF8);

            string[] files = new string[] { @"D:\Query log data\SogouQ.2012.full\querylog.txt" };

            foreach (string file in files)
            {
                using (StreamReader sr = new StreamReader(file, Encoding.GetEncoding(936)))
                {
                    String line = sr.ReadLine();

                    int counter = 0;

                    while (line != null)
                    {
                        counter++;

                        if (counter % 10000 == 0)
                            Console.WriteLine("Reached: " + counter.ToString());

                        string[] row = line.Split('\t');
                        DateTime queryTime = DateTime.ParseExact(row[0], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                        string lookupKey = row[1] + row[2];

                        // Ensure query doesn't already exist in the journal
                        if (!lookupPastQueries.Contains(lookupKey))
                        {
                            fsOutput.WriteLine(row[2].Replace("[", "").Replace("]", "") + '\t' + queryTime.ToString("yyyy-MM-dd HH:mm:ss"));

                            // Add to journal
                            JournalEntry je = new JournalEntry();
                            je.EntryDateTime = queryTime;
                            je.Query = lookupKey; // Set query as lookup query
                            journal.AddEntry(je);
                        }

                        // Remove old journal entries
                        foreach (JournalEntry je in journal.GetEntriesBeforeDateAndDelete(queryTime.AddMinutes(-30)))
                        {
                            lookupPastQueries.Remove(je.Query);
                        }

                        line = sr.ReadLine();
                    }
                }
            }
        }
    }
}
