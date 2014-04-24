using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Azyotter.Models.TwitterText
{
    /// <summary>
    /// A class to extract usernames, lists, hashtags and URLs from Tweet text.
    /// </summary>
    public class Extractor
    {
        public class Entity
        {
            public enum Types
            {
                URL, HASHTAG, MENTION, CASHTAG
            }
            public int Start { get; protected set; }
            public int End { get; protected set; }
            public string Value { get; protected set; }
            // listSlug is used to store the list portion of @mention/list.
            public string ListSlug { get; protected set; }
            public Types Type { get; protected set; }

            public string DisplayURL { get; set; }
            public string ExpandedURL { get; set; }

            public Entity(int start, int end, string value, string listSlug, Types type)
            {
                this.Start = start;
                this.End = end;
                this.Value = value;
                this.ListSlug = listSlug;
                this.Type = type;
            }

            public Entity(int start, int end, string value, Types type)
                : this(start, end, value, null, type)
            { }

            public Entity(Match matcher, Types type, int groupNumber)
                // Offset -1 on start index to include @, # symbols for mentions and hashtags
                : this(matcher, type, groupNumber, -1)
            { }

            public Entity(Match matcher, Types type, int groupNumber, int startOffset)
                : this(
                  matcher.Groups[groupNumber].Index + startOffset,
                  matcher.Groups[groupNumber].Index + matcher.Groups[groupNumber].Length,
                  matcher.Groups[groupNumber].Value,
                  type)
            { }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }

                if (!(obj is Entity))
                {
                    return false;
                }

                Entity other = (Entity)obj;

                if (this.Type.Equals(other.Type) &&
                    this.Start == other.Start &&
                    this.End == other.End &&
                    this.Value.Equals(other.Value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return this.Type.GetHashCode() + this.Value.GetHashCode() + this.Start + this.End;
            }

            public override string ToString()
            {
                return Value + "(" + Type + ") [" + Start + "," + End + "]";
            }
        }

        public bool ExtractURLWithoutProtocol { get; set; }

        /// <summary>
        /// Create a new extractor.
        /// </summary>
        public Extractor()
        {
            ExtractURLWithoutProtocol = true;
        }

        private void RemoveOverlappingEntities(List<Entity> entities)
        {
            entities.Sort((e1, e2) => e1.Start - e2.Start);

            // Remove overlapping entities.
            // Two entities overlap only when one is URL and the other is hashtag/mention
            // which is a part of the URL. When it happens, we choose URL over hashtag/mention
            // by selecting the one with smaller start index.
            if (entities.Any())
            {
                var removeList = new List<Entity>();
                var prev = entities[0];
                entities.Skip(1).ForEach(cur =>
                {
                    if (prev.End > cur.Start)
                        removeList.Add(cur);
                    else
                        prev = cur;
                });
                removeList.ForEach(e => entities.Remove(e));
            }
        }

        /// <summary>
        /// Extract URLs, @mentions, lists and #hashtag from a given text/tweet.
        /// </summary>
        /// <param name="text">text of tweet</param>
        /// <returns>list of extracted entities</returns>
        public List<Entity> ExtractEntitiesWithIndices(string text)
        {
            var entities = new List<Entity>();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Extract @username references from Tweet text. A mention is an occurance of @username anywhere in a Tweet.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract usernames</param>
        /// <returns>List of usernames referenced (without the leading @ sign)</returns>
        public List<string> ExtractMentionedScreennames(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            return ExtractMentionedScreennamesWithIndices(text)
                .Select(entity => entity.Value)
                .ToList();
        }

        /// <summary>
        /// Extract @username references from Tweet text. A mention is an occurance of @username anywhere in a Tweet.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract usernames</param>
        /// <returns>List of usernames referenced (without the leading @ sign)</returns>
        public List<Entity> ExtractMentionedScreennamesWithIndices(string text)
        {
            return ExtractMentionsOrListsWithIndices(text)
                .Where(entity => entity.ListSlug == null)
                .ToList();
        }

        public List<Entity> ExtractMentionsOrListsWithIndices(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<Entity>();
            }

            // Performance optimization.
            // If text doesn't contain @/＠ at all, the text doesn't
            // contain @mention. So we can simply return an empty list.
            var found = false;
            foreach (var c in text)
            {
                if (c == '@' || c == '＠')
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return new List<Entity>();
            }

            var extracted = new List<Entity>();
            TwitterRegex.VALID_MENTION_OR_LIST.Matches(text).Cast<Match>().ForEach(matcher =>
            {
                var after = text.Substring(matcher.Index + matcher.Length);
                if (!TwitterRegex.INVALID_MENTION_MATCH_END.IsMatch(after))
                {
                    if (!matcher.Groups[TwitterRegex.VALID_MENTION_OR_LIST_GROUP_LIST].Success)
                    {
                        extracted.Add(new Entity(matcher, Entity.Types.MENTION, TwitterRegex.VALID_MENTION_OR_LIST_GROUP_USERNAME));
                    }
                    else
                    {
                        extracted.Add(new Entity(matcher.Groups[TwitterRegex.VALID_MENTION_OR_LIST_GROUP_USERNAME].Index - 1,
                          matcher.Groups[TwitterRegex.VALID_MENTION_OR_LIST_GROUP_LIST].Index + matcher.Groups[TwitterRegex.VALID_MENTION_OR_LIST_GROUP_LIST].Length,
                          matcher.Groups[TwitterRegex.VALID_MENTION_OR_LIST_GROUP_USERNAME].Value,
                          matcher.Groups[TwitterRegex.VALID_MENTION_OR_LIST_GROUP_LIST].Value,
                          Entity.Types.MENTION));
                    }
                }
            });
            return extracted;
        }

        /// <summary>
        /// Extract a @username reference from the beginning of Tweet text. A reply is an occurance of @username at the
        /// beginning of a Tweet, preceded by 0 or more spaces.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string ExtractReplyScreenname(string text)
        {
            if (text == null)
            {
                return null;
            }

            var matcher = TwitterRegex.VALID_REPLY.Match(text);
            if (matcher.Success)
            {
                var after = text.Substring(matcher.Index + matcher.Length);
                if (TwitterRegex.INVALID_MENTION_MATCH_END.IsMatch(after))
                {
                    return null;
                }
                else
                {
                    return matcher.Groups[TwitterRegex.VALID_REPLY_GROUP_USERNAME].Value;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Extract URL references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract URLs</param>
        /// <returns>List of URLs referenced.</returns>
        public List<string> ExtractURLs(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            return ExtractURLsWithIndices(text)
                .Select(entity => entity.Value)
                .ToList();
        }

        /// <summary>
        /// Extract URL references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract URLs</param>
        /// <returns>text of the tweet from which to extract URLs</returns>
        public List<Entity> ExtractURLsWithIndices(string text)
        {
            if (string.IsNullOrEmpty(text) || !(ExtractURLWithoutProtocol ? text.Contains('.') : text.Contains(':')))
            {
                // Performance optimization.
                // If text doesn't contain '.' or ':' at all, text doesn't contain URL,
                // so we can simply return an empty list.
                return new List<Entity>();
            }

            var urls = new List<Entity>();

            TwitterRegex.VALID_URL.Matches(text).Cast<Match>().ForEach(matcher =>
            {
                if (!matcher.Groups[TwitterRegex.VALID_URL_GROUP_PROTOCOL].Success)
                {
                    // skip if protocol is not present and 'extractURLWithoutProtocol' is false
                    // or URL is preceded by invalid character.
                    if (!ExtractURLWithoutProtocol
                        || TwitterRegex.INVALID_URL_WITHOUT_PROTOCOL_MATCH_BEGIN
                            .IsMatch(matcher.Groups[TwitterRegex.VALID_URL_GROUP_BEFORE].Value))
                    {
                        return;
                    }
                }
                var url = matcher.Groups[TwitterRegex.VALID_URL_GROUP_URL].Value;
                var start = matcher.Groups[TwitterRegex.VALID_URL_GROUP_URL].Index;
                var end = start + matcher.Groups[TwitterRegex.VALID_URL_GROUP_URL].Length;
                var tco_matcher = TwitterRegex.VALID_TCO_URL.Match(url);
                if (tco_matcher.Success)
                {
                    // In the case of t.co URLs, don't allow additional path characters.
                    url = tco_matcher.Value;
                    end = start + url.Length;
                }

                urls.Add(new Entity(start, end, url, Entity.Types.URL));
            });

            return urls;
        }

        /// <summary>
        /// public List<String> extractHashtags(String text) {
        /// </summary>
        /// <param name="text">text of the tweet from which to extract hashtags</param>
        /// <returns>List of hashtags referenced (without the leading # sign)</returns>
        public List<string> ExtractHashtags(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            return ExtractHashtagsWithIndices(text)
                .Select(entity => entity.Value)
                .ToList();
        }

        /// <summary>
        /// Extract #hashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract hashtags</param>
        /// <returns>List of hashtags referenced (without the leading # sign)</returns>
        public List<Entity> ExtractHashtagsWithIndices(string text)
        {
            return ExtractHashtagsWithIndices(text, true);
        }


        /// <summary>
        /// Extract #hashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract hashtags</param>
        /// <param name="checkUrlOverlap">if true, check if extracted hashtags overlap URLs and remove overlapping ones</param>
        /// <returns>if true, check if extracted hashtags overlap URLs and remove overlapping ones</returns>
        private List<Entity> ExtractHashtagsWithIndices(string text, bool checkUrlOverlap)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<Entity>();
            }

            // Performance optimization.
            // If text doesn't contain #/＃ at all, text doesn't contain
            // hashtag, so we can simply return an empty list.
            var found = false;
            foreach (var c in text)
            {
                if (c == '#' || c == '＃')
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return new List<Entity>();
            }

            var extracted = new List<Entity>();

            TwitterRegex.VALID_HASHTAG.Matches(text).Cast<Match>().ForEach(matcher =>
            {
                var after = text.Substring(matcher.Index + matcher.Length);
                if (!TwitterRegex.INVALID_HASHTAG_MATCH_END.IsMatch(after))
                {
                    extracted.Add(new Entity(matcher, Entity.Types.HASHTAG, TwitterRegex.VALID_HASHTAG_GROUP_TAG));
                }
            });

            if (checkUrlOverlap)
            {
                // extract URLs
                var urls = ExtractURLsWithIndices(text);
                if (urls.Any())
                {
                    extracted.AddRange(urls);
                    // remove overlap
                    RemoveOverlappingEntities(extracted);
                    // remove URL entities
                    extracted.Where(entity => entity.Type != Entity.Types.HASHTAG)
                        .ToArray().ForEach(e => extracted.Remove(e));
                }
            }

            return extracted;
        }

        /// <summary>
        /// Extract $cashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract cashtags</param>
        /// <returns>List of cashtags referenced (without the leading $ sign)</returns>
        public List<string> ExtractCashtags(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            return ExtractCashtagsWithIndices(text)
                .Select(entity => entity.Value)
                .ToList();
        }

        /// <summary>
        /// Extract $cashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract cashtags</param>
        /// <returns>List of cashtags referenced (without the leading $ sign)</returns>
        public List<Entity> ExtractCashtagsWithIndices(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<Entity>();
            }

            // Performance optimization.
            // If text doesn't contain $, text doesn't contain
            // cashtag, so we can simply return an empty list.
            if (!text.Contains('$'))
            {
                return new List<Entity>();
            }

            return TwitterRegex.VALID_CASHTAG.Matches(text).Cast<Match>()
                .Select(matcher => new Entity(matcher, Entity.Types.CASHTAG, TwitterRegex.VALID_CASHTAG_GROUP_CASHTAG))
                .ToList();
        }

        /// <summary>
        /// Modify Unicode-based indices of the entities to UTF-16 based indices.
        ///
        /// In UTF-16 based indices, Unicode supplementary characters are counted as two characters.
        ///
        /// This method requires that the list of entities be in ascending order by start index.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities with Unicode based indices</param>
        public void ModifyIndicesFromUnicodeToUTF16(string text, List<Entity> entities)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Modify UTF-16-based indices of the entities to Unicode-based indices.
        /// 
        /// In Unicode-based indices, Unicode supplementary characters are counted as single characters.
        /// 
        /// This method requires that the list of entities be in ascending order by start index.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities with UTF-16 based indices</param>
        public void modifyIndicesFromUTF16ToToUnicode(string text, List<Entity> entities)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// An efficient converter of indices between code points and code units.
        /// </summary>
        private sealed class IndexConverter
        {
            protected readonly string text;

            // Keep track of a single corresponding pair of code unit and code point
            // offsets so that we can re-use counting work if the next requested
            // entity is near the most recent entity.
            protected int codePointIndex = 0;
            protected int charIndex = 0;

            IndexConverter(string text)
            {
                this.text = text;
            }

            /// <param name="charIndex">Index into the string measured in code units.</param>
            /// <returns>The code point index that corresponds to the specified character index.</returns>
            int CodeUnitsToCodePoints(int charIndex)
            {
                if (charIndex < this.charIndex)
                {
                    this.codePointIndex -= new StringInfo(text.Substring(charIndex, this.charIndex - charIndex)).LengthInTextElements;
                }
                else
                {
                    this.codePointIndex += new StringInfo(text.Substring(this.charIndex, charIndex - this.charIndex)).LengthInTextElements;
                }
                this.charIndex = charIndex;

                // Make sure that charIndex never points to the second code unit of a
                // surrogate pair.
                if (charIndex > 0 && StringInfo.GetNextTextElement(text, charIndex - 1).Length > 1)
                {
                    this.charIndex -= 1;
                }
                return this.codePointIndex;
            }

            /// <param name="codePointIndex">Index into the string measured in code points.</param>
            /// <returns>the code unit index that corresponds to the specified code point index.</returns>
            int CodePointsToCodeUnits(int codePointIndex)
            {
                this.charIndex = new StringInfo(text.Substring(this.charIndex)).SubstringByTextElements(0, codePointIndex - this.codePointIndex).Length;
                this.codePointIndex = codePointIndex;
                return this.charIndex;
            }
        }
    }
}
