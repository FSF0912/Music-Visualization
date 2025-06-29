using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using egl = UnityEditor.EditorGUILayout;
#endif

namespace FSF.CollectionFrame
{
    public enum VisualizationMode
    {
        ByAudioSource,
        ByMicroPhone
    };

    public class AudioVisualizer : MonoBehaviour
    {
        #region Variables
        public VisualizationMode visualizationMode = VisualizationMode.ByAudioSource;
        public AudioSource source;

        [Range(64, 128 * 2)]
        public int LengthSample = 128 * 2;

        [Range(0.01f, 30)]
        public float lerpSpeed = 12;
        [Min(0.01f)] public float intensity = 1f;

        [Space(15.00f)]
        public Transform[] BarsList;
        [HideInInspector] public int ElementCount = 10;
        [HideInInspector] public RectTransform ElementHolder;
        [HideInInspector] public GameObject VisualBarPrefab;
        [HideInInspector] public float ElementSplit;

        private float[] audioInfos;
        private AudioClip microRecord;
        private string device;
        private int lastOffset; // 缓存上一次的偏移量
        #endregion

        #region Methods
        private void Start()
        {
            if (BarsList.Length <= 0) { GenerateVisualizationBar(); }
            audioInfos = new float[LengthSample];
            if (visualizationMode == VisualizationMode.ByMicroPhone)
            {
                device = Microphone.devices[0];
                microRecord = Microphone.Start(device, true, 999, 44100);
                lastOffset = 0;
            }
        }

        public void GenerateVisualizationBar()
        {
            Transform[] gos = new Transform[ElementCount];
            for (int i = 0; i < gos.Length; i++)
            {
                GameObject go = Instantiate(VisualBarPrefab, ElementHolder.transform);
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(go, $"VisualizationBar {i + 1}");
#endif
                go.name = $"VisualizationBar {i + 1}";
                gos[i] = go.transform;
                
                Vector3 localScale = VisualBarPrefab.transform.localScale;
                go.transform.localPosition = new Vector3(localScale.x + ElementSplit * i, 0);
            }
            BarsList = gos;
        }

        private void Update()
        {
            switch (visualizationMode)
            {
                case VisualizationMode.ByAudioSource:
                    source.GetSpectrumData(audioInfos, 0, FFTWindow.BlackmanHarris);
                    break;
                case VisualizationMode.ByMicroPhone:
                    int currentOffset = Microphone.GetPosition(device);
                    if (currentOffset != lastOffset)
                    {
                        int validOffset = Mathf.Max(0, currentOffset - LengthSample + 1);
                        if (validOffset >= 0)
                        {
                            microRecord.GetData(audioInfos, validOffset);
                        }
                        lastOffset = currentOffset;
                    }
                    break;
            }

            for (int i = 0; i < BarsList.Length; i++)
            {
                float multiplier = 50 + i * i * 0.5f;
                float targetHeight = Mathf.Clamp(audioInfos[i] * multiplier * intensity * (visualizationMode == VisualizationMode.ByMicroPhone ? 0.2f : 1), 0, 50);
                
                Vector3 currentScale = BarsList[i].localScale;
                Vector3 targetScale = new Vector3(1, targetHeight, 1);
                
                BarsList[i].localScale = Vector3.Lerp(
                    currentScale, 
                    targetScale, 
                    Time.deltaTime * lerpSpeed
                );
            }
        }

#if UNITY_EDITOR
        public void ResetSampleValue()
        {
            LengthSample = 128 * 2;
        }
#endif
        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AudioVisualizer))]
    public class AudioVisualizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var T = target as AudioVisualizer;
            if (T == null) { return; }
            
            // 优化：使用序列化属性减少直接访问
            SerializedObject serializedObject = new SerializedObject(T);
            SerializedProperty barsProp = serializedObject.FindProperty("BarsList");
            
            if (barsProp.arraySize <= 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ElementCount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ElementHolder"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("VisualBarPrefab"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ElementSplit"));
                
                if (GUILayout.Button("Generate Visualization Bars"))
                {
                    T.GenerateVisualizationBar();
                }
            }
            
            // 优化：添加范围检查避免无效值
            if (T.LengthSample < 64 || T.LengthSample > 8192 || (T.LengthSample & (T.LengthSample - 1)) != 0)
            {
                egl.Space(20);
                using (new egl.VerticalScope(EditorStyles.helpBox))
                {
                    egl.HelpBox("LengthSample must be power of two between 64 and 8192!", MessageType.Error);
                    if (GUILayout.Button("Fix to Default"))
                    {
                        T.ResetSampleValue();
                    }
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}