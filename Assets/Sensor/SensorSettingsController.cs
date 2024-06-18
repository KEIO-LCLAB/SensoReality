using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Sensor
{
    public class SensorSettingsController : MonoBehaviour
    {
        [SerializeField] private VirtualSensor sensor;
        [SerializeField] private Transform center;

        [SerializeField] private Toggle activeToggle;
        [SerializeField] private Toggle previewToggle;
        
        //runtime
        private float radius;

        private void Start()
        {
            if (center == null)
            {
                center = sensor.gameObject.transform;
            }
            radius = (center.position - transform.position).magnitude;
            // toggles
            UpdateToggles();
            activeToggle.onValueChanged.AddListener(isOn => SetSensorActive(!isOn));
            previewToggle.onValueChanged.AddListener(SetSensorPreview);
        }
        

        public void RemoveSensor()
        {
            Destroy(sensor.gameObject);
        }
        
        public void SetSensorActive(bool active)
        {
            sensor.IsActive = active;
        }
        
        public void SetSensorPreview(bool preview)
        {
            sensor.ShowPreview = preview;
        }

        private void UpdateToggles()
        {
            activeToggle.isOn = !sensor.IsActive;
            previewToggle.isOn = sensor.ShowPreview;
        }

        private void Update()
        {
            if (!(radius > 0)) return;
            var centerPosition = center.position;
            var eyePosition = DevicesRef.Instance.CameraRigRef.CameraRig.centerEyeAnchor.position;
            transform.position = centerPosition + radius * (eyePosition - centerPosition).normalized;
            UpdateToggles();
        }
    }
}
