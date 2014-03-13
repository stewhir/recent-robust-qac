using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data
{
    public class AutoCompletionList : List<AutoCompletion>
    {
        public AutoCompletionList()
        {
            //
        }

        public AutoCompletionList(IEnumerable<AutoCompletion> inputList)
        {
            foreach (AutoCompletion autoCompletion in inputList)
                this.Add(autoCompletion);
        }

        public void SetRanks()
        {
            int count = 1;

            foreach (AutoCompletion autoCompletion in this)
            {
                autoCompletion.Rank = count;

                count++;
            }
        }


    }
}
