using System.Globalization;
using System.Linq;
using System.Text;

namespace Azyotter.Models.TwitterText
{
    /// <summary>
    /// A class for validating Tweet texts.
    /// </summary>
    public class Validator
    {
        public const int MAX_TWEET_LENGTH = 140;

        public Validator()
        {
            ShortUrlLength = 22;
            ShortUrlLengthHttps = 23;
        }

        public int ShortUrlLength { get; set; }
        public int ShortUrlLengthHttps { get; set; }

        private Extractor extractor = new Extractor();

        public int GetTweetLength(string text)
        {
            text = text.Normalize(NormalizationForm.FormC);
            var length = new StringInfo(text).LengthInTextElements;

            extractor.ExtractURLsWithIndices(text).ForEach(urlEntity =>
            {
                length += urlEntity.Start - urlEntity.End;
                length += urlEntity.Value.ToLower().StartsWith("https://") ? ShortUrlLengthHttps : ShortUrlLength;
            });

            return length;
        }

        public bool IsValidTweet(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if(text.Any(c=>
                c == '\uFFFE' || c == '\uFEFF' ||   // BOM
                c == '\uFFFF' ||                     // Special
                (c >= '\u202A' && c <= '\u202E')))   // Direction change
            {
                return false;
            }

            return GetTweetLength(text) <= MAX_TWEET_LENGTH;
        }
    }
}
