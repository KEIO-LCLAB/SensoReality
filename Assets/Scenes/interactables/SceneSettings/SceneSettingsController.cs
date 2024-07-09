using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace Scenes.interactables.SceneSettings
{
    public class SceneSettingsController : MonoBehaviour
    {
        [SerializeField] private OVRPassthroughLayer _passthroughLayer;
        [SerializeField] private EffectMesh _customEffect;
        [SerializeField] private EffectMesh _globalEffect;
        [SerializeField] private Material _sceneMaterial;
        
        public void OnBrightnessChanged(float brightness)
        {
            _passthroughLayer?.SetBrightnessContrastSaturation(brightness);
        }
        
        public void OnLightBlendFactorChanged(float blendFactor)
        {
            _sceneMaterial?.SetFloat("_HighlightOpacity", blendFactor);
        }
        
        public void ChangeSceneMeshMode(bool useGlobalMesh)
        {
            if (useGlobalMesh)
            {
                _globalEffect?.CreateMesh();
                _customEffect?.DestroyMesh();
            }
            else
            {
                _customEffect?.CreateMesh();
                _globalEffect?.DestroyMesh();
            }
        }
    }
}
