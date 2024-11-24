using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;

namespace FSF.CollectionFrame{
    [ScriptedImporter(1, "lrc")]
    public class LyricImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx){
            string lrcPath = ctx.assetPath;
            string lrcText = File.ReadAllText(ctx.assetPath);
            TextAsset textAsset = new TextAsset(lrcText);
            EditorApplication.delayCall += () =>{
            AssetDatabase.DeleteAsset(lrcPath);
            AssetDatabase.CreateAsset(textAsset, lrcPath.Replace("lrc", "asset"));
            };
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }
    }
}
