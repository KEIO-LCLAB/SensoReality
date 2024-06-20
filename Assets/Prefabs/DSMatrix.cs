using System;
using Oculus.Interaction;
using UnityEngine;

namespace Prefabs
{
    public class DSMatrix : IPreviewModel
    {
        
        public override Pose GetPlacementPose(Ray ray, RaycastHit hit)
        {
            var rotation = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
            rotation.x = 0;
            return new Pose(hit.point, Quaternion.Euler(rotation));
        }

    }
}
