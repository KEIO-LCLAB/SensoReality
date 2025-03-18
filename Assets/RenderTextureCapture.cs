using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RenderTextureCapture))]
public class RenderTextureCaptureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var capture = (RenderTextureCapture)target;

        if (GUILayout.Button("Save RenderTexture To PNG"))
        {
            capture.SaveRenderTextureToPNG();
        }
    }
}

        
public class RenderTextureCapture : MonoBehaviour
{
    public RenderTexture renderTexture;   // 与摄像机关联的 RenderTexture

    public void SaveRenderTextureToPNG()
    {
        if (renderTexture == null) return;
        // 创建一个与 RenderTexture 大小和格式匹配的 Texture2D
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // 将当前激活的 RenderTexture 保存
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // 读取 RenderTexture 像素数据
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        // 恢复之前的激活 RenderTexture
        RenderTexture.active = previous;

        // 将 Texture2D 编码为 PNG 格式字节数组
        byte[] pngData = tex.EncodeToPNG();

        var path = EditorUtility.SaveFilePanel("Save texture as PNG", "", renderTexture.name, "png");
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllBytes(path, pngData);
        }
    }
}
