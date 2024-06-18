using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sensor
{
    public class SensorAttachable : MonoBehaviour
    {
        [Tooltip("The parent of the placed sensor.")]
        public GameObject Parent;
        [AllowNull, Tooltip("If set, it will be active while its hovered.")]
        public GameObject HoverEffect;
        
        public virtual void OnAttachTo(VirtualSensor sensor)
        {
            if (Parent)
            {
                sensor.transform.SetParent(Parent.transform);
            }
            else
            {
                sensor.transform.SetParent(transform);
            }
        }
        
        public virtual void OnAttachHover(VirtualSensor sensor)
        {
            HoverEffect?.SetActive(true);
        }
        
        public virtual void OnAttachHoverExit(VirtualSensor sensor)
        {
            HoverEffect?.SetActive(false);
        }
    }
}
