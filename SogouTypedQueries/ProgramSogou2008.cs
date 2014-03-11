using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SogouTypedQueries
{
    class ProgramSogou2008
    {
        /// <summary>
        /// Extracts the typed queries from Sogou 2008
        /// </summary>
        /// <param name="args"></param>
        public static void MainSogou2008(string[] args)
        {
            Dictionary<string, int> lookupPastQueries = new Dictionary<string, int>();

            Journal journal = new Journal();

            StreamWriter fsOutput = new StreamWriter(new FileStream(@"D:\QueryLogs\SogouQ.2008\sogououtput.txt", FileMode.Create), Encoding.UTF8);

            int badCount = 0;

            foreach (string file in Directory.GetFiles(@"D:\QueryLogs\SogouQ.2008\", "*.filter"))
            {
                FileInfo fi = new FileInfo(file);

                string[] fileParts = fi.Name.Split('.');

                string datePart = fileParts[1].Substring(0, 4) + "-" + fileParts[1].Substring(4, 2) + "-" + fileParts[1].Substring(6, 2);

                using (StreamReader sr = new StreamReader(file, Encoding.GetEncoding(936)))
                {
                    String line = sr.ReadLine();

                    while (line != null)
                    {
                        string[] row = line.Split('\t');
                        DateTime queryTime = DateTime.Parse(datePart + ' ' + row[0]);

                        if (row[2].Replace("[", "").StartsWith("http") || row[2].Replace("[", "").StartsWith("www."))
                        {
                            line = sr.ReadLine();
                            continue;
                        }

                        string lookupKey = row[1] + row[2];

                        int pastCount = 0;
                        lookupPastQueries.TryGetValue(lookupKey, out pastCount);

                        // Ensure query doesn't already exist in the journal
                        if (pastCount == 0)
                        {
                            fsOutput.WriteLine(row[2].Replace("[", "").Replace("]", "") + '\t' + queryTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            badCount++;
                        }

                        // Add to journal
                        JournalEntry je = new JournalEntry();
                        je.EntryDateTime = queryTime;
                        je.Query = lookupKey; // Set query as lookup query
                        journal.AddEntry(je);

                        // Increment the counter for the query
                        if (lookupPastQueries.ContainsKey(lookupKey))
                            lookupPastQueries[lookupKey] += 1;
                        else
                            lookupPastQueries[lookupKey] = 1;
                        

                        // Remove old journal entries
                        foreach (JournalEntry je2 in journal.GetEntriesBeforeDateAndDelete(queryTime.AddMinutes(-30)))
                        {
                            lookupPastQueries[je2.Query] -= 1;

                            //if (lookupPastQueries[je2.Query] == 0)
                                //lookupPastQueries.Remove(je2.Query);
                        }

                        line = sr.ReadLine();
                    }
                }
            }

            fsOutput.Close();
        }
    }
}
