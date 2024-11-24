using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using egl = UnityEditor.EditorGUILayout;
#endif

namespace FSF.CollectionFrame{
        public enum VisualizationMode{
            ByAudioSource,
            ByMicroPhone
        };
    public class AudioVisualizer : MonoBehaviour
    {
        #region Variables
        public VisualizationMode visualizationMode = VisualizationMode.ByAudioSource;
        public AudioSource source;

        [Range(64, 128*2)]
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
        string device;
    #endregion
    #region Methods
        private void Start()
        {
            if(BarsList.Length <= 0){GenerateVisualizationBar();}
            audioInfos = new float[LengthSample];
            if(visualizationMode == VisualizationMode.ByMicroPhone){
                device = Microphone.devices[0];
                microRecord = Microphone.Start(device, true, 999, 44100);
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
                go.transform.localPosition = new Vector3(go.transform.localScale.x + ElementSplit * i, 0);
            }
            BarsList = gos;
        }
    
        private void Update()
        {   
            switch(visualizationMode){
                case VisualizationMode.ByAudioSource:
                    source.GetSpectrumData(audioInfos, 0, FFTWindow.BlackmanHarris);
                break;
                case VisualizationMode.ByMicroPhone:
                    int offset = Microphone.GetPosition(device) - LengthSample + 1;
                    if(offset < 0f){break;}
                    microRecord.GetData(audioInfos, offset);
                break;                
            }

            for (int i = 0; i < BarsList.Length; i++)
            {
                Vector3 v3 = new Vector3(1, Mathf.Clamp(audioInfos[i] * (50 + i * i * 0.5f) * intensity, 0, 50), 1);
                BarsList[i].transform.localScale = Vector3.Lerp(BarsList[i].transform.localScale, v3, Time.deltaTime * lerpSpeed);
            }
        }

    #if UNITY_EDITOR
        public void ResetSampleValue(){
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
            if(T==null){return;}
            if(T.BarsList.Length <= 0){
                T.ElementCount = egl.IntField("Element Count", T.ElementCount);
                T.ElementHolder = (RectTransform)egl.ObjectField("Element Holder", T.ElementHolder, typeof(RectTransform), true); 
                T.VisualBarPrefab = (GameObject)egl.ObjectField("Visual Bar Prefab", T.VisualBarPrefab, typeof(GameObject), false);
                T.ElementSplit = egl.FloatField("Element Split", T.ElementSplit);
                if(GUILayout.Button("GenerateVisualizationBar")){
                    T.GenerateVisualizationBar();
                }
            }
            if(T.LengthSample % 64 != 0){
                egl.Space(20);
                using(new egl.VerticalScope(EditorStyles.helpBox)){
                    egl.HelpBox("Length of sample buffer must be a power of two between 64 and 8192 !",MessageType.Error);
                    if(GUILayout.Button("ResetSampleValue")){
                        T.ResetSampleValue();
                    }
                }
            }
        }
    }
    #endif
}
