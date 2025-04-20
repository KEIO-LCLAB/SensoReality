using UnityEngine;

namespace Prefabs.CHI
{
    [RequireComponent(typeof(Light))]
    public class BreathLight : MonoBehaviour
    {
        private Light _light;
        private float _intensity;
        public float breathSpeed = 1f;
        
        private void Awake()
        {
            _light = GetComponent<Light>();
            _intensity = _light.intensity;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_light != null)
            {
                _light.intensity = _intensity * (1 + Mathf.Sin(Time.time * breathSpeed) / 2);
            }
        }
    }
}
