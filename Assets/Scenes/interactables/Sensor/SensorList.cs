using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.interactables.Sensor
{
    public class SensorList : MonoBehaviour
    {
        [SerializeField] private List<GameObject> sensors = new();
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private PrefabPreview prefabPreview;
        [SerializeField] private GameObject sensorPalmPreview;
    
        // runtime
        [AllowNull] private GameObject selectedSensor;
        [AllowNull] private SensorPalmPreview sensorPalmPreviewInstance;
    
        public List<GameObject> Sensors => sensors;
        public GameObject SelectedSensor => selectedSensor;

        // Start is called before the first frame update
        void Start()
        {
            foreach (var sensor in sensors)
            {
                var copiedToggle = Instantiate(prefabPreview, toggleGroup.transform);
                copiedToggle.gameObject.SetActive(true);
                copiedToggle.SetPrefab(sensor);
                if (copiedToggle.TryGetComponent<Toggle>(out var toggle))
                {
                    toggle.onValueChanged.AddListener(isOn =>
                    {
                        if (isOn)
                        {
                            selectedSensor = sensor;
                            if (sensorPalmPreviewInstance != null)
                            {
                                Destroy(sensorPalmPreviewInstance.gameObject);
                            }
                            sensorPalmPreviewInstance = Instantiate(sensorPalmPreview, transform).GetComponent<SensorPalmPreview>();
                            sensorPalmPreviewInstance.Setup(sensor);
                        }
                        else if (selectedSensor == sensor)
                        {
                            selectedSensor = null;
                            if (sensorPalmPreviewInstance != null)
                            {
                                Destroy(sensorPalmPreviewInstance.gameObject);
                            }
                        }
                    });
                }
            }
        }
    }
}
