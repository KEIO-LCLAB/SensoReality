using System.Diagnostics.CodeAnalysis;
using Oculus.Interaction.Input;
using Sensor;
using UnityEngine;

namespace Scenes.interactables.Sensor
{
    public class SensorPlacement : MonoBehaviour
    {
        
        // runtime
        private VirtualSensor sensor;
        [AllowNull]
        private SensorAttachable sensorAttachableCollided;
        private bool shouldTransform;
        [AllowNull]
        private IHand hand;
        
        public void SetSensor(VirtualSensor virtualSensor, IHand hand, bool shouldUseTransform = false)
        {
            sensor = virtualSensor;
            shouldTransform = shouldUseTransform;
            this.hand = hand;
        }
        
        void Update()
        {
            var rightHand = DevicesRef.Instance.RightHand;
            
            if (!rightHand.GetJointPose(HandJointId.HandIndex3, out var indexPose)) return;
            if (!rightHand.GetJointPose(HandJointId.HandThumb3, out var thumbPose)) return;
            // if is not pinching, release the sensor
            var distance = Vector3.Distance(indexPose.position, thumbPose.position);
            if (distance > 0.04f)
            {
                OnHandRelease();
            }
            else if (shouldTransform)
            {
                sensor.transform.position = (indexPose.position + thumbPose.position) / 2;
            }
        }

        private void OnHandRelease()
        {
            if (sensorAttachableCollided)
            {
                sensorAttachableCollided.OnAttachHoverExit(sensor);
                sensorAttachableCollided.OnAttachTo(sensor);
            }
            Destroy(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out SensorAttachable sensorAttachable))
            {
                if (!sensorAttachable.CanAttachTo(sensor, hand))
                {
                    return;
                }
                if (sensorAttachableCollided != null)
                {
                    sensorAttachableCollided.OnAttachHoverExit(sensor);
                }
                sensorAttachableCollided = sensorAttachable;
                sensorAttachableCollided.OnAttachHover(sensor);
            }
        }
        
        // private void OnTriggerExit(Collider other)
        // {
        //     if (sensorAttachableCollided == null) return;
        //     sensorAttachableCollided.OnAttachHoverExit(sensor);
        //     sensorAttachableCollided = null;
        // }

    }
}
