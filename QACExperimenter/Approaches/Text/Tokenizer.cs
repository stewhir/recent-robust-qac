using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Approaches.Text
{
    /// <summary>
    /// Provides string tokenization and cleaning
    /// </summary>
    public static class Tokenizer
    {
        /// <summary>
        /// Simple whitespace tokenizer
        /// TODO: fix this to do filtering etc
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public static List<string> TokenizeString(string inputText, bool returnFirstNgramOnly = false)
        {
            List<string> returnToken = new List<string>();

            returnToken.Add(NormalizeQueryOrTitle(inputText));

            return returnToken;

            // OBSOLETE -> Now, return the full inputText, normalized
            string[] tokens = inputText.ToLower().Split(' ');

            // Single token
            if (tokens.Length == 1)
            {
                return new List<string>(tokens);
            }

            // Two tokens (return regardless of any stop words)
            if (tokens.Length == 2)
            {
                List<string> twoTokenReturn = new List<string>();
                twoTokenReturn.Add(StripPunctuation(tokens[0]) + ' ' + StripPunctuation(tokens[1]));

                return twoTokenReturn;
            }

            List<string> outputTokens = new List<string>();

            // Extract 2grams
            for (int i = 0; i < tokens.Length - 1; i++) // Goes to ubound index - 1
            {
                if (!_stopWords.Contains(tokens[i]) && !_stopWords.Contains(tokens[i + 1])) // Check stopwords
                {
                    outputTokens.Add(StripPunctuation(tokens[i]) + ' ' + StripPunctuation(tokens[i + 1]));
                }

                if (i == 0 && returnFirstNgramOnly)
                    return outputTokens;
            }

            return outputTokens;
        }

        /// <summary>
        /// Remove any punctuation (high-performance single pass method)
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private static string StripPunctuation(string inputString)
        {
            List<char> outputChars = new List<char>(inputString.Length);

            for (int i = 0; i < inputString.Length; i++)
            {
                if (char.IsLetter(inputString[i]) || char.IsNumber(inputString[i])
                    || inputString[i] == '_') // Allow letters, numbers, underscores and whitespace
                    outputChars.Add(inputString[i]);
            }

            return new string(outputChars.ToArray());
        }

        /// <summary>
        /// Normalizes a query or title to the same format, with punctuation removed.
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        private static string NormalizeQueryOrTitle(string inputText)
        {
            return inputText.Replace('-', ' ').Replace("'", "").Replace(".", "").Replace("?", "").ToLower();
        }

        /// <summary>
        /// Maintain a list of stopwords
        /// </summary>
        static HashSet<string> _stopWords = new HashSet<string>(new string[] { "new york", "video", "videos", "video game", "video games", "follow", "amp", "a", "able", "about", "across", "after", "all", "almost", "also", "am", "among", "an", "and", "any", "are", "as", "at", "be", "because", "been", "but", "by", "can", "cant", "cannot", "could", "couldnt", "dear", "did", "didnt", "do", "does", "dont", "either", "else", "ever", "every", "for", "from", "get", "got", "had", "hadnt", "has", "hasnt", "have", "havent", "he", "her", "hers", "him", "his", "how", "however", "i", "if", "ill", "in", "into", "is", "isnt", "it", "its", "just", "least", "let", "like", "likely", "may", "me", "might", "most", "must", "my", "neither", "no", "nor", "not", "of", "off", "often", "on", "only", "or", "other", "our", "own", "rather", "said", "say", "says", "see", "she", "should", "since", "so", "some", "sure", "surely", "than", "that", "the", "their", "them", "then", "there", "these", "they", "theyre", "this", "tis", "to", "too", "twas", "us", "wants", "want", "was", "we", "were", "what", "when", "whenever", "where", "which", "while", "who", "whom", "why", "will", "with", "wont", "would", "yet", "you", "your", "youre", "y" });
    }
}
