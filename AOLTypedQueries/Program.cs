using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOLTypedQueries
{
    /// <summary>
    /// Create the typed query dataset from AOL 2006 raw query logs.
    /// NOTE: Make sure you change the input query log and output file paths
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, int> lookupPastQueries = new Dictionary<string, int>();

            Journal journal = new Journal();

            StreamWriter fsOutput = new StreamWriter(new FileStream(@"D:\aol-queries-new.txt", FileMode.Create), Encoding.UTF8);

            string[] files = new string[] { @"C:\hadoop-cdh4.0\aol-filtered.txt" };

            foreach (string file in files)
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    String line = sr.ReadLine();

                    int counter = 0;

                    while (line != null)
                    {
                        counter++;

                        if (counter % 10000 == 0)
                            Console.WriteLine("Reached: " + counter.ToString());

                        string[] row = line.Split('\t');

                        string anonId = row[0];

                        if (anonId == "AnonID")
                        {
                            line = sr.ReadLine();
                            continue;
                        }

                        string query = row[2].ToLower();

                        if (query.StartsWith("http") || query.StartsWith("www."))
                        {
                            line = sr.ReadLine();
                            continue;
                        }

                        string queryTime = row[1];
                        DateTime queryTimeDT = DateTime.Parse(queryTime);

                        string lookupKey = anonId + query; // Lookup

                        int pastCount = 0;
                        lookupPastQueries.TryGetValue(lookupKey, out pastCount);

                        // Ensure query doesn't already exist in the journal
                        if (pastCount == 0)
                        {
                            if (query != "-")
                                fsOutput.WriteLine(query + '\t' + queryTime);
                        }

                        // Add to journal
                        JournalEntry je = new JournalEntry();
                        je.EntryDateTime = queryTimeDT;
                        je.Query = lookupKey; // Set query as lookup query
                        journal.AddEntry(je);

                        // Increment the counter for the query
                        if (lookupPastQueries.ContainsKey(lookupKey))
                            lookupPastQueries[lookupKey] += 1;
                        else
                            lookupPastQueries[lookupKey] = 1;

                        // Remove old journal entries
                        foreach (JournalEntry je2 in journal.GetEntriesBeforeDateAndDelete(queryTimeDT.AddMinutes(-30)))
                        {
                            lookupPastQueries[je2.Query] -= 1;
                        }

                        line = sr.ReadLine();
                    }

                    fsOutput.Close();
                }
            }
        }
    }
}
