using System.Collections.Generic;
using UnityEngine;

namespace FSF.Tools{
    [System.Serializable]
    public class LyricValueKey{
        [TextArea] public string Lyric;
        public double Time;
        public LyricValueKey(string Lyric, double Time){
            this.Lyric = Lyric;
            this.Time = Time;
        }
    }
    
    public static class LyricSpliter
    {
        public static List<LyricValueKey> Split(TextAsset lrc){
            var texts = lrc.text.Split('\n');
            List<LyricValueKey> value = new();
            LyricValueKey temp = null;
            for(int i = 0; i < texts.Length; i++){
                #region Time processing region
                var timeStr = texts[i].Remove(texts[i].IndexOf(']') + 1);
                timeStr = timeStr.Replace("[","");
                timeStr = timeStr.Replace("]","");
                var times = timeStr.Split(':');
                string Time1Str = times[0], Time2Str = times[1];
                if (!double.TryParse(Time1Str, out double Time1)){return null;}
                if (!double.TryParse(Time2Str, out double Time2)){return null;}
                double timeResult = Time1 * 60 + Time2;
                #endregion
                #region Lyric processing region
                string Lyric = texts[i].Remove(0, texts[i].IndexOf(']') + 1);
                #endregion
                var singleKey = new LyricValueKey(Lyric, timeResult);
                if(temp != null){
                    if(temp.Time == singleKey.Time){
                        temp.Lyric = $"{singleKey.Lyric}\n{temp.Lyric}";
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
