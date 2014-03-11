using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNTypedQueries
{
    /// <summary>
    /// Create the typed query dataset from MSN 2006 raw query logs.
    /// NOTE: Make sure you change the input query log and output file paths
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> lookupPastQueries = new HashSet<string>();

            Journal journal = new Journal();

            StreamWriter fsOutput = new StreamWriter(new FileStream(@"D:\QueryLogs\msn-queries-new.txt", FileMode.Create), Encoding.UTF8);

            string[] files = new string[] { @"D:\QueryLogs\srfp20060501-20060531.queries.txt" };

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

                        string queryTime = row[0];
                        string query = row[1].ToLower();
                        string sessionId = row[3];

                        if (query.StartsWith("http") || query.StartsWith("www."))
                        {
                            line = sr.ReadLine();
                            continue;
                        }

                        if (queryTime == "Time")
                        {
                            line = sr.ReadLine();
                            continue;
                        }

                        string lookupKey = sessionId + query; // Lookup

                        // Ensure query doesn't already exist in the journal
                        if (!lookupPastQueries.Contains(lookupKey))
                        {
                            if (query != "-")
                                fsOutput.WriteLine(query + '\t' + queryTime);
                        }

                        lookupPastQueries.Add(lookupKey);

                        line = sr.ReadLine();
                    }

                    fsOutput.Close();
                }
            }
        }
    }
}
