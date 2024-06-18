using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.interactables.Model
{
    public class ModelList : MonoBehaviour
    {
        [SerializeField] private List<GameObject> models = new();
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private PrefabPreview prefabPreview;
        [SerializeField] private ModelPlacement modelPlacement;
        
        // runtime
        [AllowNull] private GameObject selectedModel;
        
        public List<GameObject> Models => models;
        public GameObject SelectedModel => selectedModel;
        
        // Start is called before the first frame update
        void Start()
        {
            foreach (var model in models)
            {
                var copiedToggle = Instantiate(prefabPreview, toggleGroup.transform);
                copiedToggle.gameObject.SetActive(true);
                copiedToggle.SetPrefab(model);
                if (copiedToggle.TryGetComponent<Toggle>(out var toggle))
                {
                    toggle.onValueChanged.AddListener(isOn =>
                    {
                        if (isOn)
                        {
                            selectedModel = model;
                            modelPlacement.SetModelPrefab(model);
                        }
                        else if (selectedModel == model)
                        {
                            selectedModel = null;
                            modelPlacement.SetModelPrefab(null);
                        }
                    });
                }
            }
        }
    }
}
