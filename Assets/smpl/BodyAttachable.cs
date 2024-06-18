using Sensor;
using UnityEngine;

namespace smpl
{
    public class BodyAttachable : SensorAttachable
    {
        private Transform FindNearestBone(Vector3 position)
        {
            var bonesColliders = Parent.GetComponentsInChildren<Collider>();
            Collider nearestBone = null;
            var nearestDistance = float.MaxValue;
            foreach (var boneCollider in bonesColliders)
            {
                var lastState = boneCollider.enabled;
                boneCollider.enabled = true;
                var distance = Vector3.Distance(boneCollider.ClosestPoint(position), position);
                boneCollider.enabled = lastState;
                if (distance < nearestDistance)
                {
                    nearestBone = boneCollider;
                    nearestDistance = distance;
                }
                if (distance < 0.00001f)
                {
                    break;
                }
            }
            if (nearestBone == null)
            {
                return Parent.transform;
            }
            return nearestBone.transform.parent;
        }
        
        public override void OnAttachTo(VirtualSensor sensor)
        {
            sensor.transform.SetParent(FindNearestBone(sensor.transform.position));
        }
    }
}
