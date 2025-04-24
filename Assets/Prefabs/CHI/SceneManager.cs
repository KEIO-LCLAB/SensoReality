using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Prefabs.CHI
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager SelectedScene = null;
        public GameObject scenePrefab;
        protected internal GameObject SceneInstance;
        List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
        
        public void SelectScene()
        {
            if (SelectedScene != null)
            {
                if (SelectedScene == this) return;
                SelectedScene.RemoveScene();
            }
            SelectedScene = this;
            Reset();
        }

        IEnumerator CreateOrLoadSpatialAnchor()
        {
            if (SceneInstance == null)
            {
                yield break;
            }
            
            // Check if the anchor is already saved
            var anchorUuid = PlayerPrefs.GetString("vsens_scene1_anchor", string.Empty);
            if (!string.IsNullOrEmpty(anchorUuid))
            {
                // Load the anchor
                var uuids = new List<Guid> { Guid.Parse(anchorUuid) };
                var task =  LoadAnchorsByUuid(uuids);
                yield return new WaitUntil(() => task.IsCompleted);
                if (task.Exception == null && task.Result)
                {
                    Debug.Log("Anchor loaded successfully.");
                    yield break;
                }
                Debug.LogError("Failed to load anchor.");
            }
            
            var anchor = SceneInstance.AddComponent<OVRSpatialAnchor>();
            // Wait for the async creation
            yield return new WaitUntil(() => anchor.Created);
            Debug.Log($"Created anchor {anchor.Uuid}");
        }
        
        private async Task<bool> LoadAnchorsByUuid(IEnumerable<Guid> uuids)
        {
            // Step 1: Load
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, _unboundAnchors);
            if (result.Success)
            {
                foreach (var unboundAnchor in result.Value)
                {
                    var localized = await unboundAnchor.LocalizeAsync();
                    if (localized && SceneInstance != null)
                    {
                        var spatialAnchor = SceneInstance.AddComponent<OVRSpatialAnchor>();
                        unboundAnchor.BindTo(spatialAnchor);
                        Debug.Log($"Localized & bound: {unboundAnchor.Uuid}");
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogError($"Load failed with error {result.Status}.");
            }

            return false;
        }
        
        public IEnumerator SaveSpatialAnchor(bool hasDestory=false)
        {
            if (SceneInstance == null)
            {
                Debug.LogError("Scene1Instance is null");
                yield break;
            }
            var anchor = SceneInstance.GetComponent<OVRSpatialAnchor>();
            if (anchor != null && hasDestory)
            {
                yield return new WaitUntil(() => SceneInstance.GetComponent<OVRSpatialAnchor>() == null);
                anchor = null;
            }
            if (anchor == null)
            {
                anchor = SceneInstance.AddComponent<OVRSpatialAnchor>();
                // Wait for the async creation
                yield return new WaitUntil(() => anchor.Created);
            }
            var task = anchor.SaveAnchorAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            var result = task.GetResult();
            if (result.Success)
            {
                PlayerPrefs.SetString("vsens_scene1_anchor", anchor.Uuid.ToString());
                Debug.Log($"Anchor {anchor.Uuid} saved successfully.");
            }
            else
            {
                Debug.LogError($"Anchor {anchor.Uuid} failed to save with error {result.Status}");
            }
        }
        
        public async void ResetPosition(bool saveAnchor=true)
        {
            if (SceneInstance == null) return;
            var cameraRig = DevicesRef.Instance.CameraRigRef.CameraRig;
            var forward = cameraRig.centerEyeAnchor.transform.forward;
            // move the object to the surface of the front 40 cm
            var pos = cameraRig.centerEyeAnchor.transform.position + new Vector3(forward.x, 0, forward.z).normalized * 0.4f;
            // move the object to the surface of the front table?
            var ray = new Ray(pos, Vector3.down);
            if (Physics.Raycast(ray, out var hitInfo, 1.5f))
            {
                pos = hitInfo.point + new Vector3(0, 0.05f, 0);
                Debug.Log($"Surface Detected");
            }
            else
            {
                // move 25 cm down should be enough?
                pos += new Vector3(0, -0.25f, 0);
            }

            if (!saveAnchor)
            {
                SceneInstance.transform.position = pos;
                // xy plane
                forward.y = 0;
                SceneInstance.transform.rotation = Quaternion.LookRotation(-forward);
                return;
            }
            var oldAnchor = SceneInstance.GetComponent<OVRSpatialAnchor>();
            if (oldAnchor != null)
            {
                await oldAnchor.EraseAnchorAsync();
                Destroy(oldAnchor);
            }
            SceneInstance.transform.position = pos;
            // xy plane
            forward.y = 0;
            SceneInstance.transform.rotation = Quaternion.LookRotation(-forward);
            Debug.Log("Reset Scnene1 Position");
            StartCoroutine(SaveSpatialAnchor(true));
        }

        public void Reset()
        {
            CHISceneManagement.ClearScene();
            if (SceneInstance != null)
            {
                Destroy(SceneInstance);
                SceneInstance = null;
            }
            SceneInstance = Instantiate(scenePrefab, transform);
            ResetPosition(false);
            Debug.Log("Reset Scene1");
            StartCoroutine(CreateOrLoadSpatialAnchor());
        }
        
        public void RemoveScene()
        {
            if (SceneInstance != null)
            {
                Destroy(SceneInstance);
                SceneInstance = null;
            }
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SceneManager))]
    public class SceneManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var scene1Manager = (SceneManager) target;
            if (GUILayout.Button("Reset Scene"))
            {
                scene1Manager.Reset();
            } else if (scene1Manager.SceneInstance != null)
            {
                if (GUILayout.Button("Reset Scene Position"))
                {
                    scene1Manager.ResetPosition();
                } else if (GUILayout.Button("Remove Scene"))
                {
                    scene1Manager.RemoveScene();
                } else if (GUILayout.Button("Save Anchor"))
                {
                    scene1Manager.StartCoroutine(scene1Manager.SaveSpatialAnchor());
                }
            } 
        }
    }
#endif
}
