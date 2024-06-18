using System.Collections.Generic;
using UnityEngine;

namespace Sensor
{
    public class SensorStateController : MonoBehaviour
    {
        public bool IsPreviewing;
        
        //runtime
        private readonly List<VirtualSensor> sensors = new();
        
        // Start is called before the first frame update
        void Start()
        {
            sensors.AddRange(GetComponentsInChildren<VirtualSensor>());
            if (IsPreviewing)
            {
                sensors.ForEach(sensor => sensor.ShowPreview = true);
            }
        }

    }
}
