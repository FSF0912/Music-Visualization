using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FSF.CollectionFrame
{
    public class Lyric_Demo : MonoBehaviour
    {
        [SerializeField] private Text targetText;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private List<LyricValueKey> keys;
        [SerializeField] private TextAsset LyricAsset;

        private int currentIndex;
        private int keyCount;

        public void GetLyrics()
        {
            keys = LyricSpliter.Split(LyricAsset);
            keyCount = keys.Count;
            currentIndex = 0;
            if (keyCount > 0) targetText.text = keys[0].Lyric;
        }

        private void Start()
        {
            if (keys == null || keys.Count == 0) GetLyrics();
            else keyCount = keys.Count;
        }

        private void Update()
        {
            if (keyCount == 0 || currentIndex >= keyCount - 1) return;

            float currentTime = audioSource.time;
            int nextIndex = currentIndex + 1;
            if (currentTime >= keys[nextIndex].Time)
            {
                currentIndex = nextIndex;
                targetText.text = keys[currentIndex].Lyric;
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Lyric_Demo))]
        public class Lyric_DemoEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (!GUILayout.Button("Parse Lyrics")) return;
                
                var demo = target as Lyric_Demo;
                if (demo == null) return;
                demo.GetLyrics();
                EditorUtility.SetDirty(demo);
            }
        }
#endif
    }
}