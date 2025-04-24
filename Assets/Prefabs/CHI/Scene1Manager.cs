using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Prefabs.CHI
{
    public class Scene1Manager : MonoBehaviour
    {
        public GameObject scene1Prefab;
        protected internal GameObject Scene1Instance;
        List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
        
        public IEnumerator Start()
        {
            // execute next frame
            yield return new WaitForSeconds(1);
            Reset();
        }

        IEnumerator CreateOrLoadSpatialAnchor()
        {
            if (Scene1Instance == null)
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
            
            var anchor = Scene1Instance.AddComponent<OVRSpatialAnchor>();
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
                    if (localized && Scene1Instance != null)
                    {
                        var spatialAnchor = Scene1Instance.AddComponent<OVRSpatialAnchor>();
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
        
        public async void SaveSpatialAnchor()
        {
            if (Scene1Instance == null)
            {
                Debug.LogError("Scene1Instance is null");
                return;
            }
            var anchor = Scene1Instance.GetComponent<OVRSpatialAnchor>();
            if (anchor == null)
            {
                StartCoroutine(CreateSpatialAnchorAndSave());
                return;
            }
            
            var result = await anchor.SaveAnchorAsync();
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
            if (Scene1Instance == null) return;
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

            var oldAnchor = Scene1Instance.GetComponent<OVRSpatialAnchor>();
            if (oldAnchor != null && saveAnchor)
            {
                await OVRSpatialAnchor.EraseAnchorsAsync(new []{oldAnchor}, null);
                Destroy(oldAnchor);
            }
            Scene1Instance.transform.position = pos;
            // xy plane
            forward.y = 0;
            Scene1Instance.transform.rotation = Quaternion.LookRotation(-forward);
            Debug.Log("Reset Scnene1 Position");
            if (!saveAnchor) return;
            SaveSpatialAnchor();
        }

        IEnumerator CreateSpatialAnchorAndSave()
        {
            var anchor = Scene1Instance.AddComponent<OVRSpatialAnchor>();

            // Wait for the async creation
            yield return new WaitUntil(() => anchor.Created);

            Debug.Log($"Created anchor {anchor.Uuid}");
            SaveSpatialAnchor();
        }
        
        public void Reset()
        {
            if (Scene1Instance != null)
            {
                Destroy(Scene1Instance);
                Scene1Instance = null;
            }
            Scene1Instance = Instantiate(scene1Prefab, transform);
            ResetPosition(false);
            Debug.Log("Reset Scene1");
            StartCoroutine(CreateOrLoadSpatialAnchor());
        }
        
        public void RemoveScene()
        {
            if (Scene1Instance != null)
            {
                Destroy(Scene1Instance);
                Scene1Instance = null;
            }
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Scene1Manager))]
    public class Scene1ManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var scene1Manager = (Scene1Manager) target;
            if (GUILayout.Button("Reset Scene1"))
            {
                scene1Manager.Reset();
            } else if (scene1Manager.Scene1Instance != null)
            {
                if (GUILayout.Button("Reset Scene1 Position"))
                {
                    scene1Manager.ResetPosition();
                } else if (GUILayout.Button("Remove Scene1"))
                {
                    scene1Manager.RemoveScene();
                } else if (GUILayout.Button("Save Anchor"))
                {
                    scene1Manager.SaveSpatialAnchor();
                }
            } 
        }
    }
#endif
}
