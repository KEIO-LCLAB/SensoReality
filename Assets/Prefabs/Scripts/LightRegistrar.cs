using System;
using Sensor;
using UnityEngine;

namespace Prefabs.Scripts
{
    [RequireComponent(typeof(Light))]
    public class LightRegistrar : MonoBehaviour
    {
        private Light attachedLight;

        private void Awake()
        {
            attachedLight = GetComponent<Light>();
            if (attachedLight == null)
            {
                Debug.LogError("LightRegistrar requires a Light component to be attached to the same GameObject.");
            }
        }
        
        private void OnEnable()
        {
            LightManager.RegisterLight(attachedLight);
        }
        
        private void OnDisable()
        {
            LightManager.UnregisterLight(attachedLight);
        }
        
        private void OnDestroy()
        {
            LightManager.UnregisterLight(attachedLight);
        }
    }
}
