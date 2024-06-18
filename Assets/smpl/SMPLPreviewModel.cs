using UnityEngine;

namespace smpl
{
    public class SMPLPreviewModel : IPreviewModel
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private Material _material;
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        
        public override void SetAsPreviewView()
        {
            _collider.enabled = false;
            _skinnedMeshRenderer.material = _material;
        }
        
        public override bool CanBePlacedAt(Ray ray, RaycastHit hit)
        {
            return true;
        }
        
        public override Pose GetPlacementPose(Ray ray, RaycastHit hit)
        {
            var dir = ray.direction;
            dir.y = 0;
            return new Pose(hit.point,  Quaternion.FromToRotation(Vector3.back, dir));
        }
    }
}
