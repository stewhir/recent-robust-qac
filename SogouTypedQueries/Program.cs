using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SogouTypedQueries
{
    /// <summary>
    /// Create the typed query dataset from Sogou 2008 and 2012 raw query logs.
    /// NOTE: Make sure you change the input query log and output file paths in ProgramSogou2008.cs and ProgramSogou2012.cs
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // arg[0] specifies the Sogou dataset year (they're in different formats) - must be '2008' or '2012'
            if (args[0] == "2008")
                ProgramSogou2008.MainSogou2008(args);
            else if (args[0] == "2012")
                ProgramSogou2012.MainSogou2012(args);
        }
    }
}
