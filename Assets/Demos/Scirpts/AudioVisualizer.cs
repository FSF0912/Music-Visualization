using UnityEngine.UI;
using UnityEngine;
using UnityEditor;

namespace FSF.Tools{
    public enum VisualizationMode{
        ByAudioSource,
        ByMicroPhone
    };
public class AudioVisualizer : MonoBehaviour
{
    #region Variables
    public VisualizationMode visualizationMode = VisualizationMode.ByAudioSource;
    [Space(20.00f)] public AudioSource source;

    [Range(64, 128*2)]
    public int LengthSample = 128 * 2;

    [Range(0.01f, 30)]
    public float UpLerp = 5f;

    [Min(0.01f)] public float intensity = 0.2f;

    [Space(15.00f), Header("Creatings")]
    public Image[] ImageList;
    [Min(0)] public int ElementCount = 10;
    public Transform ElementHolder;
    public GameObject VisualBarPrefab;
    public float ElementSplit;

    private float[] audioInfos;
    private AudioClip microRecord;
    string device;
#endregion
#region Methods
    private void Start()
    {
        if(ImageList.Length <= 0){GenerateVisualizationBar();}
        audioInfos = new float[LengthSample];
        if(visualizationMode == VisualizationMode.ByMicroPhone){
            device = Microphone.devices[0];
            microRecord = Microphone.Start(device, true, 999, 44100);
        }
    }

    public void GenerateVisualizationBar()
    {
        Image[] images = new Image[ElementCount];
        for (int i = 0; i < images.Length; i++)
        {
            GameObject go = Instantiate(VisualBarPrefab, ElementHolder.transform);
            #if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(go, $"VisualizationBar {i + 1}");
            #endif
            go.name = $"VisualizationBar {i + 1}";
            images[i] = go.GetComponent<Image>();
            go.transform.localPosition = new Vector3(go.transform.localScale.x + ElementSplit * i, 0);
        }
        ImageList = images;
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
        
        for (int i = 0; i < ImageList.Length; i++)
        {
            float intensityCurrent = visualizationMode == VisualizationMode.ByAudioSource ? intensity : intensity * 0.005f;
            Vector3 v3 = new Vector3(1, Mathf.Clamp(audioInfos[i] * (50 + i * i * 0.5f) * intensityCurrent, 0, 50), 1);
            ImageList[i].transform.localScale = Vector3.Lerp(ImageList[i].transform.localScale, v3, Time.deltaTime * UpLerp);
        }
    }
    #endregion
}
}