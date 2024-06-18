using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Scenes.interactables.Model
{
    public class ModelPlacement : MonoBehaviour
    {
        private GameObject modelPrefab;
        private GameObject previewPrefab;
        
        // runtime
        private float cooldown = -1f;
        private IPreviewModel previewModel;
        
        public void SetModelPrefab([AllowNull] GameObject modelPrefab)
        {
            this.modelPrefab = modelPrefab;
            if (previewPrefab != null)
            {
                Destroy(previewPrefab);
            }
            createPreviewPrefab();
        }
        
        private void createPreviewPrefab()
        {
            if (modelPrefab != null)
            {
                previewPrefab = Instantiate(modelPrefab);
                if (previewPrefab.TryGetComponent(out previewModel))
                {
                    previewModel.SetAsPreviewView();
                }
                else
                {
                    previewModel = null;
                    foreach (var collider in previewPrefab.GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            // hand ray
            if (previewPrefab == null) return;
            var rightHand = DevicesRef.Instance.RightHand;
            if (rightHand.GetPointerPose(out var pointerPose))
            {
                var ray = new Ray(pointerPose.position, pointerPose.forward);
                if (Physics.Raycast(ray, out var hit) && hit.distance > 0.05f)
                {
                    if (previewModel != null && !previewModel.CanBePlacedAt(ray, hit)) return;
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);

                    var placementPose = previewModel == null ? 
                        new Pose(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) :
                        previewModel.GetPlacementPose(ray, hit);
                    
                    previewPrefab.transform.position = placementPose.position;
                    previewPrefab.transform.rotation = placementPose.rotation;
                    
                    // grab to spawn
                    if (cooldown < 0 && rightHand.GetIndexFingerIsPinching())
                    {
                        cooldown = 0.5f;
                        // spawn
                        var model = Instantiate(modelPrefab, hit.point, Quaternion.identity);
                        model.transform.rotation = previewPrefab.transform.rotation;
                    }

                    if (cooldown == 0 && !rightHand.GetIndexFingerIsPinching())
                    {
                        cooldown = -1;
                    }
                    
                    if (cooldown > 0)
                    {
                        cooldown = Mathf.Clamp(cooldown - Time.deltaTime, 0, 0.5f);
                    }
                }
            }
        }

        private void OnEnable()
        {
            createPreviewPrefab();
        }
        
        private void OnDisable()
        {
            if (previewPrefab != null)
            {
                Destroy(previewPrefab);
            }
        }
    }
}
