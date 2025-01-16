using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrefabPreview : MonoBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] public Image preview;
    [SerializeField] public TextMeshProUGUI prefabName;
    
    public void Start()
    {
        if (preview == null)
        {
            preview = GetComponentInChildren<Image>();
        }
        if (prefabName == null)
        {
            prefabName = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (prefab != null && preview.sprite == null)
        {
            StartCoroutine(RenderingPreview(prefab));
        }
    }

    public void SetPrefab(GameObject prefab)
    {
        this.prefab = prefab;
        prefabName.text = prefab.name;
        StartCoroutine(RenderingPreview(prefab));
    }

    public IEnumerator RenderingPreview(GameObject modelPrefab)
    {
        if (preview == null) yield break;
        Texture2D texture2D = null;
        RuntimePreviewGenerator.OrthographicMode = true;
        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.GenerateModelPreviewAsync(tex => texture2D = tex, 
            modelPrefab.transform, shouldIgnoreParticleSystems:true, width:256, height:256);
        yield return new WaitUntil(() => texture2D != null);
        preview.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
    }
}