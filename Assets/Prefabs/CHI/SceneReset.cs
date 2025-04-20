using System;
using UnityEngine;

namespace Prefabs.CHI
{
    public class SceneReset : MonoBehaviour
    {
        public void Start()
        {
            // ResetPosition();
        }

        public void ResetPosition()
        {
            var cameraRig = DevicesRef.Instance.CameraRigRef.CameraRig;
            var forward = cameraRig.centerEyeAnchor.transform.forward;
            var pos = cameraRig.centerEyeAnchor.transform.position + new Vector3(forward.x, -0.4f, forward.z) * 0.5f;
            transform.position = pos;
            // xy plane
            forward.y = 0;
            transform.rotation = Quaternion.LookRotation(-forward);
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SceneReset))]
    public class SceneResetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var sceneReset = (SceneReset) target;
            if (GUILayout.Button("Reset Position"))
            {
                sceneReset.ResetPosition();
            } else if (GUILayout.Button("Reset Scene"))
            {
                
            }
        }
    }
#endif
}
