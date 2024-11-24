using System.Collections.Generic;
using UnityEngine;

namespace FSF.CollectionFrame{
    [System.Serializable]
    public class LyricValueKey{
        [TextArea(5,int.MaxValue)] public string Lyric;
        public float Time;
        public LyricValueKey(string Lyric, float Time){
            this.Lyric = Lyric;
            this.Time = Time;
        }
    }
    
    public static class LyricSpliter
    {
        #if false
        一般来说，我们下载的歌词如果有翻译的话，一般<都排在原文之后>。
        控制reverse参数可以决定分解出来的歌词是翻译歌词排在前面还是原文歌词排在前面。
        如果为true,原文歌词将会排在前面；反之排在后面。
        (此规则只适用于文字里提到的歌词排列顺序，如果你下载的歌词排列顺序不同，请根据实际情况调整。)
    

        ↓↓↓translated by Google Translate↓↓↓

        Generally speaking, if the lyrics we download have translations, 
        they are usually <sorted after the original>.

        Controlling the reverse parameter can determine whether the decomposed lyrics are sorted first,
        the translated lyrics or the original lyrics.

        If true, the original lyrics will be sorted first; otherwise, they will be sorted last.
        (This rule only applies to the order of lyrics mentioned in the text.,
        If the order of lyrics you downloaded is different, please adjust it according to the actual situation.)


        一般的に、ダウンロードした歌詞に翻訳がある場合、通常は<原文の後にランク付けされます>。

        逆パラメータを制御することで、分解された歌詞が翻訳された歌詞かオリジナルの歌詞のどちらが 1 位にランクされるかを決定できます。

        true の場合、元の歌詞が最初にランク付けされます。それ以外の場合は、元の歌詞が後にランク付けされます。
        (このルールは本文中に記載されている歌詞の順序にのみ適用されます。
        ダウンロードした歌詞の順序が異なる場合は、実際の状況に応じて調整してください。)
        #endif

        /// <summary>
        /// Split the lyric from txt.
        /// <para>Detailed summary in file.</para>
        /// </summary>
        /// <param name="lrc"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static List<LyricValueKey> Split(TextAsset lrc, bool reverse = false){
            var texts = lrc.text.Split('\n');
            List<LyricValueKey> value = new List<LyricValueKey>();
            LyricValueKey temp = null;
            for(int i = 0; i < texts.Length; i++){
                #region Time processing region
                var timeStr = texts[i].Remove(texts[i].IndexOf(']') + 1);
                timeStr = timeStr.Replace("[","");
                timeStr = timeStr.Replace("]","");
                var times = timeStr.Split(':');
                string Time1Str = times[0], Time2Str = times[1];
                if (!float.TryParse(Time1Str, out float Time1)){return null;}
                if (!float.TryParse(Time2Str, out float Time2)){return null;}
                float timeResult = Time1 * 60 + Time2;
                #endregion
                #region Lyric processing region
                string Lyric = texts[i].Remove(0, texts[i].IndexOf(']') + 1);
                #endregion
                var singleKey = new LyricValueKey(Lyric, timeResult);
                if(temp != null){
                    if(temp.Time == singleKey.Time){
                        temp.Lyric = reverse ? 
                        $"{temp.Lyric}\n{singleKey.Lyric}" : $"{singleKey.Lyric}\n{temp.Lyric}";
                    }
                    else{
                        value.Add(singleKey);
                        temp = singleKey;
                    }
                }
                else{
                    value.Add(singleKey);
                    temp = singleKey;
                }
            }
            return value;
        }

        /// <summary>
        /// Split the lyric from string.
        /// <para>Detailed summary in file.</para>
        /// </summary>
        /// <param name="lrc"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static List<LyricValueKey> Split(string lrc, bool reverse = false){
            var texts = lrc.Split('\n');
            List<LyricValueKey> value = new List<LyricValueKey>();
            LyricValueKey temp = null;
            for(int i = 0; i < texts.Length; i++){
                #region Time processing region
                var timeStr = texts[i].Remove(texts[i].IndexOf(']') + 1);
                timeStr = timeStr.Replace("[","");
                timeStr = timeStr.Replace("]","");
                var times = timeStr.Split(':');
                string Time1Str = times[0], Time2Str = times[1];
                if (!float.TryParse(Time1Str, out float Time1)){return null;}
                if (!float.TryParse(Time2Str, out float Time2)){return null;}
                float timeResult = Time1 * 60 + Time2;
                #endregion
                #region Lyric processing region
                string Lyric = texts[i].Remove(0, texts[i].IndexOf(']') + 1);
                #endregion
                var singleKey = new LyricValueKey(Lyric, timeResult);
                if(temp != null){
                    if(temp.Time == singleKey.Time){
                        temp.Lyric = reverse ? 
                        $"{temp.Lyric}\n{singleKey.Lyric}" : $"{singleKey.Lyric}\n{temp.Lyric}";
                    }
                    else{
                        value.Add(singleKey);
                        temp = singleKey;
                    }
                }
                else{
                    value.Add(singleKey);
                    temp = singleKey;
                }
            }
            return value;
        }
    }
}
