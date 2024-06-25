using UnityEngine;

namespace Prefabs.Scripts
{
    public class DSMatrix : IPreviewModel
    {
        [SerializeField] private GameObject MatrixVisualization;
        
        public override Pose GetPlacementPose(Ray ray, RaycastHit hit)
        {
            var rotation = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
            rotation.x = 0;
            return new Pose(hit.point, Quaternion.Euler(rotation));
        }

        public override void OnPlaced()
        {
            base.OnPlaced();
            MatrixVisualization.SetActive(true);
        }
    }
}
