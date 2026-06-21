using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor.AssetImporters;

[ScriptedImporter(1, "lrc")]
public class LrcImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        byte[] bytes = File.ReadAllBytes(ctx.assetPath);
        string text = DecodeText(bytes);

        TextAsset textAsset = new TextAsset(text);

        // "main" 是子资源标识符,SetMainObject 决定了双击/拖拽这个文件时拿到的是哪个对象
        ctx.AddObjectToAsset("main", textAsset);
        ctx.SetMainObject(textAsset);
    }

    /// <summary>
    /// 国内很多 .lrc 歌词文件不是 UTF-8 而是 GBK/GB2312 编码,
    /// 这里做一个简单的探测:优先 UTF-8(含 BOM),失败则尝试 GB18030,最后兜底用系统默认编码。
    /// </summary>
    private static string DecodeText(byte[] bytes)
    {
        // 带 UTF-8 BOM
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }

        // 严格模式尝试 UTF-8,解析失败说明大概率不是 UTF-8
        try
        {
            return new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            try
            {
                return Encoding.GetEncoding("GB18030").GetString(bytes);
            }
            catch
            {
                // 实在不行就用系统默认编码兜底,保证至少不报错
                return Encoding.Default.GetString(bytes);
            }
        }
    }
}
