using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FSF.Tools{
    [CustomEditor(typeof(AudioVisualizer))]
    public class AudioVisualizer_Button : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("GenerateVisualizationBar(Use Ctrl+Z to undo)."))
            {
                (target as AudioVisualizer).GenerateVisualizationBar();
            }
        }
    }

    [CustomEditor(typeof(Lyric_Demo))]
    public class Lyric_Demo_Button : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("GetLyrics"))
            {
                (target as Lyric_Demo).GetLyrics();
            }
        }
    }
}
