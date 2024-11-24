using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace  FSF.CollectionFrame{
public class Lyric_Demo : MonoBehaviour
{
    [SerializeField] private Text targetText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<LyricValueKey> keys;
    [SerializeField] private TextAsset LyricAsset;
    bool Showing = true;
    int index = 0;

    public void GetLyrics(){
        keys = LyricSpliter.Split(LyricAsset);
    }

    private void Start() {
        if(keys.Count <= 0){GetLyrics();}
    }

    private void Update() {
        if(Showing){
            if(audioSource.time >= keys[index + 1 > keys.Count ? index : index + 1].Time){
                index++;
                targetText.text = keys[index].Lyric;
            }
        }
    }
    #if UNITY_EDITOR
    public class Lyric_DemoEditor : Editor{
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Get Lyrics")){
                var T = target as Lyric_Demo;
                if(T == null){return;}
                T.GetLyrics();
            }
        }
        }
    #endif
}}
