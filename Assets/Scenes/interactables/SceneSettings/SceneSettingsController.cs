using UnityEngine;

namespace Scenes.interactables.SceneSettings
{
    public class SceneSettingsController : MonoBehaviour
    {
        [SerializeField] private OVRPassthroughLayer _passthroughLayer;
        
        public void OnBrightnessChanged(float brightness)
        {
            _passthroughLayer?.SetBrightnessContrastSaturation(brightness);
        }
        
    }
}
