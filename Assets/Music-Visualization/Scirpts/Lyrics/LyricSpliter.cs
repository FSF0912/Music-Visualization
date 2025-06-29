using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace FSF.CollectionFrame
{
    /// <summary>
    /// Serializable class representing a single lyric entry with its associated time
    /// </summary>
    [System.Serializable]
    public class LyricValueKey
    {
        /// <summary>
        /// The lyric text (supports multi-line text)
        /// </summary>
        [TextArea(5, int.MaxValue)] public string Lyric;
        
        /// <summary>
        /// The playback time (in seconds) for this lyric entry
        /// </summary>
        public float Time;
        
        /// <summary>
        /// Creates a new lyric key
        /// </summary>
        /// <param name="lyric">Lyric text content</param>
        /// <param name="time">Playback time in seconds</param>
        public LyricValueKey(string lyric, float time)
        {
            Lyric = lyric;
            Time = time;
        }
    }
    
    /// <summary>
    /// Static class for parsing LRC-formatted lyrics with multi-language support and rich text formatting
    /// </summary>
    public static class LyricSpliter
    {
        /// <summary>
        /// Splits lyrics from a TextAsset with language-specific formatting
        /// </summary>
        /// <param name="lrc">TextAsset containing LRC formatted lyrics</param>
        /// <param name="reverse">Determines language order processing:
        /// - When false: First language uses richTextSymbols[0], second uses richTextSymbols[1]
        /// - When true: Languages are processed in reverse order</param>
        /// <param name="richTextSymbols">Rich text formatting patterns for each language line:
        /// - Patterns must contain '*' as a placeholder for text
        /// - Example: ["<b>*</b>", "<i>*</i>"] would make first line bold and second line italic</param>
        /// <returns>List of parsed lyric entries</returns>
        public static List<LyricValueKey> Split(TextAsset lrc, bool reverse = false, params string[] richTextSymbols)
        {
            return Split(lrc.text, reverse, richTextSymbols);
        }

        /// <summary>
        /// Splits lyrics from a string with language-specific formatting
        /// </summary>
        /// <param name="lrc">String containing LRC formatted lyrics</param>
        /// <param name="reverse">Determines language order processing</param>
        /// <returns>List of parsed lyric entries</returns>
        public static List<LyricValueKey> Split(string lrc, bool reverse = false)
        {
            return Split(lrc, reverse, Array.Empty<string>());
        }
        
        /// <summary>
        /// Core parsing method with language-specific rich text formatting
        /// </summary>
        /// <param name="lrcText">Raw lyric text content</param>
        /// <param name="reverse">Determines order of language processing</param>
        /// <param name="richTextSymbols">Rich text format patterns for each language</param>
        /// <returns>List of parsed lyric entries</returns>
        public static List<LyricValueKey> Split(string lrcText, bool reverse, params string[] richTextSymbols)
        {
            var value = new List<LyricValueKey>(64);
            var builder = new StringBuilder(256);
            int lineStart = 0;
            LyricValueKey previousKey = null;

            for (int i = 0; i <= lrcText.Length; i++)
            {
                if (i != lrcText.Length && lrcText[i] != '\n' && lrcText[i] != '\r') 
                    continue;
                
                string line = string.Empty;
                if (i > lineStart)
                {
                    line = lrcText.Substring(lineStart, i - lineStart).Trim();
                }
                
                lineStart = i + 1;

                if (i < lrcText.Length - 1 && lrcText[i] == '\r' && lrcText[i+1] == '\n')
                {
                    i++;
                    lineStart = i + 1;
                }

                if (string.IsNullOrEmpty(line) || line[0] != '[')
                    continue;
                
                int timeEnd = ParseTimeTag(line, out float timeValue);
                if (timeValue < 0 || timeEnd < 0)
                    continue;

                string lyric = ExtractLyricText(line, timeEnd);
                lyric = ApplyLanguageTags(lyric, richTextSymbols, reverse);
                
                var currentKey = new LyricValueKey(lyric, timeValue);
                if (previousKey != null && Mathf.Approximately(previousKey.Time, timeValue))
                {
                    MergeLyrics(previousKey, currentKey, reverse, builder);
                }
                else
                {
                    value.Add(currentKey);
                    previousKey = currentKey;
                }
            }
            
            return value;
        }
        
        /// <summary>
        /// Applies language-specific rich text tags to each lyric line
        /// </summary>
        /// <param name="lyric">Raw lyric text with multiple languages (separated by newlines)</param>
        /// <param name="tags">Formatting patterns (must contain '*' as placeholder)</param>
        /// <param name="reverse">Indicates language order:
        /// - false: [Language1, Language2] -> [tags[0], tags[1]]
        /// - true: [Language2, Language1] -> [tags[0], tags[1]]</param>
        /// <returns>Formatted lyric with rich text tags applied</returns>
        private static string ApplyLanguageTags(string lyric, string[] tags, bool reverse)
        {
            if (tags == null || tags.Length < 1) 
                return lyric;
            string[] languageParts = lyric.Split('\n');
            for (int i = 0; i < languageParts.Length; i++)
            {
                int tagIndex;
                if (reverse)
                {
                    tagIndex = languageParts.Length - 1 - i;
                }
                else
                {
                    tagIndex = i;
                }
                if (tagIndex < tags.Length && !string.IsNullOrEmpty(tags[tagIndex]))
                {
                    string pattern = tags[tagIndex];
                    int asteriskPos = pattern.IndexOf('*');
                    
                    if (asteriskPos >= 0)
                    {
                        string startTag = pattern.Substring(0, asteriskPos);
                        string endTag = pattern.Substring(asteriskPos + 1);
                        
                        languageParts[i] = $"{startTag}{languageParts[i]}{endTag}";
                    }
                    else
                    {
                        languageParts[i] = $"{pattern}{languageParts[i]}{pattern}";
                    }
                }
            }
            
            return string.Join("\n", languageParts);
        }
        
        #region Utility Methods
        
        /// <summary>
        /// Parses the time tag from a lyric line
        /// </summary>
        /// <param name="line">Full lyric line with time tag</param>
        /// <param name="time">Output parameter for parsed time (in seconds)</param>
        /// <returns>End position of the time tag, or -1 on error</returns>
        private static int ParseTimeTag(string line, out float time)
        {
            time = -1;
            
            int timeEnd = line.IndexOf(']');
            if (timeEnd < 2) return -1;
            string timeStr = line[1..timeEnd];
            return TryParseTime(timeStr, out time) ? timeEnd : -1;
        }
        
        /// <summary>
        /// Extracts lyric text after the time tags
        /// </summary>
        /// <param name="line">Full lyric line</param>
        /// <param name="timeEnd">End position of the time tag</param>
        /// <returns>Cleaned lyric text</returns>
        private static string ExtractLyricText(string line, int timeEnd)
        {
            return (line.Length > timeEnd + 1) ? 
                line.Substring(timeEnd + 1).Trim() : 
                string.Empty;
        }
        
        /// <summary>
        /// Merges two lyrics with the same timestamp
        /// </summary>
        /// <param name="existing">Existing lyric entry</param>
        /// <param name="current">New lyric to merge</param>
        /// <param name="reverse">Merge order (false = current before existing)</param>
        /// <param name="builder">Reusable StringBuilder for efficient string operations</param>
        private static void MergeLyrics(LyricValueKey existing, LyricValueKey current, bool reverse, StringBuilder builder)
        {
            builder.Clear();
            
            if (reverse)
            {
                builder.Append(current.Lyric).Append('\n').Append(existing.Lyric);
            }
            else
            {
                builder.Append(existing.Lyric).Append('\n').Append(current.Lyric);
            }
            
            existing.Lyric = builder.ToString();
        }
        
        /// <summary>
        /// Parses time string in [mm:ss.xx] format
        /// </summary>
        /// <param name="timeStr">Time string to parse</param>
        /// <param name="time">Output parameter for converted time (in seconds)</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        private static bool TryParseTime(string timeStr, out float time)
        {
            time = -1;
            ReadOnlySpan<char> span = timeStr.AsSpan();
            int colonPos = span.IndexOf(':');
            if (colonPos < 1) return false;
            
            // Parse minutes component
            if (!float.TryParse(span.Slice(0, colonPos), out float minutes))
                return false;
            
            // Process seconds component
            ReadOnlySpan<char> secondsPart = span.Slice(colonPos + 1);
            int dotPos = secondsPart.IndexOf('.');
            
            // Handle milliseconds format [mm:ss.xx]
            if (dotPos >= 0)
            {
                // Parse whole seconds
                if (!float.TryParse(secondsPart.Slice(0, dotPos), out float seconds))
                    return false;
                    
                // Parse milliseconds fraction
                if (!float.TryParse(secondsPart.Slice(dotPos + 1), out float milliseconds))
                    return false;
                    
                // Combine all components
                time = minutes * 60 + seconds + milliseconds / 100f;
                return true;
            }
            else
            {
                // Handle simple format [mm:ss]
                if (!float.TryParse(secondsPart, out float seconds))
                    return false;
                    
                time = minutes * 60 + seconds;
                return true;
            }
        }
        
        #endregion
    }
}