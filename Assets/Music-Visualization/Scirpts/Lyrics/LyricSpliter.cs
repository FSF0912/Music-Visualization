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
            if (string.IsNullOrEmpty(lrcText))
                return new List<LyricValueKey>(0);

            var value = new List<LyricValueKey>(64);
            var builder = new StringBuilder(256);
            ReadOnlySpan<char> textSpan = lrcText.AsSpan();
            int lineStart = 0;
            LyricValueKey previousKey = null;

            for (int i = 0; i <= textSpan.Length; i++)
            {
                if (i != textSpan.Length && textSpan[i] != '\n' && textSpan[i] != '\r') 
                    continue;
                
                ReadOnlySpan<char> lineSpan = ReadOnlySpan<char>.Empty;
                if (i > lineStart)
                {
                    lineSpan = textSpan.Slice(lineStart, i - lineStart).Trim();
                }
                
                lineStart = i + 1;

                // \r\n
                if (i < textSpan.Length - 1 && textSpan[i] == '\r' && textSpan[i + 1] == '\n')
                {
                    i++;
                    lineStart = i + 1;
                }

                if (lineSpan.IsEmpty || lineSpan[0] != '[')
                    continue;
                
                int timeEnd = ParseTimeTag(lineSpan, out float timeValue);
                if (timeValue < 0 || timeEnd < 0)
                    continue;

                string lyric = ExtractLyricText(lineSpan, timeEnd);
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
            if (tags == null || tags.Length < 1 || string.IsNullOrEmpty(lyric)) 
                return lyric;
                
            string[] languageParts = lyric.Split('\n');
            for (int i = 0; i < languageParts.Length; i++)
            {
                if (string.IsNullOrEmpty(languageParts[i]))
                    continue;
                    
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
        private static int ParseTimeTag(ReadOnlySpan<char> line, out float time)
        {
            time = -1;
            
            int timeEnd = line.IndexOf(']');
            if (timeEnd < 2) return -1;
            
            var timeSpan = line.Slice(1, timeEnd - 1);
            return TryParseTime(timeSpan, out time) ? timeEnd : -1;
        }
        
        /// <summary>
        /// Extracts lyric text after the time tags
        /// </summary>
        /// <param name="line">Full lyric line</param>
        /// <param name="timeEnd">End position of the time tag</param>
        /// <returns>Cleaned lyric text</returns>
        private static string ExtractLyricText(ReadOnlySpan<char> line, int timeEnd)
        {
            if (line.Length <= timeEnd + 1)
                return string.Empty;
                
            return line.Slice(timeEnd + 1).Trim().ToString();
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
        private static bool TryParseTime(ReadOnlySpan<char> timeStr, out float time)
        {
            time = -1;
            
            // skip non-time tags（[ar:Artist]）
            if (timeStr.Length < 4)
                return false;
                
            int colonPos = timeStr.IndexOf(':');
            if (colonPos <= 0 || colonPos >= timeStr.Length - 1)
                return false;

            // parse minutes
            if (!int.TryParse(timeStr.Slice(0, colonPos), out int minutes) || minutes < 0)
                return false;

            // parse seconds and optional milliseconds
            ReadOnlySpan<char> secondsPart = timeStr.Slice(colonPos + 1);
            
            // search for decimal point
            int dotPos = secondsPart.IndexOf('.');
            float seconds;
            float milliseconds = 0f;

            if (dotPos >= 0)
            {
                // parse seconds part
                if (!float.TryParse(secondsPart.Slice(0, dotPos), out seconds) || 
                    seconds < 0 || seconds >= 60)
                    return false;
                
                // parse milliseconds part
                if (dotPos + 1 < secondsPart.Length)
                {
                    var millisSpan = secondsPart.Slice(dotPos + 1);
                    if (millisSpan.Length > 0)
                    {
                        if (!int.TryParse(millisSpan, out int millisValue) || millisValue < 0)
                            return false;
                        
                        // Convert to fractional seconds
                        milliseconds = millisValue / (float)Math.Pow(10, millisSpan.Length);
                    }
                }
            }
            else
            {
                // no decimal point, parse whole seconds
                if (!float.TryParse(secondsPart, out seconds) || 
                    seconds < 0 || seconds >= 60)
                    return false;
            }

            time = minutes * 60 + seconds + milliseconds;
            return true;
        }
        
        #endregion
    }
}